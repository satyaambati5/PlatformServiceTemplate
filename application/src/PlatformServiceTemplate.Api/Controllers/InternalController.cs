using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformServiceTemplate.Api.Contracts.Common;

namespace PlatformServiceTemplate.Api.Controllers;

[ApiController]
[Route("api/v1/internal")]
[Authorize(Policy = "InternalPolicy")]
public sealed class InternalController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(ApiResponse<object>.Ok(new { Message = "internal-ok" }, "Internal ping ok.", HttpContext.TraceIdentifier));
}
