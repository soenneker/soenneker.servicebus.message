using System;
using Azure.Messaging.ServiceBus;
using AwesomeAssertions;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.ServiceBus.Message.Tests.Messages;
using Soenneker.Tests.FixturedUnit;
using Xunit;


namespace Soenneker.ServiceBus.Message.Tests;

[Collection("ServiceBusMessageUtilCollection")]
public class ServiceBusMessageUtilTests : FixturedUnitTest
{
    private readonly IServiceBusMessageUtil _util;

    public ServiceBusMessageUtilTests(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
        _util = Resolve<IServiceBusMessageUtil>();
    }

    [Fact]
    public void Default()
    {
    }

    [Fact]
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