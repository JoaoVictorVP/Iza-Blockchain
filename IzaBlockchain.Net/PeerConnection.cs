using IzaBlockchain.MemDatas;
using System.Buffers.Binary;
using System.Net.Sockets;

namespace IzaBlockchain.Net;

public class PeerConnection
{
    public readonly Node Node;
    public readonly Peer Peer;
    public readonly List<Address> Addresses;
    //public readonly Address Address;

    //public TcpClient? Client;
    //public TcpListener? Listener;
    public TcpPeer Net;
    public readonly Queue<SendPeerDataMethod> Pending = new Queue<SendPeerDataMethod>(32);

    /// <summary>
    /// Enqueue peer data sending<br/>
    /// Format: (PeerConnection peer, TcpClient client, NetworkStream stream) <see cref="SendPeerDataMethod"/>
    /// </summary>
    /// <param name="sendPeerData"></param>
    public void SendData(SendPeerDataMethod sendPeerData)
    {
        Pending.Enqueue(sendPeerData);
    }

    public void Update()
    {
        Net.Update();
    }

    public void Connect()
    {

        var ip = Peer.GetIP();
        int port = Node.GetCurrentConnectionCount();

#if DEBUG
        NetworkFeedback.SendFeedback($"CONNECTING TO PEER: {ip}:{port} AT {TimeOnly.FromDateTime(DateTime.Now)}", NetworkFeedback.FeedbackType.Info);
#endif

        //Client = new TcpClient();

        //Listener = new TcpListener(ClientUtils.GetSelfIP(), port);

        //Client.Connect(ip, port);

        //Listener.Start();

        Net = new TcpPeer(ip, port);

        Net.Connect();

        Node.IncreaseConnectionCount();

        RetrieveAddresses();
    }
    public void Disconnect()
    {
        //Client.Dispose();
        //Listener.Stop();
        Net.Disconnect();

#if DEBUG
        NetworkFeedback.SendFeedback($"DISCONNECTED FROM {Peer}", NetworkFeedback.FeedbackType.Info);
#endif
    }

    void RetrieveAddresses()
    {
        Pending.Enqueue((peer) =>
        {
            return new PeerSendDataBuilder(
                new SpanStream(1)
                    .WriteByte((byte)CoreRequestTypes.RetrieveAddresses))
                .AsRequest()
                // On response from request
                .OnResponse(data =>
                {
                    var stream = new SpanStream(data);

                    //Span<byte> addressesCountBytes = receivedData.Slice(0, sizeof(int));
                    var addressesCountBytes = stream.ReadTo(sizeof(int));
                    int addressesCount = BinaryPrimitives.ReadInt32LittleEndian(addressesCountBytes);

                    for (int i = sizeof(int); i < addressesCount; i++)
                    {
                        //Span<byte> addressBytes = receivedData.Slice(i * BlockchainGenerals.AddressSize, BlockchainGenerals.AddressSize);
                        var addressBytes = stream.ReadTo(BlockchainGenerals.AddressSize);
                        var address = new Address(addressBytes);
#if DEBUG
                        NetworkFeedback.SendFeedback($"ADDRESS RETRIEVED {address}", NetworkFeedback.FeedbackType.Info);
#endif
                        Addresses.Add(address);
                    }
                });
        });
    }

    public PeerConnection(Node node, Peer peer)
    {
        Node = node;
        Peer = peer;
        Addresses = new List<Address>(32);
        // Address = peer.GetPublicAddress();
    }
}
public delegate PeerSendDataBuilder SendPeerDataMethod(PeerConnection peer);

public abstract class PeerRequestProcessor
{
    public abstract string Name { get; }
    public abstract byte RequestType { get; }

    public abstract bool Process(TcpPeer.Header receivedHeader, SpanStream receivedData, PeerConnection fromPeer, TcpClient fromClient);

    public virtual void Initialize()
    {
        Node.ProcessPeerData(new PeerDataProcessor(Name, Process, RequestType));
    }
}
public ref struct PeerSendDataBuilder
{
    TcpPeer.Header header;
    Span<byte> data;
    OnResponseMethod onResponse = null;

    public PeerSendDataBuilder OnResponse(OnResponseMethod onResponse)
    {
        this.onResponse = onResponse;
        return this;
    }

    public PeerSendDataBuilder AsRequest()
    {
        header = TcpPeer.Header.Request();
        return this;
    }
    public PeerSendDataBuilder AsResponse(ushort id)
    {
        header = TcpPeer.Header.Response(id);
        return this;
    }
    public PeerSendDataBuilder AsData()
    {
        header = TcpPeer.Header.Data();
        return this;
    }

    public PeerSendDataBuilder(Span<byte> data)
    {
        header = TcpPeer.Header.Data();
        this.data = data;
    }
    public PeerSendDataBuilder(SpanStream stream)
    {
        header = TcpPeer.Header.Data();
        data = stream.AsSpan();
    }

    /// <summary>
    /// Should be called by system
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public Span<byte> Finish(out TcpPeer.Header header)
    {
        header = this.header;
        return data;
    }

    /// <summary>
    /// Should be called by system
    /// </summary>
    /// <param name="response"></param>
    public void Response(Span<byte> response) => onResponse?.Invoke(response);

    public delegate void OnResponseMethod(Span<byte> response);
}