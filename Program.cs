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
    bool includeEndDate = false) =>
{
    var result = DateDurationCalculator.Calculate(start, end, includeEndDate);
    return Results.Json(result);
});

app.Run();
