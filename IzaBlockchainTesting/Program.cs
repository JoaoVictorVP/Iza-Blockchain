using IzaBlockchain;
using IzaBlockchain.Net;
using Mii.NET;
using Newtonsoft.Json;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ZeroKnowledgeNipah;

SeedPhrase seed = SeedPhrase.CreateSeed("My name is John");
SeedPhrase otherA = SeedPhrase.CreateSeed("My name is Carlos");
SeedPhrase otherB = SeedPhrase.CreateSeed("My name is John");

Console.WriteLine("Seed: " + seed);
Console.WriteLine("Seed equals OtherA? " + seed.IsEqual(otherA));
Console.WriteLine("Seed equals OtherB? " + seed.IsEqual(otherB));

var wallet = Utils.CreatePrivateKey(seed);
var publicAddress = wallet.GetPublicAddress();

var blockData = new byte[]
{
    100, 0, 10, 50, 250, 130, 33, 1
};

var block = Block.Create(default, publicAddress, default, default, blockData);
var block2 = Block.Create(block.header.blockHash, publicAddress, default, default, blockData);
var block3 = Block.Create(block2.header.blockHash, publicAddress, default, default, blockData);

Console.WriteLine("Block: " + block.ToString());
Console.WriteLine("Block2: " + block2.ToString());
Console.WriteLine("Block3: " + block3.ToString());

var sign = ZKNipah.SecondSign(wallet.SignArbitrary(new Span<byte> (new byte[] { 0, 0, 0 })), ref publicAddress);

var third = ZKNipah.ThirdSign(ref sign, ref wallet, new TimeStamp(new DateTime(3, 3, 3, 3, 3, 3, 3)));

var mask = ZKNipah.GetMask(wallet, new TimeStamp(new DateTime(3, 3, 3, 3, 3, 3, 3)));

var verifyThird = ZKNipah.ThirdSignExternal(ref sign, ref mask);


Hash256 h1 = publicAddress.ToHash256();

Console.WriteLine("Hash: " + $"0x{h1}");



Console.WriteLine($"Provided: {third}\nVerifying: {verifyThird}\nIs Equal: {verifyThird.IsEqual(third)}");
ClientUtils.GetSelfIP();

string privAddr = wallet.ToString();
PrivateAddress fromStr = PrivateAddress.FromString(privAddr);
Console.WriteLine("FromString: " + wallet.IsEqual(fromStr));

Wallet walletObj = new Wallet(wallet);
string json = Wallet.Serialize(walletObj, "Testing");
walletObj = Wallet.Deserialize(json, "Testing");

Console.WriteLine($"Deserialized wallet equals to owned: {wallet.IsEqual(walletObj.PrivateAddress)}");

//Blockchain.Local.SetData("Name", "Michael");

JsonSerializerSettings sets = null;
JsonConvert.DefaultSettings = () =>
{
    if (sets == null) sets = new JsonSerializerSettings();
    sets.Formatting = Formatting.Indented;
    return sets;
};

Console.WriteLine("Data got from LocalData: " + Blockchain.Local.GetData("Name"));

List<int> allCollisions = new List<int>(320);
List<int> collisions = new List<int>(30);
HashSet<int> nonces = new HashSet<int>(32);
int colls = 0;
int lastSizeNonces = 0;

int difficulty = 1;
unsafe
{
    while (true)
    {
        using var hash = MiningUtils.GetHashToMine(new Span<byte>(sign.data, BlockchainGenerals.SignatureSize), difficulty, false);
        Console.WriteLine("Start mining of: " + Convert.ToHexString(hash));
        int nonce = MiningUtils.Mine(hash, difficulty);
        if (nonces.Contains(nonce))
        {
            int difference = (nonces.Count + 1) - lastSizeNonces;

            lastSizeNonces = nonces.Count + 1;

            allCollisions.Add(difference);

            collisions.Add(difference);

            colls++;
            if(colls > 30)
            {
                double average = collisions.Average();
                Console.WriteLine($"Medium hashes-peer-nonce collisions in difficulty (3) are " + average);
                File.WriteAllText($"average-collisions-{difficulty} ({average}).json", JsonConvert.SerializeObject(collisions));

                collisions.Clear();

                difficulty++;
                colls = 0;

                File.WriteAllText($"Average-Collisions-All (until difficulty {difficulty}).json", JsonConvert.SerializeObject(allCollisions));
            }
        }
        nonces.Add(nonce);
        Console.WriteLine("Mined with nonce: " + nonce);
        bool valid = MiningUtils.TryNonce(hash, nonce, difficulty);
        Console.WriteLine("Finished mining with validity: " + valid);
    }
}



