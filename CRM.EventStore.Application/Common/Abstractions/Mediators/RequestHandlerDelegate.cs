using CRM.EventStore.Domain.Common.Models;

namespace CRM.EventStore.Application.Common.Abstractions.Mediators;

public delegate ValueTask<Result<TResponse>> RequestHandlerDelegate<TResponse>();
