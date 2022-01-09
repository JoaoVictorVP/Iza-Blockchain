using IzaBlockchain;

SeedPhrase seed = SeedPhrase.CreateSeed("My name is John");
SeedPhrase otherA = SeedPhrase.CreateSeed("My name is Carlos");
SeedPhrase otherB = SeedPhrase.CreateSeed("My name is John");

Console.WriteLine("Seed: " + seed);
Console.WriteLine("Seed equals OtherA? " + seed.IsEqual(otherA));
Console.WriteLine("Seed equals OtherB? " + seed.IsEqual(otherB));

var wallet = Utils.CreateWallet(seed);
