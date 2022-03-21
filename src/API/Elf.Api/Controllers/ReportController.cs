﻿using Elf.Api.Features;
using System.ComponentModel.DataAnnotations;

namespace Elf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public ReportController(IConfiguration configuration, IMediator mediator)
    {
        _configuration = configuration;
        _mediator = mediator;
    }

    [HttpGet("requests")]
    [ProducesResponseType(typeof(IReadOnlyList<RequestTrack>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Requests(
        [Range(1, int.MaxValue)] int take,
        [Range(0, int.MaxValue)] int offset)
    {
        var requests = await _mediator.Send(new GetRecentRequestsQuery(offset, take));
        return Ok(requests);
    }

    [HttpGet("requests/link/{daysFromNow:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<MostRequestedLinkCount>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MostRequestedLinks([Range(1, 90)] int daysFromNow)
    {
        var linkCounts = await _mediator.Send(new GetMostRequestedLinkCountQuery(daysFromNow));
        return Ok(linkCounts);
    }

    [HttpGet("requests/clienttype/{daysFromNow:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientTypeCount>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClientType([Range(1, 90)] int daysFromNow)
    {
        var types = await _mediator.Send(new GetClientTypeCountsQuery(daysFromNow,
            _configuration.GetSection("AppSettings:TopClientTypes").Get<int>()));
        return Ok(types);
    }

    [HttpGet("tracking/{daysFromNow:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LinkTrackingDateCount>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackingCount([Range(1, 90)] int daysFromNow)
    {
        var dateCounts = await _mediator.Send(new GetLinkTrackingDateCountQuery(daysFromNow));
        return Ok(dateCounts);
    }

    [HttpDelete("tracking/clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearTrackingData()
    {
        await _mediator.Send(new ClearTrackingDataCommand());
        return NoContent();
    }
}
