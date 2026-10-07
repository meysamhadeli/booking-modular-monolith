using AutoBogus;
using Griffin.Core.EventBus.Messages;

namespace Integration.Test.Fakes;


public class FakeUserCreated : AutoFaker<UserCreated>
{
    public FakeUserCreated()
    {
        RuleFor(r => r.Id, _ => NewId.NextGuid());
        RuleFor(r => r.Name, _ => "Sam");
        RuleFor(r => r.PassportNumber, _ => "123456789");
    }
}