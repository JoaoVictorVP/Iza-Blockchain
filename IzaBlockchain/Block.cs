using IzaBlockchain.Net;
using System.Buffers.Binary;

namespace IzaBlockchain
{
    public unsafe struct Block
    {
        public readonly BlockHeader header;

/*        /// <summary>
        /// The specific data size of the block (excluding header)
        /// </summary>
        public int dataSize => header.size - BlockHeader.HeaderByteCount;*/

        public readonly NativeArray<byte> data;

        public static Block Create(BlockHash previousBlockHash, Address contract, Signature sender_signature, Signature validator_signature, byte[] data)
        {
            BlockHeader header;
            header.previousBlockHash = previousBlockHash;
            header.contract = contract;
            header.sender_signature = sender_signature;
            header.validator_signature = validator_signature;
            header.size = data.Length;

            header.blockHash = default;

            var block = new Block(header with { blockHash = BlockHash.GetBlockHash(header, data) }, data);

            return block;
        }

        /// <summary>
        /// Verify if the previous block on chain validate the next (this) in order to check the blockchain integrity
        /// </summary>
        /// <param name="previousBlock">The suposed to be previous block</param>
        /// <returns></returns>
        public bool VerifyPreviousChainBlock(Block previousBlock)
        {
            return header.previousBlockHash.IsEqual(previousBlock.header.blockHash);
        }
        /// <summary>
        /// Verify if the next block on chain is valid according to the previous (this) in order to check the blockchain integrity from now on
        /// </summary>
        /// <param name="nextBlock">The suposed to be next block on chain</param>
        /// <returns></returns>
        public bool VerifyNextChainBlock(Block nextBlock)
        {
            return nextBlock.VerifyPreviousChainBlock(this);
        }

        public Block(BlockHeader header, Span<byte> data)
        {
            this.header = header;
            this.data = new NativeArray<byte>(data);
        }

        public Block(BlockHeader header, byte[] data)
        {
            this.header = header;
            this.data = new NativeArray<byte>(data);
        }

        public override string ToString() => header.ToString();
    }
    public struct BlockHeader
    {
        public BlockHash blockHash;
        public BlockHash previousBlockHash;
        public Address contract;
        public Signature sender_signature;
        public Signature validator_signature;

        /// <summary>
        /// *outdated* The total size of the block (counting header)<br/>
        /// The total size of the block (excluding header)
        /// </summary>
        public int size;

        public const int HeaderByteCount = /* contract */ BlockchainGenerals.AddressSize + /* sender_signature + validator_signature */ (BlockchainGenerals.SignatureSize * 2) + /* blockHash + previousBlockHash */ (BlockchainGenerals.BlockHashSize * 2) + /* size */ sizeof(int);
        public unsafe Memory<byte> GetBytes(bool includingSelfHash = false)
        {
            byte[] data;
            if (includingSelfHash)
            {
                const int size = HeaderByteCount;

                //Span<byte> data = stackalloc byte[size];
                data = new byte[size];

                // blockHash
                for (int i = 0; i < BlockchainGenerals.BlockHashSize; i++)
                    data[i] = blockHash.hash[i];
                // previousBlockHash
                for (int i = 0; i < BlockchainGenerals.BlockHashSize; i++)
                    data[i + BlockchainGenerals.BlockHashSize] = previousBlockHash.hash[i];

                // contract
                for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize * 2)] = contract.data[i];

                // sender_signature
                for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize * 2) + BlockchainGenerals.AddressSize] = sender_signature.data[i];
                // validator_signature
                for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize * 2) + BlockchainGenerals.AddressSize + BlockchainGenerals.SignatureSize] = sender_signature.data[i];
            }
            else
            {
                const int size = HeaderByteCount - BlockchainGenerals.BlockHashSize;

                //Span<byte> data = stackalloc byte[size];
                data = new byte[size];

                // previousBlockHash
                for (int i = 0; i < BlockchainGenerals.BlockHashSize; i++)
                    data[i] = previousBlockHash.hash[i];

                // contract
                for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize)] = contract.data[i];

                // sender_signature
                for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize) + BlockchainGenerals.AddressSize] = sender_signature.data[i];
                // validator_signature
                for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                    data[i + (BlockchainGenerals.BlockHashSize) + BlockchainGenerals.AddressSize + BlockchainGenerals.SignatureSize] = validator_signature.data[i];
            }

            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, this.size);
            data[size - 3] = sizeSpan[0];
            data[size - 2] = sizeSpan[1];
            data[size - 1] = sizeSpan[2];
            data[size - 0] = sizeSpan[3];

            return new Memory<byte>(data);
        }

        public override string ToString()
        {
            return Convert.ToHexString(GetBytes(true).Span.ToArray());
        }
    }
}