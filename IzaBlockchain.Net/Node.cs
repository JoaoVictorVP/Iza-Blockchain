using IzaBlockchain.MemDatas;
using IzaBlockchain.Net.RequestProcessors;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace IzaBlockchain.Net;

public class Node
{
    public static Node Self => self;

    public Wallet CurrentWallet;

    public void CurrentWallet_CreateNew(string seedPhrase)
    {
        var seed = SeedPhrase.CreateSeed(seedPhrase);
        var privAddr = Utils.CreatePrivateKey(seed);
        CurrentWallet = new Wallet(privAddr);
    }

    public void CurrentWallet_SaveToFile(string path, string password = null)
    {
        string json = Wallet.Serialize(CurrentWallet, password);

        File.WriteAllText(path, json);
    }

    public void CurrentWallet_LoadFromFile(string path, string password = null)
    {
        string json = File.ReadAllText(path);

        CurrentWallet = Wallet.Deserialize(json, password);
    }


    static Node self;
    //static readonly List<PeerDataProcessor> peerProcessors = new(32);
    static readonly Dictionary<byte, PeerDataProcessor> peerProcessors = new(32);

    //public static void ProcessPeerData(PeerDataProcessor processor) => peerProcessors.Add(processor);
    public static void ProcessPeerData(PeerDataProcessor processor) => peerProcessors.Add(processor.Type, processor);

    int _conCount = BlockchainGenerals.ConnectionPort;
    public int GetCurrentConnectionCount() => _conCount;
    public void IncreaseConnectionCount() => _conCount++;

    List<PeerConnection> connections = new List<PeerConnection>(32);

    public PeerConnection GetRandomConnectedPeer() => connections[random.Next(0, connections.Count)];

    public IEnumerable<PeerConnection> AllPeers() => connections;

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
                connection.Disconnect();
                connections.Remove(connection);

                return true;
            }
        }
        return false;
    }
    readonly Random random = new Random();
    public void Initialize()
    {
        // Add's all the core request processors (in future source generators should be used to make things easier)
        _ = new FeedPeerDataAndPropagateProcessor(); // 0
        _ = new SyncPeerDataProcessor(); // 1
        _ = new RetrieveAddressesProcessor(); // 2

        Task.Run(Run);

        // Send node IP for all connected peers and propagate though network
        var peer = Peer.From(ClientUtils.GetSelfIP());
        foreach (var connection in connections)
        {
            connection.SendData((_peer) =>
            {
                Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).AddPeer(peer);

#if DEBUG
                NetworkFeedback.SendFeedback($"SENDING NODE IP: FROM {peer} TO: {connection.Peer}", NetworkFeedback.FeedbackType.Info);
#endif

                return new PeerSendDataBuilder(new SpanStream(6)
                    // Type
                    .WriteByte((byte)CoreRequestTypes.FeedPeerDataAndPropagate)

                    // Code to add
                    .WriteByte(1)

                    // Peer
                    .WriteByte(peer.A)
                    .WriteByte(peer.B)
                    .WriteByte(peer.C)
                    .WriteByte(peer.D))
                .AsData();
            });
        }

        // Add's syncing for the node necessities (not blockchain itself it should be done on other class)
        Blockchain.OnSync += timeOff =>
        {
            // Sync's with the peer data
            var connection = GetRandomConnectedPeer();
            connection.SendData((_peer) =>
            {
#if DEBUG
                NetworkFeedback.SendFeedback($"REQUESTING ALL PEER DATA (SYNC) FROM: {connection.Peer}", NetworkFeedback.FeedbackType.Info);
#endif

                return new PeerSendDataBuilder(new SpanStream(1)
                    .WriteByte((byte)CoreRequestTypes.SyncPeerData))
                .AsRequest()
                .OnResponse(data =>
                {
#if DEBUG
                    NetworkFeedback.SendFeedback("RECEIVING ALL PEER DATA", NetworkFeedback.FeedbackType.Info);
#endif
                    File.WriteAllBytes(Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).FullPath, data.ToArray());
                });
            });
        };
#if DEBUG
        NetworkFeedback.SendFeedback($"NODE CONNECTED, IP: {ClientUtils.GetSelfIP()}", NetworkFeedback.FeedbackType.Info);
