using IzaBlockchain;
using IzaBlockchain.Net;
using Newtonsoft.Json;
using System.Buffers.Binary;
using System.Diagnostics;
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

Console.WriteLine($"Provided: {third}\nVerifying: {verifyThird}\nIs Equal: {verifyThird.IsEqual(third)}");
ClientUtils.GetSelfIP();

string privAddr = wallet.ToString();
PrivateAddress fromStr = PrivateAddress.FromString(privAddr);
Console.WriteLine("FromString: " + wallet.IsEqual(fromStr));

Wallet walletObj = new Wallet(wallet);
string json = Wallet.Serialize(walletObj, "Testing");
walletObj = Wallet.Deserialize(json, "Testing");

Console.WriteLine($"Deserialized wallet equals to owned: {wallet.IsEqual(walletObj.PrivateAddress)}");

Blockchain.Local.SetData("Name", "Michael");

Console.WriteLine("Data got from LocalData: " + Blockchain.Local.GetData("Name"));

/*while (true)
{
    Console.WriteLine("Type your message to sign:");
    Console.Write("> ");
    string message = Console.ReadLine() ?? "";

    var signature = wallet.SignArbitrary(new Span<byte>(Encoding.UTF8.GetBytes(message)));

    Console.WriteLine("Signature: " + signature);
}*/

/*Console.WriteLine("Mining Test");
// Mining Algorithm Test
var miner = new Miner();

int difficulty = 1;
int calls = 0;
while (true)
{
    var hash = miner.InitBlock(difficulty);
    Console.WriteLine($"Hash to be worked (difficulty {difficulty}: {Convert.ToHexString(hash)}");
    var watch = new Stopwatch();
    watch.Start();
    int nonce = miner.Mine(hash, difficulty);
    watch.Stop();

    Console.WriteLine($"Block minned in {watch.Elapsed} with nonce {nonce}");

    calls++;

    if(calls > 100)
    {
        difficulty++;
        calls = 0;
    }
}*/

/*public class Miner
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
*//*        Span<byte> subhash = hash.Slice(0, difficulty);
        Span<byte> tryHash = stackalloc byte[32];
        Span<byte> checkNonce = stackalloc byte[4];

        for (int nonce = 0; nonce < int.MaxValue; nonce++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(checkNonce, nonce);
            SHA256.HashData(checkNonce, tryHash);
            var subtryHash = tryHash.Slice(0, difficulty);

            if (subtryHash.SequenceEqual(subhash))
                return nonce;
        }*//*

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
            for(int nonce = 0; nonce < middle; nonce++)
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
            for(int nonce = middle; nonce < middle * 2; nonce++)
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

        while(!worked)
        {

        }

        *//*        byte[] nhash = hash.ToArray();
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
                    return (int)result.LowestBreakIteration;*//*

        return result;
    }
}*/