namespace Soenneker.ServiceBus.Message.Tests.Messages;

public class TestMessage : Soenneker.Messages.Base.Message
{
    public string? Contents { get; set; }

    public TestMessage(string queue) : base(queue)
    {
    }
}