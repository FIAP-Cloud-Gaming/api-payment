global using Xunit;
global using FluentAssertions;
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;

global using Payment.Domain.AggregatesModel.PaymentAggregate;
global using Payment.Domain.ValueObjects;
global using Payment.Domain.Events;
global using Payment.Domain.Exceptions;

global using Payment.Infrastructure;
global using Payment.Infrastructure.Repositories;

global using Payment.API.Application.Commands.CreatePayment;
global using Payment.API.Application.Commands.ProcessPayment;
global using Payment.API.Application.Commands.CancelPayment;
global using Payment.API.Application.Commands.RefundPayment;
global using Payment.API.Application.Queries.GetPayment;
global using Payment.API.Application.Queries.GetPaymentsByUser;
global using Payment.API.Application.Queries.GetPaymentsByStatus;
global using Payment.API.Application.Behaviors;
global using Payment.API.Application.Models;
global using Payment.API.Middleware;

global using Mediator;
global using Mediator.Commands;
global using Mediator.Queries;
global using Mediator.Notifications;
global using Mediator.Behaviors;

global using Core.SeedWork;

global using FluentValidation;
global using FluentValidation.Results;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.AspNetCore.Http;
