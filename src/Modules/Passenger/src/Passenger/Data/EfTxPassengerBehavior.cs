using System.Text.Json;
using BuildingBlocks.Domain;
using MediatR;

namespace Passenger.Data;

public class EfTxPassengerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxPassengerBehavior<TRequest, TResponse>> _logger;
    private readonly PassengerDbContext _dbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxPassengerBehavior(
        ILogger<EfTxPassengerBehavior<TRequest, TResponse>> logger,
        IBusPublisher busPublisher, PassengerDbContext dbContext)
    {
        _logger = logger;
        _busPublisher = busPublisher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
           "{Prefix} Handled command {MediatrRequest}",
           nameof(EfTxPassengerBehavior<TRequest, TResponse>),
           typeof(TRequest).FullName);

        _logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxPassengerBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        _logger.LogInformation(
            "{Prefix} Open the transaction for {MediatrRequest}",
            nameof(EfTxPassengerBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);


        var response = await next();

        while (true)
        {
            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxPassengerBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _dbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            // ref: https://learn.microsoft.com/en-us/ef/ef6/fundamentals/connection-resiliency/retry-logic?redirectedfrom=MSDN#solution-manually-call-execution-strategy
            await _dbContext.ExecuteTransactionalAsync(cancellationToken);

            return response;
        }
    }
}