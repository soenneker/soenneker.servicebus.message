using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Runtime.Serialization;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.ServiceBus.Message.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Message;

public sealed class ServiceBusMessageUtil : IServiceBusMessageUtil
{
    private const int _messageLimitBytes = 260_096;
    private static readonly Encoding _utf8 = Encoding.UTF8;

    private readonly bool _log;
    private readonly ILogger<ServiceBusMessageUtil> _logger;

    public ServiceBusMessageUtil(IConfiguration config, ILogger<ServiceBusMessageUtil> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _log = config.GetValue<bool>("Azure:ServiceBus:Log");
    }

    public ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type) where TMessage : Messages.Base.Message
    {
        return BuildMessageStjUtf8(message, type, static value => JsonUtil.SerializeToUtf8Bytes(value), static value => JsonUtil.Serialize(value));
    }

    public ServiceBusMessage? BuildMessage<TMessage>(TMessage message, string type, JsonSerializerContext jsonContext) where TMessage : Messages.Base.Message
    {
        ArgumentNullException.ThrowIfNull(jsonContext);
        return BuildMessageStjUtf8(message, type,
            value => JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), jsonContext),
            value => JsonSerializer.Serialize(value, value.GetType(), jsonContext));
    }

    private ServiceBusMessage? BuildMessageStjUtf8<TMessage>(TMessage message, string type, Func<TMessage, byte[]> serialize,
        Func<TMessage, string?> serializeForLog) where TMessage : Messages.Base.Message
    {
        try
        {
            byte[]? utf8Bytes = serialize(message);

            if (utf8Bytes is null)
                throw new SerializationException("Couldn't serialize message of type " + type);

            int size = utf8Bytes.Length;

            if (size > _messageLimitBytes)
            {
                _logger.LogError("== ServiceBusMessageUtil: Message size is over limit. Type: {Type}, Size: {SizeBytes} bytes", type, size);

                return null;
            }

            if (_log && _logger.IsEnabled(LogLevel.Debug))
            {
                string payload = _utf8.GetString(utf8Bytes);
                _logger.LogDebug("Creating message ({Type}): {Message}", type, payload);
            }

            var sbMessage = new ServiceBusMessage(utf8Bytes)
            {
                ApplicationProperties =
                {
                    ["type"] = type
                }
            };

            return sbMessage;
        }
        catch (Exception ex)
        {
            LogCriticalError(ex, type, message, serializeForLog);
            return null;
        }
    }

    private void LogCriticalError<TMessage>(Exception ex, string type, TMessage message, Func<TMessage, string?> serialize)
    {
        if (!_logger.IsEnabled(LogLevel.Critical))
            return;

        if (_log)
        {
            try
            {
                string? serialized = serialize(message);

                _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}, Message: {Message}", type, serialized);
            }
            catch
            {
                _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}; payload could not be serialized for logging", type);
            }
        }
        else
        {
            _logger.LogCritical(ex, "== ServiceBusMessageUtil: Error building service bus message. Type: {Type}", type);
        }
    }
}