#endif
    }
    bool ended;
    public void Finish()
    {
        ended = true;

        // Send node IP removal order for all connected peers and propagate though network
        var peer = Peer.From(ClientUtils.GetSelfIP());
        foreach (var connection in connections)
        {
            connection.SendData((_peer) =>
            {
                Blockchain.GetMemData<PeerData>(BlockchainMemDataGenerals.PeerDataName).RemovePeer(peer);

#if DEBUG
                NetworkFeedback.SendFeedback($"REMOVING IP FROM {connection.Peer}", NetworkFeedback.FeedbackType.Info);
#endif

                return new PeerSendDataBuilder(new SpanStream(6)
                    // Type
                    .WriteByte((byte)CoreRequestTypes.FeedPeerDataAndPropagate)

                    // Code to remove
                    .WriteByte(0)

                    // Peer
                    .WriteByte(peer.A)
                    .WriteByte(peer.B)
                    .WriteByte(peer.C)
                    .WriteByte(peer.D))
                .AsData();
            });
        }
#if DEBUG
        NetworkFeedback.SendFeedback("NODE DISCONNECTED", NetworkFeedback.FeedbackType.Info);
#endif
    }
    async void Run()
    {
        // Bind Peers
        foreach (var peer in connections)
        {
            if (peer.Net != null)
            {
                peer.Net.OnReceiveData += (header, data, isRequest, sender, _peer) =>
{
                    //Span<byte> buffer = stackalloc byte[size - /* request type */ 1];
                    using var stream = new SpanStream(data);
                    byte requestType = (byte)stream.ReadByte();
                    //stream.Read(buffer);

                    if (peerProcessors.TryGetValue(requestType, out var processor))
                        processor.Processor(header, stream, peer, sender);
                    else
                        throw new Exception($"Cannot find request processor of type {requestType}: Your version is probably different from of that peer");
                };
            }
        }

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
                    NetworkFeedback.SendFeedback($"Peer ({peer.Peer.GetIP()}) Error: " + exc.Message, NetworkFeedback.FeedbackType.Error);
                }
            }

            await Task.Delay(10);

            if (ended)
                return;
        }
    }
    void RunPeer(PeerConnection peer)
    {
        if (peer == null || peer.Net != null) return;
        // Read incoming peer data (deprecated)
/*        var listener = peer.Listener;
        if(listener.Pending())
        {
        begin:

            using (var request = await listener.AcceptTcpClientAsync(CancellationToken.None))
            {

                int size = request.Available;
                var reqStream = request.GetStream();
                processRequestData(reqStream, size, peer, request);
            }

            if (listener.Pending())
                goto begin;
        }*/

        var net = peer.Net;

        net.Update();

        // Write pending peer data
        //var stream = peer.Client.GetStream();
        while(peer.Pending.Count > 0)
        {
            //using var stream = new SpanStream();
            var pending = peer.Pending.Dequeue();

            var builder = pending(peer);

            var data = builder.Finish(out var header);

            peer.Net.SendData(header, data);

            if(header.Type == TcpPeer.MessageType.Request)
            {
                var response = peer.Net.WaitForResponse();
                builder.Response(response.AsSpan());
            }
        }
    }
/*    void processRequestData(NetworkStream stream, int size, PeerConnection connection, TcpClient client)
    {
        Span<byte> buffer = stackalloc byte[size - *//* request type *//* 1];
        byte requestType = (byte)stream.ReadByte();
        stream.Read(buffer);

        if (peerProcessors.TryGetValue(requestType, out var processor))
            processor.Processor(buffer, connection, client);
        else
            throw new Exception($"Cannot find request processor of type {requestType}: Your version is probably different from of that peer");
*//*        foreach(var processor in peerProcessors)
        {
            bool result = processor.Processor(buffer);
            if (result && processor.Exclusive)
                break;
        }*//*
    }*/

    public Node()
    {
        if (self != null)
            throw new Exception("Only one Node can be run at once");
        self = this;
    }
}
/// <summary>
/// Peer Data Processor
/// </summary>
/// <param name="Id">The id of this peer data processor</param>
/// <param name="Processor">The processor method to process incoming data from peers</param>
/// <param name="Type">The request type of this processor (a processor can only have one and there are only <see cref="NetworkGenerals.MaxRequestTypes"/> request types disponible to be claimed)</param>
public readonly record struct PeerDataProcessor(string Id, ProcessPeerDataMethod Processor, byte Type);
public delegate bool ProcessPeerDataMethod(TcpPeer.Header receivedHeader, SpanStream receivedData, PeerConnection fromPeer, TcpClient fromClient);
