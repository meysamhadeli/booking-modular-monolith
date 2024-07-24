using Mapster;
using Passenger.Passengers.Dtos;

namespace Passenger.Passengers.Features;

public class ReservationMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.Passenger, PassengerResponseDto>();
    }
}
