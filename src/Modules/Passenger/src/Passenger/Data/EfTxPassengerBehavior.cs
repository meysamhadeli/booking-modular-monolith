using System.Text.Json;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Passenger.Data;

public class EfTxPassengerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxBehavior<TRequest, TResponse>> _logger;
    private readonly PassengerDbContext _passengerDbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxPassengerBehavior(
        ILogger<EfTxBehavior<TRequest, TResponse>> logger,
        IBusPublisher busPublisher, PassengerDbContext passengerDbContext)
    {
        _logger = logger;
        _busPublisher = busPublisher;
        _passengerDbContext = passengerDbContext;
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

        await _passengerDbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _passengerDbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            await _passengerDbContext.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            await _passengerDbContext.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
