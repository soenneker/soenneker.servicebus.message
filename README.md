[![](https://img.shields.io/nuget/v/Soenneker.ServiceBus.Message.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.ServiceBus.Message/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.message/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.message/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.ServiceBus.Message.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.ServiceBus.Message/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.message/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.message/actions/workflows/codeql.yml)

# Soenneker.ServiceBus.Message

Builds Azure `ServiceBusMessage` instances from `Soenneker.Messages.Base.Message` payloads with serializer selection, type metadata, size rejection, and optional payload logging.

## Installation

```bash
dotnet add package Soenneker.ServiceBus.Message
```

## Registration

```csharp
using Soenneker.ServiceBus.Message.Registrars;

services.AddServiceBusMessageUtilAsSingleton();
```

The utility is stateless after configuration is loaded. A scoped registration is also available through `AddServiceBusMessageUtilAsScoped()`.

## Build and send a message

Define a transport payload derived from `Soenneker.Messages.Base.Message`, then pass its stable type identifier to `BuildMessage`:

```csharp
using Azure.Messaging.ServiceBus;
using Soenneker.Messages.Base;
using Soenneker.ServiceBus.Message.Abstract;

public sealed class OrderCreated : Message
{
    public required string OrderId { get; init; }
}

OrderCreated payload = new()
{
    Type = "order.created.v1",
    Id = Guid.NewGuid().ToString("N"),
    Queue = "orders",
    Sender = "checkout-api",
    CreatedAt = DateTimeOffset.UtcNow,
    OrderId = orderId
};

ServiceBusMessage? transportMessage =
    messageUtil.BuildMessage(payload, payload.Type);

if (transportMessage is null)
    return;

await sender.SendMessageAsync(transportMessage, cancellationToken);
```

The `type` argument is placed in `ApplicationProperties["type"]`. It is independent of `payload.Type`, so pass the same stable value unless your routing contract deliberately requires something else.

This builder does not map `payload.Id` to `ServiceBusMessage.MessageId`, set `ContentType`, choose a queue, or send the message. Set additional broker properties on the returned message before sending when required.

## Serialization and size behavior

System.Text.Json is used by default. Set the payload's `NewtonsoftSerialize` property to `true` to use Newtonsoft.Json instead:

```csharp
payload.NewtonsoftSerialize = true;
```

Messages whose serialized body exceeds 260,096 bytes are rejected and return `null`. That check covers the body only; it does not calculate the broker size of application properties or other AMQP overhead.

Serialization failures are logged at critical level and return `null`. Always check the result before passing it to a sender.

## Payload logging

`Azure:ServiceBus:Log` controls payload logging:

```json
{
  "Azure": {
    "ServiceBus": {
      "Log": false
    }
  }
}
```

When enabled, complete serialized payloads are written at debug level during normal message construction and may be included in critical error logs. It also selects the pretty JSON option instead of the web JSON option, so enabling it changes whitespace in the serialized body.

Leave payload logging disabled for messages containing credentials, personal data, or other sensitive fields unless the log destination and retention policy are appropriate.
