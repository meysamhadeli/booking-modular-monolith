using Booking.Booking.Events.Domain;
using Booking.Data;
using BuildingBlocks.EventStoreDB.Events;
using BuildingBlocks.EventStoreDB.Projections;
using BuildingBlocks.IdsGenerator;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Booking;

using Booking.Models;
using MassTransit;

public class BookingProjection : IProjectionProcessor
{
    private readonly BookingReadDbContext _bookingReadDbContext;

    public BookingProjection(BookingReadDbContext bookingReadDbContext)
    {
        _bookingReadDbContext = bookingReadDbContext;
    }

    public async Task ProcessEventAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification
    {
        switch (streamEvent.Data)
        {
            case BookingCreatedDomainEvent bookingCreatedDomainEvent:
                await Apply(bookingCreatedDomainEvent, cancellationToken);
                break;
        }
    }

    private async Task Apply(BookingCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var reservation =
            await _bookingReadDbContext.Booking.AsQueryable().SingleOrDefaultAsync(x => x.Id == @event.Id && !x.IsDeleted,
                cancellationToken);

        if (reservation == null)
        {
            var bookingReadModel = new BookingReadModel
            {
                Id = SnowFlakIdGenerator.NewId(),
                Trip = @event.Trip,
                BookId = @event.Id,
                PassengerInfo = @event.PassengerInfo,
                IsDeleted = @event.IsDeleted
            };

            await _bookingReadDbContext.Booking.InsertOneAsync(bookingReadModel, cancellationToken: cancellationToken);
        }
    }
}