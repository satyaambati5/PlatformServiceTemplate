using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformServiceTemplate.Api.Contracts.Common;

namespace PlatformServiceTemplate.Api.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
[Authorize(Policy = "WebhookPolicy")]
public sealed class WebhooksController : ControllerBase
{
    [HttpPost("events")]
    public IActionResult Receive([FromBody] object payload)
        => Accepted(ApiResponse<object>.Ok(new { Received = true, payload }, "Webhook accepted.", HttpContext.TraceIdentifier));
}
