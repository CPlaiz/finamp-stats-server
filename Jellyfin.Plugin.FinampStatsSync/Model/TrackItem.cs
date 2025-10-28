using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using J2N.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.FinampStatsSync.Model;

public class TrackItem
{
    public int Id { get; set; }

    public required string TrackId { get; set; }

    #pragma warning disable CA1002, CA2227
    public required List<string> ArtistIds { get; set; }

    public required long Duration { get; set; }

    public required long StartTime { get; set; }

    [JsonIgnore]
    public Guid? UserId { get; set; }

    [JsonIgnore]
    public long CreatedAt { get; set; }
}
