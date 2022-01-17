using IzaBlockchain.MemDatas;
using IzaBlockchain.Net;

namespace IzaBlockchain.Final;

/// <summary>
/// Override this class and assembly to implement your blockchain specifics and links (otherwise contains default Iza Blockhain implementation)<br/>
/// Documentation Reading Guide:<br/>
/// --- (...) * (...): Required code area
/// </summary>
public class MainClass
{
    public static Node Node { get; private set; }
    public static void Run()
    {
        // Implements peers recovery and writting on this blockchain *
        Blockchain.AddMemData(BlockchainMemDataGenerals.PeerDataName, new PeerData());

        // Initialize blockchain
        Blockchain.Begin();

        // Runs the node
        Node = new Node();
        Node.Initialize();

        // Syncs the blockchain
        Blockchain.Sync();

#if DEBUG
        NetworkFeedback.ListenFeedbacks(feedback =>
        {
            // Default, Console implementation of feedback listening

            var original = Console.ForegroundColor;

            var color = original;
            switch(feedback.Type)
            {
                case NetworkFeedback.FeedbackType.Info:
                    color = original;
                    break;
                case NetworkFeedback.FeedbackType.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case NetworkFeedback.FeedbackType.Error:
                    color = ConsoleColor.Red;
                    break;
            }

            Console.ForegroundColor = color;

            Console.WriteLine(feedback.Message);

            Console.ForegroundColor = original;
        });
#endif
    }
    public static void Stop()
    {
        // Stops the blockchain
        Blockchain.End();

        // Finalize the node
        Node.Finish();
    }
}
