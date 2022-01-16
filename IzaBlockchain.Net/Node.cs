using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace IzaBlockchain.Net;

public class Node
{
    //static readonly List<PeerDataProcessor> peerProcessors = new(32);
    static readonly Dictionary<byte, PeerDataProcessor> peerProcessors = new(32);

    //public static void ProcessPeerData(PeerDataProcessor processor) => peerProcessors.Add(processor);
    public static void ProcessPeerData(PeerDataProcessor processor) => peerProcessors.Add(processor.Type, processor);

    int _conCount = BlockchainGenerals.ConnectionPort;
    public int GetCurrentConnectionCount() => _conCount;
    public void IncreaseConnectionCount() => _conCount++;

    List<PeerConnection> connections = new List<PeerConnection>(32);

    public PeerConnection GetRandomConnectedPeer() => connections[random.Next(0, connections.Count)];

    public void Connect(Peer peer)
    {
        var connected = new PeerConnection(this, peer);
        connections.Add(connected);
    }
    public bool Disconnect(Peer peer)
    {
        foreach(var connection in connections)
        {
            if(connection.Peer.GetHashCode() == peer.GetHashCode())
            {
                connection?.Client?.Dispose();
                connection?.Listener?.Stop();
                connections.Remove(connection);

                return true;
            }
        }
        return false;
    }
    readonly Random random = new Random();
    public void Initialize()
    {
        Task.Run(Run);
    }
    async void Run()
    {
        while(true)
        {
            foreach (var peer in connections)
            {
                try
                {
                    RunPeer(peer);
                }
                catch(Exception exc)
                {
                    NetworkFeedback.SendFeedback($"Peer ({peer.Address}; {peer.Peer.GetIP()}) Error: " + exc.Message, NetworkFeedback.FeedbackType.Error);
                }
            }

            await Task.Delay(10);
        }
    }
    async void RunPeer(PeerConnection peer)
    {
        if (peer == null || peer.Client == null || peer.Listener == null) return;
        // Read incoming peer data
        var listener = peer.Listener;
        if(listener.Pending())
        {
        begin:

            using (var request = await listener.AcceptTcpClientAsync(CancellationToken.None))
            {

                int size = request.Available;
                var reqStream = request.GetStream();
                processRequestData(reqStream, size);
            }

            if (listener.Pending())
                goto begin;
        }

        // Write pending peer data
        var stream = peer.Client.GetStream();
        while(peer.Pending.Count > 0)
        {
            var pending = peer.Pending.Dequeue();
            pending(peer.Address, peer.Client, stream);
        }
    }
    void processRequestData(NetworkStream stream, int size)
    {
        Span<byte> buffer = stackalloc byte[size - /* request type */ 1];
        byte requestType = (byte)stream.ReadByte();
        stream.Read(buffer);

        if (peerProcessors.TryGetValue(requestType, out var processor))
            processor.Processor(buffer);
        else
            throw new Exception($"Cannot find request processor of type {requestType}: Your version is probably different from of that peer");
/*        foreach(var processor in peerProcessors)
        {
            bool result = processor.Processor(buffer);
            if (result && processor.Exclusive)
                break;
        }*/
    }
}
/// <summary>
/// Peer Data Processor
/// </summary>
/// <param name="Id">The id of this peer data processor</param>
/// <param name="Processor">The processor method to process incoming data from peers</param>
/// <param name="Type">The request type of this processor (a processor can only have one and there are only <see cref="NetworkGenerals.MaxRequestTypes"/> request types disponible to be claimed)</param>
public readonly record struct PeerDataProcessor(string Id, ProcessPeerDataMethod Processor, byte Type);
public delegate bool ProcessPeerDataMethod(Span<byte> receivedData);
