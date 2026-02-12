global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

global using Payment.Domain.AggregatesModel.PaymentAggregate;
global using Payment.Domain.Events;
global using Payment.Infrastructure.IntegrationEvents.Events;

global using Core.SeedWork;

global using EventBus.Abstractions;
global using EventBus.Events;

global using Mediator;
global using Mediator.Extensions;
global using Mediator.Notifications;
