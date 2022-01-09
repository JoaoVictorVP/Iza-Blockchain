namespace IzaBlockchain
{
    public unsafe struct Block
    {
        public readonly BlockHeader header;


    }
    public struct BlockHeader
    {
        public Address contract;
        public Signature sender_signature;
        public Signature validator_signature;

        public int size;
    }
}