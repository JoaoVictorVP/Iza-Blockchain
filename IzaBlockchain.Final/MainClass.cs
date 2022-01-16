using IzaBlockchain.MemDatas;

namespace IzaBlockchain.Final;

/// <summary>
/// Override this class and assembly to implement your blockchain specifics and links (otherwise contains default Iza Blockhain implementation)<br/>
/// Documentation Reading Guide:<br/>
/// --- (...) * (...): Required code area
/// </summary>
public class MainClass
{
    public static void Run()
    {
        // Implements peers recovery and writting on this blockchain *
        Blockchain.AddMemData(FinalGenerals.PeerDataName, new PeerData());

        // Initialize blockchain
        Blockchain.Begin();
    }
}
