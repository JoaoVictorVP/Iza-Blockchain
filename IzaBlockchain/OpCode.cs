namespace IzaBlockchain
{
    public enum OpCode : byte
    {
        /// <summary>
        /// Don't do anything, represents a null value on blockchain data
        /// </summary>
        Null = 0,
        /// <summary>
        /// Represents an address on blockchain, must be followed by {<see cref="BlockchainGenerals.AddressSize"/> (16)} bytes
        /// </summary>
        Address = 1,
        /// <summary>
        /// Represents a beginning of a block on the blockchain, must be followed by <see cref="BlockHeader.size"/>
        /// </summary>
        Block = 2,

    }
}