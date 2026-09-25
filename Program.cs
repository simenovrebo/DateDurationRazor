using DateDurationRazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.MapGet("/api/duration", (
    DateOnly start,
    DateOnly end,
    TimeOnly? startTime,
    TimeOnly? endTime,
    string? startZone,
    string? endZone,
    bool includeEndDate = false,
    bool countOnlyWorkdays = false) =>
{
    var startTz = TimeZones.Find(startZone);
    var endTz = TimeZones.Find(endZone);

    if (!string.IsNullOrWhiteSpace(startZone) && startTz is null)
        return Results.BadRequest(new { error = $"Unknown time zone '{startZone}'." });
    if (!string.IsNullOrWhiteSpace(endZone) && endTz is null)
        return Results.BadRequest(new { error = $"Unknown time zone '{endZone}'." });

    // A single zone applies to both sides.
    startTz ??= endTz;
    endTz ??= startTz;

    var result = DateDurationCalculator.Calculate(
        start, end, startTime, endTime, startTz, endTz, includeEndDate, countOnlyWorkdays);
    return Results.Json(result);
});

app.MapGet("/api/timezones", () => Results.Json(TimeZones.Ids));

app.Run();
