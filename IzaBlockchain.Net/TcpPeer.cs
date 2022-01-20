using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace IzaBlockchain.Net;

/// <summary>
/// Custom protocol for internet connection among peers and nodes<br/>
/// Benefits from traditional TcpClient/TcpListener only using:<br/>
///  * Ensures connectivity with constant verifications<br/>
///  * Allows for easy and arbitrary data sending and receiving without the need of handling streams while keeping benefits of Tcp protocol<br/>
///  * Supports data requesting and receiving for peer-to-peer direct communications<br/>
///  <br/>
/// It assumes the subsequent pressupositions:<br/>
///  * That any peer requesting need not to know about this peer (i.e. it is possible to that peer to be not directly connected with this peer)
///  <br/>
///  The format of data sent and received will be:<br/>
///  Header: 1 byte (type of message: 0 pure data, 1 data request, 2 data response)<br/>
///  Id (optional): 2 bytes (short) representing request and response values (used in that cases)<br/>
///  Data: X bytes (any message to be sent or received)
/// </summary>
public class TcpPeer
{
    IPEndPoint ip;
    TcpClient client;
    TcpListener server;

    public event OnReceiveDataMethod OnReceiveData;

    /// <summary>
    /// Starts this TcpPeer processing<br/>
    /// While receiving requests as a server this will handle them properly,<br/>
    /// and while sending them as a client the other peer will know how to handle them
    /// </summary>
    public void Connect()
    {
        if(!onlyClient) server.Start();
    }
    /// <summary>
    /// Disconnects TcpPeer
    /// </summary>
    public void Disconnect()
    {
        if(!onlyClient) server.Stop();
        if (!onlyServer)
        {
            client.Close();
            client.Dispose();
        }
    }

    public void EnsureClient()
    {
        if (onlyServer)
            return;

        if (!client.Connected)
            client = new TcpClient(ip);
    }

    public unsafe void Update()
    {
        // Received some data (server-side)
        if (onlyClient) return;
        if(server.Pending())
        {
            // The peer wich connected to this server (probably is the same as 'client' as port is different for each peer, but no guarantee on that)
            var connection = server.AcceptTcpClient();

            var stream = connection.GetStream();

            // Waits until data is completely delivered to server
            while (!stream.DataAvailable)
                Thread.Sleep(10);

            int size = connection.Available;

            // If size of data is bellow 1024 bytes we do use a stackalloc, fast and reliable, and if it is above of this size then we allocate memory with new .NET 6.0 malloc implementation for a faster and unmanaged memory handling
            byte* bufferPtr = null;
            Span<byte> buffer = size < 1024 ? stackalloc byte[size] : new Span<byte>(bufferPtr = (byte*)NativeMemory.Alloc((nuint)size), size);

            stream.Read(buffer);

            processReceivedData(buffer, connection, stream);

            // Case malloc was been used, then releases it from memory manually now
            if (size >= 1024)
                NativeMemory.Free(bufferPtr);
        }
    }

    #region Server-Side
    Dictionary<int, NetworkStream> requests = new Dictionary<int, NetworkStream>(32);
    void processReceivedData(Span<byte> buffer, TcpClient sender, NetworkStream stream)
    {
        byte messageType = buffer[0];
        ushort id = 0;
        if (messageType == 1 || messageType == 2)
            id = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(1, sizeof(short)));

        bool isRequest = messageType == 1;

        //if (isRequest) waitingResponse = stream;
        if(isRequest)
        {
            requests.Add(id, stream);
            // Wait for any processing event on this server to response this request
            //while (!responseSent)
            //    Thread.Sleep(1);
        }

