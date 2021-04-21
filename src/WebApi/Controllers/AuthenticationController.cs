using Core.Authentication.Commands;
using Core.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("init")]
        public async Task<IActionResult> Initialization()
        {
            await _mediator.Send(new GetAuthenticationCodeQuery());

            return Ok();
        }

        [HttpGet("validation")]
        public async Task<IActionResult> Validation(string code)
        {
            return Ok(await _mediator.Send(new SaveAccessTokenCommand(code)));
        }
    }
}
