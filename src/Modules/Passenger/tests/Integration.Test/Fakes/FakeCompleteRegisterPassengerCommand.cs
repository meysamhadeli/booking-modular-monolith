using AutoBogus;
using Passenger.Passengers.Enums;

namespace Integration.Test.Fakes;

using global::Passenger.Passengers.Features.CompletingRegisterPassenger.V1;
using MassTransit;

public sealed class FakeCompleteRegisterPassengerCommand : AutoFaker<CompleteRegisterPassenger>
{
    public FakeCompleteRegisterPassengerCommand(string passportNumber, Guid passengerId)
    {
        RuleFor(r => r.Id, _ => passengerId);
        RuleFor(r => r.PassportNumber, _ => passportNumber);
        RuleFor(r => r.PassengerType, _ => PassengerType.Male);
        RuleFor(r => r.Age, _ => 30);
    }
}
