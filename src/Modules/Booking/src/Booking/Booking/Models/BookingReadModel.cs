using Booking.Booking.Models.ValueObjects;

namespace Booking.Booking.Models;

public class BookingReadModel
{
    public required long Id { get; init; }
    public required long BookId { get; init; }
    public required Trip Trip { get; init; }
    public required PassengerInfo PassengerInfo { get; init; }
    public required bool IsDeleted { get; init; }
}