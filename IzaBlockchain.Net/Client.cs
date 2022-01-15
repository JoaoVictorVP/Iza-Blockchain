using System.Net;

namespace IzaBlockchain.Net;

public class Client
{
    List<PeerConnection> connections = new List<PeerConnection>(32);

    public void Connect(Peer peer)
    {

    }
}

public class PeerConnection
{
    public readonly Peer Peer;

    public void Connect()
    {
        IPAddress.Any.
    }

    public PeerConnection(Peer peer)
    {
        Peer = peer;
    }
}