using BuildingBlocks.Web;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Passenger.Passengers.Features.GetPassengerById;

[Route(BaseApiPath + "/passenger")]
public class GetPassengerByIdEndpoint : BaseController
{
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Get passenger by id", Description = "Get passenger by id")]
    public async Task<ActionResult> GetById([FromRoute] GetPassengerQueryById query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