// RPC Try
/*Console.Write("Client or Server?\n> ");
string command = Console.ReadLine();

switch(command)
{
    case "Client":
        client();
        break;
    case "Server":
        server();
        break;
}*/

/*void client()
{
    Console.Write("Server IP\n> ");
    string ip = Console.ReadLine();
    var peer = new TcpPeer(IPAddress.Parse(ip), 8080, false, true);

    peer.Connect();
    //var client = new TcpClient(ip, 8080);

*//*    new Timer(p =>
    {
        if (!client.Connected)
            client.Connect(ip, 8080);
    }, null, 0, 50);*//*

    while(true)
    {
        //var client = new TcpClient(ip, 8080);
        Console.Write("Send Message To Server\n> ");
        string message = Console.ReadLine();
        byte[] buffer = Encoding.Unicode.GetBytes(message);

        peer.SendData(TcpPeer.Header.Request(), buffer);
        //var stream = client.GetStream();
        //stream.Write(buffer);
        

        Console.WriteLine("Message Sent");

        if(message.Contains('?'))
        {
            Console.WriteLine("Waiting for response...");
            using var responseResult = peer.WaitForResponse();
            //while(!stream.DataAvailable)
            //    Thread.Sleep(10);
            //Span<byte> loadBuffer = stackalloc byte[client.Available];
            //stream.Read(loadBuffer);

            string response = Encoding.Unicode.GetString(responseResult.Data);

            Console.WriteLine("Response: " + response);
        }

        Console.WriteLine();

        Console.ReadKey();
    }
}
void server()
{
    var rpc = new RPCServer();
    rpc.Run();
}

public class RPCServer
{
    TcpPeer peer;
    public void Run()
    {
        var ip = ClientUtils.GetSelfIP();
        peer = new TcpPeer(ip, 8080, true);
        peer.Connect();

        Console.WriteLine("Server Started At: " + ip);

        Loop();
    }
    void Loop()
    {
        peer.OnReceiveData += (header, data, isRequest, sender, peer) =>
        {
            string message = Encoding.Unicode.GetString(data);
            Console.WriteLine($"Message from ({(sender.Client.LocalEndPoint as IPEndPoint).Address}: {message}");

            if(header.Type == TcpPeer.MessageType.Request)
            {
                Console.Write("> ");
                string response = Console.ReadLine();
                Span<byte> responseBytes = stackalloc byte[Encoding.Unicode.GetByteCount(response)];
                
                Encoding.Unicode.GetBytes(response, responseBytes);

                peer.SendResponse(responseBytes, header.Id);
            }
        };

        while (true)
        {
            peer.Update();

            Thread.Sleep(300);
        }
    }
    void ProcessClient(TcpClient client)
    {
        //if (size == 0)
         //   return;
        var stream = client.GetStream();

        while(!stream.DataAvailable)
            Thread.Sleep(10);

        Span<byte> buffer = stackalloc byte[client.Available];

        stream.Read(buffer);

        string message = Encoding.Unicode.GetString(buffer);

        Console.WriteLine($"Received Data From: ({client.Client.LocalEndPoint}), Data: " + message);

        if(message.Contains('?'))
        {
            Console.Write("\n> ");
            string response = Console.ReadLine();
            stream.Write(Encoding.Unicode.GetBytes(response));
        }
    }
}*/


