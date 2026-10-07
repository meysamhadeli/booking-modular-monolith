using System.Text.Json;
using System.Transactions;
using Griffin.Core;
using Griffin.Polly;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Flight.Data;

public class EfTxFlightBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxFlightBehavior<TRequest, TResponse>> _logger;
    private readonly FlightDbContext _flightDbContext;
    private readonly IEventDispatcher _eventDispatcher;

    public EfTxFlightBehavior(
        ILogger<EfTxFlightBehavior<TRequest, TResponse>> logger,
        FlightDbContext flightDbContext,
        IEventDispatcher eventDispatcher
    )
    {
        _logger = logger;
        _flightDbContext = flightDbContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "{Prefix} Handled command {MediatrRequest}",
            GetType().Name,
            typeof(TRequest).FullName);

        _logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            GetType().Name,
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        var response = await next();

        _logger.LogInformation(
            "{Prefix} Executed the {MediatrRequest} request",
            GetType().Name,
            typeof(TRequest).FullName);

        while (true)
        {
            var domainEvents = _flightDbContext.GetDomainEvents();

            if (domainEvents is null || !domainEvents.Any())
            {
                return response;
            }

            _logger.LogInformation(
                "{Prefix} Open the transaction for {MediatrRequest}",
                GetType().Name,
                typeof(TRequest).FullName);

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            await _eventDispatcher.SendAsync(
                domainEvents.ToArray(),
                typeof(TRequest),
                cancellationToken);

            // Save data to database with some retry policy in distributed transaction
            await _flightDbContext.RetryOnFailure(
                async () =>
                {
                    await _flightDbContext.SaveChangesAsync(cancellationToken);
                });

            scope.Complete();

            return response;
        }
    }
}
