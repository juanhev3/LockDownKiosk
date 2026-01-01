using System.Text.Json;
using LockDownKiosk.Shared;
using NUnit.Framework;

namespace LockDownKiosk.Tests;

public class MessageModelsTests
{
    [Test]
    public void AppMessage_SerializeAndDeserialize_PreservesFields()
    {
        var original = new AppMessage
        {
            Type = MessageType.Hello,
            Sender = "Student-1",
            Content = "HELLO from Student",
            TimestampUtc = new DateTime(2025, 12, 13, 0, 0, 0, DateTimeKind.Utc)
        };

        var json = JsonSerializer.Serialize(original);
        var copy = JsonSerializer.Deserialize<AppMessage>(json);

        Assert.That(copy, Is.Not.Null);
        Assert.That(copy!.Type, Is.EqualTo(original.Type));
        Assert.That(copy.Sender, Is.EqualTo(original.Sender));
        Assert.That(copy.Content, Is.EqualTo(original.Content));
        Assert.That(copy.TimestampUtc, Is.EqualTo(original.TimestampUtc));
    }

    [Test]
    public void AppMessage_DefaultValues_AreSafe()
    {
        var msg = new AppMessage();

        Assert.That(msg.Sender, Is.Not.Null);
        Assert.That(msg.Content, Is.Not.Null);
        Assert.That(msg.TimestampUtc, Is.Not.EqualTo(default(DateTime)));
    }
}
