using IzaBlockchain.MemDatas;
using System.Net.Sockets;

namespace IzaBlockchain.Net.RequestProcessors;

public class FeedPeerDataAndPropagateProcessor : PeerRequestProcessor
{
    public override string Name => "FeedPeerDataAndPropagate";

    public override byte RequestType => (byte)CoreRequestTypes.FeedPeerDataAndPropagate;

    public override bool Process(Span<byte> receivedData, PeerConnection fromPeer, TcpClient fromClient)
    {
        byte removeOrAdd = receivedData[0];

        Peer peer;
        peer.A = receivedData[1];
        peer.B = receivedData[2];
        peer.C = receivedData[3];
        peer.D = receivedData[4];

        if (removeOrAdd == 1)
        {
            if (!Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).AddPeer(peer))
                // Ends propagation on this node
                return false;
        }
        else if (removeOrAdd == 0)
        {
            if (!Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).RemovePeer(peer))
                // Ends propagation on this node
                return false;
        }

        // Propagate
        foreach(var cPeer in Node.Self.AllPeers())
        {
            // Propagate this process into connected peers
            cPeer.SendData((_peer, client, stream) =>
            {
                stream.WriteByte((byte)CoreRequestTypes.FeedPeerDataAndPropagate);

                // Remove Code
                stream.WriteByte(0);

                stream.WriteByte(peer.A);
                stream.WriteByte(peer.B);
                stream.WriteByte(peer.C);
                stream.WriteByte(peer.D);
            });
        }

        return true;
    }
}