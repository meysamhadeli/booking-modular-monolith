// using Ardalis.GuardClauses;
// using BuildingBlocks.Contracts.EventBus.Messages;
// using DotNetCore.CAP;
// using Passenger.Data;
//
// namespace Passenger.Identity.RegisterNewUser;
//
// public class RegisterNewUserConsumerHandler : ICapSubscribe
// {
//     private readonly PassengerDbContext _passengerDbContext;
//
//     public RegisterNewUserConsumerHandler(PassengerDbContext passengerDbContext)
//     {
//         _passengerDbContext = passengerDbContext;
//     }
//
//     [CapSubscribe(nameof(UserCreated))]
//     public async Task Consume(UserCreated userCreated)
//     {
//         Guard.Against.Null(userCreated, nameof(UserCreated));
//
//         var passenger =
//             Passengers.Models.Passenger.Create(userCreated.Id, userCreated.Name, userCreated.PassportNumber);
//
//         await _passengerDbContext.AddAsync(passenger);
//
//         await _passengerDbContext.SaveChangesAsync();
//     }
// }