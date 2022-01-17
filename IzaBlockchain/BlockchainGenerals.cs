namespace IzaBlockchain;

public static class BlockchainGenerals
{
    public const string Name = "IzaChain";

    public const int AddressSize = 32;
    public const int PrivateAddressSize = 32;
    public const int SignatureSize = 64;
        
    public const int BlockHashSize = 64;

    /// <summary>
    /// Main connection port to network, starts with 30000 and can go through until 30500
    /// </summary>
    public const int ConnectionPort = 30001;
}

public static class BlockchainMemDataGenerals
{
    /// <summary>
    /// Name of entry on <see cref="Blockchain"/> for a <see cref="MemData"/> called <see cref="MemDatas.PeerData"/>
    /// </summary>
    public const string PeerDataName = "Peers";
}