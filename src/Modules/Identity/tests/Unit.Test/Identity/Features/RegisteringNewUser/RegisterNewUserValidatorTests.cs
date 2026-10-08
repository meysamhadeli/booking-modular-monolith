using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Unit.Test.Identity.Features.RegisteringNewUser;

using global::Identity.Identity.Features.RegisteringNewUser.V1;

public class RegisterNewUserValidatorTests
{
    private readonly RegisterNewUserValidator _validator = new RegisterNewUserValidator();

    [Fact]
    public void is_valid_should_be_true_when_all_parameters_are_valid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void is_valid_should_be_false_when_full_name_is_longer_than_passenger_name_column()
    {
        // Arrange
        // `UserCreated` carries `FirstName + " " + LastName` as its `Name`, and in this modular
        // monolith the Passenger module persists that in-process into a `character varying(50)`
        // column, so an over-long combined name would fail there instead of being rejected here.
        var command = CreateValidCommand() with
        {
            FirstName = new string('a', 30),
            LastName = new string('b', 30),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void is_valid_should_depend_on_passport_number_length(int length, bool expected)
    {
        // Arrange
        // The Passenger module stores `PassportNumber` as `character varying(10)`, so a longer
        // value overflows the column with `22001: value too long for type character varying(10)`.
        var command = CreateValidCommand() with
        {
            PassportNumber = new string('9', length),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().Be(expected);

        if (expected)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.PassportNumber);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.PassportNumber);
        }
    }

    private static RegisterNewUser CreateValidCommand()
    {
        return new RegisterNewUser(
            "Sam",
            "H",
            "TestMyUser",
            "test@test.com",
            "Password@123",
            "Password@123",
            "123456789"
        );
    }
}
