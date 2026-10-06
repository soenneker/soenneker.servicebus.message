using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Serialization;
using System.Diagnostics.Contracts;
using Azure.Messaging.ServiceBus;

namespace Soenneker.ServiceBus.Message.Abstract;

/// <summary>
/// Builds Azure Service Bus messages from Soenneker message envelopes.
/// </summary>
/// <remarks>Messages use System.Text.Json. The default method uses reflection-based web JSON defaults.
/// Pass a generated context to the method overload for trimmed or Native AOT applications.</remarks>
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
    [RequiresUnreferencedCode("The legacy serializer uses reflection. Supply a generated JsonSerializerContext instead.")]
    [RequiresDynamicCode("The legacy serializer may require runtime code generation. Supply a generated JsonSerializerContext instead.")]
    ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message;

    /// <summary>Builds a message using generated metadata for the runtime message type.</summary>
    /// <typeparam name="TMessage">The message envelope type.</typeparam>
    /// <param name="message">Message content to send.</param>
    /// <param name="type">The stable type stored in the message application properties.</param>
    /// <param name="jsonContext">Generated metadata covering the message and its payload.</param>
    /// <returns>The built message, or null when serialization fails or the body exceeds the size limit.</returns>
    ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type, JsonSerializerContext jsonContext) where TMessage : Messages.Base.Message;
}
