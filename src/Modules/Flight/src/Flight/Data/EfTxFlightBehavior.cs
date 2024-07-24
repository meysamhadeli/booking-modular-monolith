using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Flight.Data;

public class EfTxFlightBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxFlightBehavior<TRequest, TResponse>> _logger;
    private readonly FlightDbContext _dbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxFlightBehavior(
        ILogger<EfTxFlightBehavior<TRequest, TResponse>> logger,
        IBusPublisher busPublisher, FlightDbContext dbContext)
    {
        _logger = logger;
        _busPublisher = busPublisher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
    "{Prefix} Handled command {MediatrRequest}",
    nameof(EfTxFlightBehavior<TRequest, TResponse>),
    typeof(TRequest).FullName);

        _logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxFlightBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        _logger.LogInformation(
            "{Prefix} Open the transaction for {MediatrRequest}",
            nameof(EfTxFlightBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);


        var response = await next();

        while (true)
        {
            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxFlightBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _dbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            // ref: https://learn.microsoft.com/en-us/ef/ef6/fundamentals/connection-resiliency/retry-logic?redirectedfrom=MSDN#solution-manually-call-execution-strategy
            await _dbContext.ExecuteTransactionalAsync(cancellationToken);

            return response;
        }
    }
}