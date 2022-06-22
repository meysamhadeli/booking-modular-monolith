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
    private readonly ILogger<EfTxBehavior<TRequest, TResponse>> _logger;
    private readonly BookingDbContext _bookingDbContext;
    private readonly IBusPublisher _busPublisher;

    public EfTxBookingBehavior(
        ILogger<EfTxBehavior<TRequest, TResponse>> logger,
        IBusPublisher busPublisher, BookingDbContext bookingDbContext)
    {
        _logger = logger;
        _busPublisher = busPublisher;
        _bookingDbContext = bookingDbContext;
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

        await _bookingDbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "{Prefix} Executed the {MediatrRequest} request",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var domainEvents = _bookingDbContext.GetDomainEvents();

            await _busPublisher.SendAsync(domainEvents.ToArray(), cancellationToken);

            await _bookingDbContext.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            await _bookingDbContext.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
