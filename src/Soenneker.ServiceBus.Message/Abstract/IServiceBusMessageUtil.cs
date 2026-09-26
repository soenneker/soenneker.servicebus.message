using System;
using System.Diagnostics.Contracts;
using Azure.Messaging.ServiceBus;

namespace Soenneker.ServiceBus.Message.Abstract;

/// <summary>
/// Builds Azure Service Bus messages from Soenneker message envelopes.
/// </summary>
/// <remarks>Registered JSON contexts contribute generated metadata for message types. The primary JSON context is used when no contributed context handles a type.</remarks>
/// <remarks>Register a source-generated JsonSerializerContext covering application payloads.
/// Messages explicitly opting into NewtonsoftSerialize retain Newtonsoft contracts and require reflection support.</remarks>
public interface IServiceBusMessageUtil
{
    /// <summary>
    /// Serializes the payload, rejects bodies larger than 260,096 bytes, and adds the supplied type to the message application properties.
    /// </summary>
    /// <typeparam name="TMessage">Type of message used by the operation.</typeparam>
    /// <param name="message">Message content to send.</param>
    /// <param name="type">The stable message type stored in <c>ApplicationProperties["type"]</c>.</param>
    /// <returns>The resulting Service Bus message, or <see langword="null"/> when serialization fails or the body exceeds the size limit.</returns>
    [Pure]
    ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message;
}
