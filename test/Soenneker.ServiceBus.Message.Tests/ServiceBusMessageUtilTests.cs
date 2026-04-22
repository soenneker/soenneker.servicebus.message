using System;
using Azure.Messaging.ServiceBus;
using AwesomeAssertions;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.ServiceBus.Message.Tests.Messages;
using Soenneker.Tests.HostedUnit;


namespace Soenneker.ServiceBus.Message.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class ServiceBusMessageUtilTests : HostedUnitTest
{
    private readonly IServiceBusMessageUtil _util;

    public ServiceBusMessageUtilTests(Host host) : base(host)
    {
        _util = Resolve<IServiceBusMessageUtil>();
    }

    [Test]
    public void Default()
    {
    }

    [Test]
    public void BuildMessage_should_build_a_message()
    {
        var testMessage = new TestMessage
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Queue = "test",
            Sender = "test",
            Type = "test",
            Contents = "blah"
        };

        ServiceBusMessage? result = _util.BuildMessage(testMessage, "test");
        result.Should()
              .NotBeNull();
    }
}