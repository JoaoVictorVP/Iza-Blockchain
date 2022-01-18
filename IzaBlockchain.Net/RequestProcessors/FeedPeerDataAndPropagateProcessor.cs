using IzaBlockchain.MemDatas;
using System.Net.Sockets;

namespace IzaBlockchain.Net.RequestProcessors;

public class FeedPeerDataAndPropagateProcessor : PeerRequestProcessor
{
    public override string Name => "FeedPeerDataAndPropagate";

    public override byte RequestType => (byte)CoreRequestTypes.FeedPeerDataAndPropagate;

    public override bool Process(TcpPeer.Header receivedHeader, SpanStream receivedData, PeerConnection fromPeer, TcpClient fromClient)
    {
        byte removeOrAdd = receivedData.ReadByte();

        Peer peer;
        peer.A = receivedData.ReadByte();
        peer.B = receivedData.ReadByte();
        peer.C = receivedData.ReadByte();
        peer.D = receivedData.ReadByte();

#if DEBUG
        NetworkFeedback.SendFeedback($"FEEDING PEER DATA ({(removeOrAdd == 1? "Adding" : "Removing")})  WITH {peer}", NetworkFeedback.FeedbackType.Info);
#endif

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
            cPeer.SendData(peer =>
            {
#if DEBUG
                NetworkFeedback.SendFeedback($"PROPAGATING PEER DATA TO: {cPeer.Peer}", NetworkFeedback.FeedbackType.Info);
#endif
                return new PeerSendDataBuilder(new SpanStream(6)
                    // Type
                    .WriteByte((byte)CoreRequestTypes.FeedPeerDataAndPropagate)

                    // Remove or Add Code
                    .WriteByte(removeOrAdd)

                    // IP
                    .WriteByte(fromPeer.Peer.A)
                    .WriteByte(fromPeer.Peer.B)
                    .WriteByte(fromPeer.Peer.C)
                    .WriteByte(fromPeer.Peer.D))
                .AsData();
            });
        }

        return true;
    }
}