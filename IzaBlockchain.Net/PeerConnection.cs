using System.Net.Sockets;

namespace IzaBlockchain.Net;

public class PeerConnection
{
    public readonly Node Node;
    public readonly Peer Peer;
    public readonly List<Address> Addresses;
    //public readonly Address Address;

    public TcpClient? Client;
    public TcpListener? Listener;
    public readonly Queue<SendPeerDataMethod> Pending = new Queue<SendPeerDataMethod>(32);

    public void SendData(SendPeerDataMethod sendPeerData)
    {
        Pending.Enqueue(sendPeerData);
    }

    public void Connect()
    {
        var ip = Peer.GetIP();
        int port = Node.GetCurrentConnectionCount();

        Client = new TcpClient();

        Listener = new TcpListener(ClientUtils.GetSelfIP(), port);

        Client.Connect(ip, port);

        Listener.Start();

        Node.IncreaseConnectionCount();

        RetrieveAddresses();
    }

    void RetrieveAddresses()
    {
        Pending.Enqueue((addresses, client, stream) =>
        {
            stream.WriteByte((byte)CoreRequestTypes.RetrieveAddresses);
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
public delegate void SendPeerDataMethod(List<Address> addresses, TcpClient client, NetworkStream stream);

public abstract class PeerRequestProcessor
{
    public abstract string Name { get; }
    public abstract byte RequestType { get; }

    public abstract bool Process(Span<byte> receivedData);

    public virtual void Initialize()
    {
        Node.ProcessPeerData(new PeerDataProcessor(Name, Process, RequestType));
    }
}
