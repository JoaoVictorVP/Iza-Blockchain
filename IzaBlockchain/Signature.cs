namespace IzaBlockchain
{
    public unsafe struct Signature
    {
        public fixed byte data[BlockchainGenerals.SignatureSize];

        public Signature(byte* ptr_data)
        {
            for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                data[i] = ptr_data[i];
        }
        public Signature(byte[] arr_data)
        {
            for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                data[i] = arr_data[i];
        }
        public Signature(Span<byte> span_data)
        {
            for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                data[i] = span_data[i];
        }
    }
}