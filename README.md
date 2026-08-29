[![](https://img.shields.io/nuget/v/Soenneker.ServiceBus.Message.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.ServiceBus.Message/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.message/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.message/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.ServiceBus.Message.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.ServiceBus.Message/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.message/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.message/actions/workflows/codeql.yml)

# Soenneker.ServiceBus.Message

A utility library for building Azure Service messages Singleton IoC.

## Install

```bash
dotnet add package Soenneker.ServiceBus.Message
```

## Quick start

```csharp
using Soenneker.ServiceBus.Message.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddServiceBusMessageUtilAsSingleton();
```

Registers Service Bus Message Util with a singleton lifetime.

## What you get

- `IServiceBusMessageUtil` — A utility library for building Azure Service messages Singleton IoC.
- `ServiceBusMessageUtilRegistrar` — A utility library for building Azure Service messages.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ServiceBusMessageUtilRegistrar.AddServiceBusMessageUtilAsSingleton(services)` | Registers Service Bus Message Util with a singleton lifetime. | The same service collection, so additional registrations can be chained. |
| `ServiceBusMessageUtilRegistrar.AddServiceBusMessageUtilAsScoped(services)` | Registers Service Bus Message Util with a scoped lifetime. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Calls that return a cached or singleton value reuse the same instance until the owning service is disposed.
