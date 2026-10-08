using AutoBogus;

namespace Integration.Test.Fakes;

using global::Identity.Identity.Features.RegisteringNewUser.V1;

public class FakeRegisterNewUserCommand : AutoFaker<RegisterNewUser>
{
    public FakeRegisterNewUserCommand()
    {
        // Keep these values deterministic AND short on purpose.
        // The Identity test boots the whole modular-monolith host, so the published
        // `UserCreated` event (Id, FirstName + " " + LastName, PassPortNumber) is handled
        // in-process by the Passenger module. Passenger stores `PassportNumber` as
        // `character varying(10)` and `Name` as `character varying(50)`, so random AutoBogus
        // values overflow the column and fail with Postgres `22001: value too long`.
        // (In the microservices version each service runs in its own process, so this only
        // surfaces in the modular monolith.)
        RuleFor(r => r.FirstName, _ => "Sam");
        RuleFor(r => r.LastName, _ => "H");
        RuleFor(r => r.Username, x => "TestMyUser");
        RuleFor(r => r.Password, _ => "Password@123");
        RuleFor(r => r.ConfirmPassword, _ => "Password@123");
        RuleFor(r => r.Email, _ => "test@test.com");
        RuleFor(r => r.PassportNumber, _ => "123456789");
    }
}
