using BuildingBlocks.Web;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Booking.Booking.Features.CreateBooking;

[Route(BaseApiPath + "/booking")]
public class CreateBookingEndpoint : BaseController
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create new Reservation", Description = "Create new Reservation")]
    public async Task<ActionResult> CreateReservation([FromBody] CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
