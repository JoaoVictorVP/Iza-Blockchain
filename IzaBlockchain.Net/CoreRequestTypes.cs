namespace IzaBlockchain.Net;

public enum CoreRequestTypes : byte
{
    /// <summary>
    /// Command to feed peer data with this node information and propagate to other known peers<br/>
    /// Data Format:<br/>
    ///  *addOrRemove*: bool (1 byte) --- If true, then add, if false, then remove
    /// </summary>
    FeedPeerDataAndPropagate = 0,
    /// <summary>
    /// Command to retrieve peer data from requested peer and sync with the network
    /// </summary>
    SyncPeerData = 1,
    /// <summary>
    /// Command to retrieve peer public addresses to this node
    /// </summary>
    RetrieveAddresses = 2
}