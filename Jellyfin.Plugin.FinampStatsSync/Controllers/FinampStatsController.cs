using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Jellyfin.Data.Entities.Security;
using Jellyfin.Plugin.FinampStatsSync.Database;
using Jellyfin.Plugin.FinampStatsSync.Model;
using Jellyfin.Plugin.FinampStatsSync.Util;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.FinampStatsSync.Controllers;

/// <summary>
/// Extended API for MediaSegments Management.
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[Route("FinampStats")]
public class FinampStatsController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinampStatsController"/> class.
    /// </summary>
    /// <param name="userManager">The User manager.</param>
    /// <param name="db">The DB Context.</param>
    public FinampStatsController(IUserManager userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
        _db.Database.EnsureCreated();
    }

    /// <summary>
    /// Get stats for device.
    /// </summary>
    /// <returns>The created segment.</returns>
    [HttpPost("AddTrackItems")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    #pragma warning disable SA1611, CA1002
    public async Task<IActionResult> AddTrackItems([FromBody] List<TrackItem> trackItems)
    {
        var userId = User.GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Attach the userId
        foreach (var track in trackItems)
        {
            track.UserId = userId.Value;
            track.CreatedAt = createdAt;
        }

        // Get existing StartTimes for this user
        var existingStartTimes = await _db.TrackItems
            .Where(t => t.UserId == userId.Value)
            .Select(t => t.StartTime)
            .ToListAsync()
            .ConfigureAwait(false);

        // Filter out items with duplicate StartTime
        var newTrackItems = trackItems
            .Where(t => !existingStartTimes.Contains(t.StartTime))
            .ToList();

        // Save new track items
        _db.TrackItems.AddRange(newTrackItems);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return Ok();
    }

    [HttpGet("GetUserTrackItems")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserTrackItemsAsync([FromQuery] long? since = null)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = _db.TrackItems.Where(t => t.UserId == userId.Value);

        if (since.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= since.Value);
        }

        var tracks = await query.ToListAsync().ConfigureAwait(false);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

        return new JsonResult(tracks, options);
    }
}
