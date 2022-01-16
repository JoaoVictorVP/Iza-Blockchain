using IzaBlockchain;
using IzaBlockchain.Net;
using System.Numerics;
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

while (true)
{
    Console.WriteLine("Type your message to sign:");
    Console.Write("> ");
    string message = Console.ReadLine() ?? "";

    var signature = wallet.SignArbitrary(new Span<byte>(Encoding.UTF8.GetBytes(message)));

    Console.WriteLine("Signature: " + signature);
}