using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Flight.Data;

public class EfTxFlightBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxBehavior<TRequest, TResponse>> _logger;
    private readonly FlightDbContext _flightDbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxFlightBehavior(
        ILogger<EfTxBehavior<TRequest, TResponse>> logger,
        FlightDbContext flightDbContext,
        IBusPublisher busPublisher)
    {
        _logger = logger;
        _flightDbContext = flightDbContext;
        _busPublisher = busPublisher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation(
            "{Prefix} Handled command {MediatrRequest}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        _logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        _logger.LogInformation(
            "{Prefix} Open the transaction for {MediatrRequest}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        await _flightDbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _flightDbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            await _flightDbContext.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            await _flightDbContext.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
