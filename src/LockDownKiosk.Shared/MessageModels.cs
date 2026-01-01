namespace LockDownKiosk.Shared;

public enum MessageType
{
    Hello,
    StatusUpdate,
    Command,
    StartSession,
    EndSession
}

public sealed class AppMessage
{
    public MessageType Type { get; set; }

    public string Sender { get; set; } = "";

    public string Content { get; set; } = "";

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