/*while (true)
{
    Console.WriteLine("Type your message to sign:");
    Console.Write("> ");
    string message = Console.ReadLine() ?? "";

    var signature = wallet.SignArbitrary(new Span<byte>(Encoding.UTF8.GetBytes(message)));

    Console.WriteLine("Signature: " + signature);
}*/

Console.WriteLine("Mining Test");
// Mining Algorithm Test
var miner = new Miner();

int difficultyX = 1;
int calls = 0;
while (true)
{
    var hash = miner.InitBlock(difficultyX);
    Console.WriteLine($"Hash to be worked (difficulty {difficultyX}: {Convert.ToHexString(hash)}");
    var watch = new Stopwatch();
    watch.Start();
    int nonce = miner.Mine(hash, difficultyX);
    watch.Stop();

    Console.WriteLine($"Block minned in {watch.Elapsed} with nonce {nonce}");

    calls++;

    if (calls > 100)
    {
        difficultyX++;
        calls = 0;
    }
}

public class Miner
{
    public Span<byte> InitBlock(int difficulty)
    {
        Span<byte> hash = stackalloc byte[32];
        Span<byte> subhash = hash.Slice(0, difficulty);
        RandomNumberGenerator.Fill(subhash);

        return hash.ToArray();
    }
    public int Mine(Span<byte> hash, int difficulty)
    {
        Span<byte> subhash = hash.Slice(0, difficulty);
        Span<byte> tryHash = stackalloc byte[32];
        Span<byte> checkNonce = stackalloc byte[4];

/*        for (int nonce = 0; nonce < int.MaxValue; nonce++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(checkNonce, nonce);
            SHA256.HashData(checkNonce, tryHash);
            var subtryHash = tryHash.Slice(0, difficulty);

            if (subtryHash.SequenceEqual(subhash))
                return nonce;
        }*/

        bool worked = false;
        int result = -1;

        const int middle = int.MaxValue / 2;

        byte[] nhash = hash.ToArray();

        Thread ta = new Thread(() =>
        {
            Span<byte> hash = nhash;
            Span<byte> subhash = hash.Slice(0, difficulty);
            Span<byte> tryHash = stackalloc byte[32];
            Span<byte> checkNonce = stackalloc byte[4];
            for (int nonce = 0; nonce < middle; nonce++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(checkNonce, nonce);
                SHA256.HashData(checkNonce, tryHash);
                var subtryHash = tryHash.Slice(0, difficulty);

                if (subtryHash.SequenceEqual(subhash))
                {
                    result = nonce;
                    worked = true;
                    return;
                }
            }
        });

        Thread tb = new Thread(() =>
        {
            Span<byte> hash = nhash;
            Span<byte> subhash = hash.Slice(0, difficulty);
            Span<byte> tryHash = stackalloc byte[32];
            Span<byte> checkNonce = stackalloc byte[4];
            for (int nonce = middle; nonce < middle * 2; nonce++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(checkNonce, nonce);
                SHA256.HashData(checkNonce, tryHash);
                var subtryHash = tryHash.Slice(0, difficulty);

                if (subtryHash.SequenceEqual(subhash))
                {
                    result = nonce;
                    worked = true;
                    return;
                }
            }
        });

        ta.Start();
        tb.Start();

        while (!worked)
        {

        }

/*        byte[] nhash = hash.ToArray();
        bool worked = false;
        var result = Parallel.For(0, int.MaxValue, (nonce, state) =>
        {
            Span<byte> hash = nhash;
            Span<byte> subhash = hash.Slice(0, difficulty);
            Span<byte> tryHash = stackalloc byte[32];
            Span<byte> checkNonce = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(checkNonce, nonce);
            SHA256.HashData(checkNonce, tryHash);
            var subtryHash = tryHash.Slice(0, difficulty);

            if (subtryHash.SequenceEqual(subhash))
            {
                worked = true;
                state.Break();
            }
        });

        if (worked)
            return (int)result.LowestBreakIteration;*/

        return result;
    }
}