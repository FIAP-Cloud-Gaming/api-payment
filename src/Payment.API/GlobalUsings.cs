global using System.Reflection;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.OpenApi;
global using Microsoft.OpenApi;

global using FluentValidation;

global using Mediator;
global using Mediator.Commands;
global using Mediator.Queries;
global using Mediator.Notifications;
global using Mediator.Behaviors;
global using Mediator.Extensions;

global using Payment.Domain.AggregatesModel.PaymentAggregate;
global using Payment.Domain.ValueObjects;
global using Payment.Domain.Events;
global using Payment.Domain.Exceptions;

global using Payment.Infrastructure;

global using Payment.API.Application.Models;
global using Payment.API.Application.Behaviors;
global using Payment.API.Application.IntegrationEvents.Events;

global using Elastic.Serilog.Sinks;

global using Serilog;

global using Scalar.AspNetCore;

global using EventBus.Events;
global using EventBus.Abstractions;
