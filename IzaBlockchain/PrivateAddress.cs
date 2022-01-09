namespace IzaBlockchain
{
    public unsafe struct PrivateAddress
    {
        public fixed byte data[BlockchainGenerals.PrivateAddressSize];

        public PrivateAddress(byte* ptr_data)
        {
            for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
                data[i] = ptr_data[i];
        }
        public PrivateAddress(byte[] arr_data)
        {
            for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
                data[i] = arr_data[i];
        }
        public PrivateAddress(Span<byte> span_data)
        {
            for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
                data[i] = span_data[i];
        }

        public bool IsEqual(PrivateAddress other)
        {
            for(int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
            {
                if (data[i] != other.data[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            fixed (byte* ptr = data)
                return Convert.ToHexString(new ReadOnlySpan<byte>(ptr, BlockchainGenerals.PrivateAddressSize));
        }
        public static PrivateAddress FromString(string hexAddress) => new PrivateAddress(Convert.FromHexString(hexAddress));
    }
}