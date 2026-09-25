using System;
using System.ComponentModel.DataAnnotations;
using DateDurationRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DateDurationRazor.Pages;

public class IndexModel : PageModel
{
    [BindProperty, DataType(DataType.Date), Required]
    public DateOnly? Start { get; set; }

    [BindProperty, DataType(DataType.Date), Required]
    public DateOnly? End { get; set; }

    [BindProperty, DataType(DataType.Time)]
    public TimeOnly? StartTime { get; set; }

    [BindProperty, DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; }

    [BindProperty]
    public string? StartZone { get; set; }

    [BindProperty]
    public string? EndZone { get; set; }

    [BindProperty]
    public bool IncludeEndDate { get; set; }

    [BindProperty]
    public bool CountOnlyWorkdays { get; set; }

    /// <summary>Whether the time-of-day fields are open. Persisted through the postback.</summary>
    [BindProperty]
    public bool ShowTime { get; set; }

    /// <summary>Whether the time zone fields are open. Persisted through the postback.</summary>
    [BindProperty]
    public bool ShowZones { get; set; }

    public DateDurationResult? Result { get; private set; }

    public void OnGet()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        Start = today;
        End = today;
        StartZone = TimeZones.LocalId;
        EndZone = TimeZones.LocalId;
    }

    public void OnPost()
    {
        // Closed sections contribute nothing, whatever their hidden inputs still hold.
        if (!ShowTime) { StartTime = null; EndTime = null; }

        TimeZoneInfo? startTz = null, endTz = null;
        if (ShowZones)
        {
            startTz = TimeZones.Find(StartZone);
            endTz = TimeZones.Find(EndZone);

            if (!string.IsNullOrWhiteSpace(StartZone) && startTz is null)
                ModelState.AddModelError(nameof(StartZone), $"Unknown time zone '{StartZone}'.");
            if (!string.IsNullOrWhiteSpace(EndZone) && endTz is null)
                ModelState.AddModelError(nameof(EndZone), $"Unknown time zone '{EndZone}'.");

            // A single zone applies to both sides.
            startTz ??= endTz;
            endTz ??= startTz;
        }

        if (!ModelState.IsValid || Start is null || End is null)
            return;

        Result = DateDurationCalculator.Calculate(
            Start.Value, End.Value, StartTime, EndTime, startTz, endTz, IncludeEndDate, CountOnlyWorkdays);
    }
}
