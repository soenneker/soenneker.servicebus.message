using Azure.Messaging.ServiceBus;
using FluentAssertions;
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
    public void BuildMessage_should_build_a_message()
    {
        var testMessage = new TestMessage("")
        {
            Contents = "blah"
        };

        ServiceBusMessage? result = _util.BuildMessage(testMessage, typeof(TestMessage));
        result.Should().NotBeNull();
    }
}