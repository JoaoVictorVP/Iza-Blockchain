using IzaBlockchain;
using System.Text;

SeedPhrase seed = SeedPhrase.CreateSeed("My name is John");
SeedPhrase otherA = SeedPhrase.CreateSeed("My name is Carlos");
SeedPhrase otherB = SeedPhrase.CreateSeed("My name is John");

Console.WriteLine("Seed: " + seed);
Console.WriteLine("Seed equals OtherA? " + seed.IsEqual(otherA));
Console.WriteLine("Seed equals OtherB? " + seed.IsEqual(otherB));

var wallet = Utils.CreateWallet(seed);
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



while(true)
{
    Console.WriteLine("Type your message to sign:");
    Console.Write("> ");
    string message = Console.ReadLine() ?? "";

    var signature = wallet.SignArbitrary(new Span<byte>(Encoding.UTF8.GetBytes(message)));

    Console.WriteLine("Signature: " + signature);
}