using System.Buffers.Binary;
using System.Security.Cryptography;

namespace IzaBlockchain
{
    public static class Utils
    {
        static int privateAddressCount;
        public static unsafe PrivateAddress CreateWallet(SeedPhrase seed)
        {
            var hashing = new HMACSHA256(new Span<byte>(seed.seed, BlockchainGenerals.PrivateAddressSize).ToArray());

            Span<byte> privAddrCount = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(privAddrCount, privateAddressCount);

            Span<byte> hash = stackalloc byte[BlockchainGenerals.PrivateAddressSize];
            if (hashing.TryComputeHash(privAddrCount, hash, out _))
            {
                // disabled feature here (incremental addresses for same seed)
                //privateAddressCount++;
                return new PrivateAddress(hash);
            }

            return default;
        }

        public static unsafe bool CompareBytes(byte* a, byte* b, int size)
        {
            for (int i = 0; i < size; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
    }
}