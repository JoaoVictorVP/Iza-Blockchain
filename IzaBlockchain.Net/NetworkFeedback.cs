namespace IzaBlockchain.Net;

public static class NetworkFeedback
{
    static List<NetworkFeedbackMessage> messages = new List<NetworkFeedbackMessage>(32);
    static event Action<NetworkFeedbackMessage>? messageReceived;

    public static void SendFeedback(string message, FeedbackType type)
    {
        var _message = new NetworkFeedbackMessage(message, type);

        messages.Add(_message);

        messageReceived?.Invoke(_message);
    }
    public static void ListenFeedbacks(Action<NetworkFeedbackMessage> listener) => messageReceived += listener;

    public record struct NetworkFeedbackMessage(string Message, FeedbackType Type);
    public enum FeedbackType
    {
        Info,
        Warning,
        Error
    }
}