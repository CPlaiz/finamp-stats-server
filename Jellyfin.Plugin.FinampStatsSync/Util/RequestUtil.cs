using System;
using System.Linq;
using System.Security.Claims;

namespace Jellyfin.Plugin.FinampStatsSync.Util;

public static class RequestUtil
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = GetClaimValue(user, "Jellyfin-UserId");

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Guid.Parse(value);
    }

    private static string? GetClaimValue(in ClaimsPrincipal user, string name)
        => user.Claims.FirstOrDefault(claim => claim.Type.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
}
