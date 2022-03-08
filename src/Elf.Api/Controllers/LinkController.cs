﻿using Elf.Api.Features;
using Elf.MultiTenancy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.FeatureManagement;

namespace Elf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LinkController : ControllerBase
{
    private readonly ILinkVerifier _linkVerifier;
    private readonly IDistributedCache _cache;
    private readonly IFeatureManager _featureManager;
    private readonly Tenant _tenant;
    private readonly IMediator _mediator;

    public LinkController(
        ITenantAccessor<Tenant> tenantAccessor,
        ILinkVerifier linkVerifier,
        IDistributedCache cache,
        IFeatureManager featureManager, IMediator mediator)
    {
        _linkVerifier = linkVerifier;
        _cache = cache;
        _featureManager = featureManager;
        _mediator = mediator;

        _tenant = tenantAccessor.Tenant;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(LinkEditModel model)
    {
        var flag = await _featureManager.IsEnabledAsync(nameof(FeatureFlags.AllowSelfRedirection));
        var verifyResult = _linkVerifier.Verify(model.OriginUrl, Url, Request, flag);
        switch (verifyResult)
        {
            case LinkVerifyResult.InvalidFormat:
                return BadRequest("Not a valid URL.");
            case LinkVerifyResult.InvalidLocal:
                return BadRequest("Can not use local URL.");
            case LinkVerifyResult.InvalidSelfReference:
                return BadRequest("Can not use url pointing to this site.");
        }

        var createLinkRequest = new CreateLinkRequest
        {
            OriginUrl = model.OriginUrl,
            Note = model.Note,
            AkaName = string.IsNullOrWhiteSpace(model.AkaName) ? null : model.AkaName,
            IsEnabled = model.IsEnabled,
            TTL = model.TTL,
            TenantId = _tenant.Id
        };

        var response = await _mediator.Send(new CreateLinkCommand(createLinkRequest));
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Edit(int id, LinkEditModel model)
    {
        var flag = await _featureManager.IsEnabledAsync(nameof(FeatureFlags.AllowSelfRedirection));
        var verifyResult = _linkVerifier.Verify(model.OriginUrl, Url, Request, flag);
        switch (verifyResult)
        {
            case LinkVerifyResult.InvalidFormat:
                return BadRequest("Not a valid URL.");
            case LinkVerifyResult.InvalidLocal:
                return BadRequest("Can not use local URL.");
            case LinkVerifyResult.InvalidSelfReference:
                return BadRequest("Can not use url pointing to this site.");
        }

        var token = await _mediator.Send(new EditLinkCommand(id, model));
        if (token is not null) await _cache.RemoveAsync(token);
        return Ok(token);
    }

    [HttpPut("{id:int}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetEnable(int id, bool isEnabled)
    {
        await _mediator.Send(new SetEnableCommand(id, isEnabled));
        return NoContent();
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedLinkResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(string term, int take, int offset)
    {
        var data = await _mediator.Send(new ListLinkQuery(offset, take, term));

        var result = new PagedLinkResult
        {
            Links = data.Links,
            TotalRows = data.TotalRows,
            PageSize = take
        };

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LinkEditModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var link = await _mediator.Send(new GetLinkQuery(id));
        if (link is null) return NotFound();

        var model = new LinkEditModel
        {
            Note = link.Note,
            AkaName = link.AkaName,
            OriginUrl = link.OriginUrl,
            IsEnabled = link.IsEnabled,
            TTL = link.TTL ?? 0
        };

        return Ok(model);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(LinkEditModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var link = await _mediator.Send(new GetLinkQuery(id));
        if (link is null) return NotFound();

        await _mediator.Send(new DeleteLinkCommand(id));

        await _cache.RemoveAsync(link.FwToken);
        return Ok();
    }
}
