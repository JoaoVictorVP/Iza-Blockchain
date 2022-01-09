namespace IzaBlockchain
{
    public unsafe struct Address
    {
        public fixed byte data[BlockchainGenerals.AddressSize];

        public Address(byte* ptr_data)
        {
            for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
                data[i] = ptr_data[i];
        }
        public Address(byte[] arr_data)
        {
            for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
                data[i] = arr_data[i];
        }
        public Address(Span<byte> span_data)
        {
            for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
                data[i] = span_data[i];
        }
    }
}