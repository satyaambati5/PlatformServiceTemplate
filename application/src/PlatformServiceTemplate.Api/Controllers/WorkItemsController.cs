using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using PlatformServiceTemplate.Api.Contracts.Common;
using PlatformServiceTemplate.Api.Contracts.WorkItems;
using PlatformServiceTemplate.Application.Features.WorkItems.Commands.CreateWorkItem;
using PlatformServiceTemplate.Application.Features.WorkItems.Commands.UpdateWorkItemStatus;
using PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItemById;
using PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItems;

namespace PlatformServiceTemplate.Api.Controllers;

[ApiController]
[Route("api/v1/work-items")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "JwtPolicy")]
public sealed class WorkItemsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await sender.Send(new GetWorkItemsQuery(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(data, "Work items fetched.", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var data = await sender.Send(new GetWorkItemByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(data, "Work item fetched.", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkItemRequest request, CancellationToken cancellationToken)
    {
        var created = await sender.Send(new CreateWorkItemCommand(request.Title, request.Description), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, ApiResponse<object>.Ok(created, "Work item created.", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateWorkItemStatusRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateWorkItemStatusCommand(id, request.Status), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Work item status updated.", HttpContext.TraceIdentifier));
    }
}
