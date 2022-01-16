namespace IzaBlockchain.Net;

/// <summary>
/// Provide general information about this implementation of networking
/// </summary>
public static class NetworkGenerals
{
    /// <summary>
    /// Peer Request Format:<br/>
    /// *request type* (byte: 1 byte)<br/>
    /// *request data* (any: X bytes)
    /// </summary>
    public const int MaxRequestTypes = byte.MaxValue;
}