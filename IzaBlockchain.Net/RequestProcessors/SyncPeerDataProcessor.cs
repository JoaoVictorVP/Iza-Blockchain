using IzaBlockchain.MemDatas;
using System.Net.Sockets;

namespace IzaBlockchain.Net.RequestProcessors;

public class SyncPeerDataProcessor : PeerRequestProcessor
{
    public override string Name => "SyncPeerData";

    public override byte RequestType => (byte)CoreRequestTypes.SyncPeerData;

    public override bool Process(Span<byte> receivedData, PeerConnection fromPeer, TcpClient fromClient)
    {
        // It's receiving peers
        if(receivedData.Length > 0)
        {
#if DEBUG
            NetworkFeedback.SendFeedback("RECEIVING ALL PEER DATA", NetworkFeedback.FeedbackType.Info);
#endif
            File.WriteAllBytes(Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).FullPath, receivedData.ToArray());
        }
        // Has to send peers
        else
        {
            fromPeer.SendData((_peer, client, stream) =>
            {
#if DEBUG
                NetworkFeedback.SendFeedback("SENDING ALL PEER DATA", NetworkFeedback.FeedbackType.Info);
#endif
                stream.WriteByte((byte)CoreRequestTypes.SyncPeerData);

                var peerData = File.ReadAllBytes(Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).FullPath);

                stream.Write(peerData, 0, peerData.Length);
            });
        }

        return true;
    }
}
