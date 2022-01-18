using IzaBlockchain.MemDatas;
using System.Net.Sockets;

namespace IzaBlockchain.Net.RequestProcessors;

public class SyncPeerDataProcessor : PeerRequestProcessor
{
    public override string Name => "SyncPeerData";

    public override byte RequestType => (byte)CoreRequestTypes.SyncPeerData;

    public override bool Process(TcpPeer.Header receivedHeader, SpanStream receivedData, PeerConnection fromPeer, TcpClient fromClient)
    {
        // Has to send peers

        var peerData = File.ReadAllBytes(Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).FullPath);

        var stream = new SpanStream(peerData.Length);

        stream.Write(peerData);

#if DEBUG
        NetworkFeedback.SendFeedback("SENDING ALL PEER DATA", NetworkFeedback.FeedbackType.Info);
#endif

        fromPeer.Net.SendResponse(stream.AsSpan(), receivedHeader.Id);

        return true;
    }
}
