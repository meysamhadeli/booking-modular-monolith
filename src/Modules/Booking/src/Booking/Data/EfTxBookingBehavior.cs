using System.Text.Json;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Booking.Data;

public class EfTxBookingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<EfTxBookingBehavior<TRequest, TResponse>> _logger;
    private readonly BookingDbContext _dbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxBookingBehavior(
        ILogger<EfTxBookingBehavior<TRequest, TResponse>> logger,
        IBusPublisher busPublisher, BookingDbContext dbContext)
    {
        _logger = logger;
        _busPublisher = busPublisher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation(
            "{Prefix} Handled command {MediatrRequest}",
            nameof(EfTxBookingBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);

        _logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxBookingBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        _logger.LogInformation(
            "{Prefix} Open the transaction for {MediatrRequest}",
            nameof(EfTxBookingBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);


        var response = await next();

        while (true)
        {
            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxBookingBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _dbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            // ref: https://learn.microsoft.com/en-us/ef/ef6/fundamentals/connection-resiliency/retry-logic?redirectedfrom=MSDN#solution-manually-call-execution-strategy
            await _dbContext.ExecuteTransactionalAsync(cancellationToken);
            
            return response;
        }
    }
}