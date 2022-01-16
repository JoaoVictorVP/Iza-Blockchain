using System.Net.Sockets;

namespace IzaBlockchain.Net;

public class PeerConnection
{
    public readonly Node Node;
    public readonly Peer Peer;
    public readonly Address Address;

    public TcpClient? Client;
    public TcpListener? Listener;
    public readonly Queue<SendPeerDataMethod> Pending = new Queue<SendPeerDataMethod>(32);

    public void Connect()
    {
        var ip = Peer.GetIP();
        int port = Node.GetCurrentConnectionCount();

        Client = new TcpClient();

        Listener = new TcpListener(ClientUtils.GetSelfIP(), port);

        Client.Connect(ip, port);

        Listener.Start();

        Node.IncreaseConnectionCount();
    }

    public PeerConnection(Node node, Peer peer)
    {
        Node = node;
        Peer = peer;
        Address = peer.GetPublicAddress();
    }
}
public delegate void SendPeerDataMethod(Address address, TcpClient client, NetworkStream stream);