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

        public bool IsEqual(Signature other)
        {
            for(int i = 0; i < BlockchainGenerals.SignatureSize; i++)
            {
                if (data[i] != other.data[i])
                    return false;
            }
            return true;
        }

        public void GetBytes(Span<byte> bytes)
        {
            for(int i = 0; i < BlockchainGenerals.SignatureSize; i++)
                bytes[i] = data[i];
        }
        public override string ToString()
        {
            fixed (byte* ptr = data)
                return Convert.ToHexString(new ReadOnlySpan<byte>(ptr, BlockchainGenerals.SignatureSize));
        }
        public static Signature FromString(string hexStr) => new Signature(Convert.FromHexString(hexStr));
    }
}