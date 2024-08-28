using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Flight.Airports.Features.CreateAirport;

[Route(BaseApiPath + "/flight/airport")]
public class CreateAirportEndpoint : BaseController
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create new airport", Description = "Create new airport")]
    public async Task<ActionResult> Create([FromBody] CreateAirportCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
