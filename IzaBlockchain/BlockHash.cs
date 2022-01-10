using System.Security.Cryptography;

namespace IzaBlockchain
{
    public unsafe struct BlockHash
    {
        public fixed byte hash[BlockchainGenerals.BlockHashSize];

        public BlockHash(Span<byte> span_data)
        {
            for (int i = 0; i < BlockchainGenerals.BlockHashSize; i++)
                hash[i] = span_data[i];
        }

        public bool IsEqual(BlockHash other)
        {
            fixed(byte* hashPtr = hash)
                return Utils.CompareBytes(hashPtr, other.hash, BlockchainGenerals.BlockHashSize);
        }

        public override string ToString()
        {
            fixed (byte* ptr = hash)
                return Convert.ToHexString(new ReadOnlySpan<byte>(ptr, BlockchainGenerals.BlockHashSize));
        }

        static readonly SHA512 sha512 = SHA512.Create();
        public static unsafe BlockHash GetBlockHash(Block block)
        {
            /*int headerSize = BlockHeader.HeaderByteCount;
            var header = block.header.GetBytes().Span;
            int dataSize = block.header.size;
            var data = new Memory<byte>(block.data).Span;

            Span<byte> data_header = stackalloc byte[headerSize + dataSize];
            for (int i = 0; i < headerSize; i++)
                data_header[i] = header[i];
            for (int i = 0; i < dataSize; i++)
                data_header[i + headerSize] = data[i];

            Span<byte> hash = stackalloc byte[BlockchainGenerals.BlockHashSize];

            if (sha512.TryComputeHash(data_header, hash, out _))
                return new BlockHash(hash);

            return default;*/

            return GetBlockHash(block.header, block.data);
        }
        public static unsafe BlockHash GetBlockHash(BlockHeader blockHeader, byte[] blockData)
        {
            int headerSize = BlockHeader.HeaderByteCount - /* exclude self hash */ BlockchainGenerals.BlockHashSize;
            var header = blockHeader.GetBytes().Span;
            int dataSize = blockHeader.size;
            var data = new Memory<byte>(blockData).Span;

            Span<byte> data_header = stackalloc byte[headerSize + dataSize];
            for (int i = 0; i < headerSize; i++)
                data_header[i] = header[i];
            for (int i = 0; i < dataSize; i++)
                data_header[i + headerSize] = data[i];

            Span<byte> hash = stackalloc byte[BlockchainGenerals.BlockHashSize];

            if (sha512.TryComputeHash(data_header, hash, out _))
                return new BlockHash(hash);

            return default;
        }
    }
}