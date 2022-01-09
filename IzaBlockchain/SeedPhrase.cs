using System.Security.Cryptography;
using System.Text;

namespace IzaBlockchain
{
    public unsafe struct SeedPhrase
    {
        public fixed byte seed[BlockchainGenerals.PrivateAddressSize];

        public static SeedPhrase CreateSeed(string phrase)
        {
            SHA256 sha = SHA256.Create();

            int phraseByteCount = Encoding.UTF8.GetByteCount(phrase);
            Span<byte> phrasePtr = stackalloc byte[phraseByteCount];
            Encoding.UTF8.GetBytes(phrase, phrasePtr);

            Span<byte> seedPtr = stackalloc byte[BlockchainGenerals.PrivateAddressSize];
            if(sha.TryComputeHash(phrasePtr, seedPtr, out _))
            {
                SeedPhrase seed;
                for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
                    seed.seed[i] = seedPtr[i];
                return seed;
            }
            return default;
        }

        public bool IsEqual(SeedPhrase other)
        {
            for(int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
            {
                if (seed[i] != other.seed[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            fixed (byte* seedPtr = seed)
            {
                ReadOnlySpan<byte> mSeed = new ReadOnlySpan<byte>(seedPtr, BlockchainGenerals.PrivateAddressSize);

                return Convert.ToHexString(mSeed);
            }
        }
    }
}