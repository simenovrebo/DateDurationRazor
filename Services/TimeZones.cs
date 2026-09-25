using System;
using System.Collections.Generic;
using System.Linq;

namespace DateDurationRazor.Services;

/// <summary>Lookup helpers over the IANA zones known to the host.</summary>
public static class TimeZones
{
    /// <summary>All zone ids, sorted, for the search list on the page.</summary>
    public static IReadOnlyList<string> Ids { get; } =
        TimeZoneInfo.GetSystemTimeZones()
            .Select(z => z.Id)
            .Where(id => id.Contains('/'))          // skip legacy aliases like "EST" and "CET"
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// The host's own zone id, e.g. "Europe/Oslo". .NET can report an alias of the same zone
    /// (macOS gives "Atlantic/Jan_Mayen" for Oslo), so the OS's /etc/localtime link is
    /// preferred when it names a zone we know.
    /// </summary>
    public static string LocalId { get; } = ResolveLocalId();

    private static string ResolveLocalId()
    {
        try
        {
            var target = new System.IO.FileInfo("/etc/localtime").LinkTarget;
            const string marker = "zoneinfo/";
            var at = target?.IndexOf(marker, StringComparison.Ordinal) ?? -1;
            if (target is not null && at >= 0)
            {
                var id = target[(at + marker.Length)..];
                if (Ids.Contains(id)) return id;
            }
        }
        catch (Exception)
        {
            // Not a symlink, no such file, or no permission: fall through to .NET's answer.
        }

        return TimeZoneInfo.Local.Id;
    }

    /// <summary>Quick picks shown under the search box.</summary>
    public static IReadOnlyList<string> Suggestions { get; } =
        new[] { LocalId, "UTC", "Europe/London", "America/New_York", "Asia/Tokyo" }
            .Distinct()
            .ToList();

    /// <summary>Resolves an IANA id or Windows id. Returns null when the text is blank or unknown.</summary>
    public static TimeZoneInfo? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return TimeZoneInfo.TryFindSystemTimeZoneById(id.Trim(), out var zone) ? zone : null;
    }
}