        OnReceiveData?.Invoke(new Header { Type = (MessageType)messageType, Id = id }, buffer.Slice(1 + sizeof(short)), isRequest, sender, this);
    }

    //NetworkStream waitingResponse;
    //bool responseSent;
    /// <summary>
    /// Send response to a incoming request by using request id and a response span of bytes
    /// </summary>
    /// <param name="response">The span of bytes to send</param>
    /// <param name="requestId">The request id</param>
    public void SendResponse(Span<byte> response, ushort requestId)
    {
        if (requests.TryGetValue(requestId, out NetworkStream stream))
        {
            stream.Write(response);

            requests.Remove(requestId);
        }
    }
    #endregion

    #region Client-Side
    public void SendData(Header header, Span<byte> data)
    {
        EnsureClient();

        var stream = client.GetStream();

        // Writes type from header
        stream.WriteByte((byte)header.Type);

        if(header.Type == MessageType.Request || header.Type == MessageType.Response)
        {
            Span<byte> id = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(id, header.Id);
            stream.Write(id);
        }
        stream.Write(data);
    }
    /// <summary>
    /// Intended to be used by requesters, call and wait for response of the peer
    /// </summary>
    public unsafe Response WaitForResponse()
    {
        // Wait until client sends something
        while (client.Available == 0)
            Thread.Sleep(0);
        int size = client.Available;
        
        byte* bufferPtr = null;
        Span<byte> buffer = size < 1024 ? stackalloc byte[size] : new Span<byte>(bufferPtr = (byte*)NativeMemory.Alloc((nuint)size), size);

        var stream = client.GetStream();

        stream.Read(buffer);

        Response r = new Response(new ReadOnlySpan<byte>(bufferPtr, size), bufferPtr, size >= 1024);
        return r;
    }
    public readonly unsafe ref struct Response
    {
        public readonly ReadOnlySpan<byte> Data;
        public readonly byte* ResponsePtr;
        readonly bool malloc;

        public unsafe Span<byte> AsSpan()
        {
            byte* spanPtr = stackalloc byte[Data.Length];
            Span<byte> span = new Span<byte>(spanPtr, Data.Length);
            Data.CopyTo(span);

            return span;
        }

        /// <summary>
        /// Should be called when Response use is ended to ensure memory freeing in case of allocations
        /// </summary>
        public void Dispose()
        {
            if (malloc)
                NativeMemory.Free(ResponsePtr);
        }

        public Response(ReadOnlySpan<byte> data, byte* responsePtr, bool malloc)
        {
            Data = data;
            ResponsePtr = responsePtr;
            this.malloc = malloc;
        }
    }
    #endregion


    bool onlyServer, onlyClient;
    public TcpPeer(IPAddress ip, int port, bool onlyServer = false, bool onlyClient = false)
    {
        this.ip = new IPEndPoint(ip, port);
        if(!onlyServer)
            client = new TcpClient(this.ip);
        if(!onlyClient)
            server = new TcpListener(ClientUtils.GetSelfIP(), port);

        this.onlyServer = onlyServer;

        this.onlyClient = onlyClient;
    }

    public struct Header
    {
        public MessageType Type;
        public ushort Id;

        public static Header Data() => new Header { Type = MessageType.Data };
        public static Header Request()
        {
            Span<byte> id = stackalloc byte[sizeof(short)];
            RandomNumberGenerator.Fill(id);
            return new Header { Type = MessageType.Request, Id = BinaryPrimitives.ReadUInt16LittleEndian(id) };
        }
        public static Header Response(ushort id) => new Header { Type = MessageType.Response, Id = id };
    }
    public enum MessageType
    {
        Data = 0,
        Request = 1,
        Response = 2
    }

    /// <summary>
    /// On Receive Data Method
    /// </summary>
    /// <param name="data">The received data</param>
    /// <param name="isRequest">It is this received message a request?</param>
    /// <param name="sender">The client who sent this message/data</param>
    /// <param name="peer">The peer who triggered this event</param>
    public delegate void OnReceiveDataMethod(Header header, Span<byte> data, bool isRequest, TcpClient sender, TcpPeer peer);
}
