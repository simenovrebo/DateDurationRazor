# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this is

An ASP.NET Core Razor Pages app (.NET 10) that computes the duration between two dates
as years/months/days plus total days, mirroring timeanddate.com's date duration
calculator. There is a web form at `/` and a JSON endpoint at `/api/duration`.

## Build and run

```bash
dotnet build        # Release only; there is no Debug configuration
dotnet run          # starts on http://localhost:5212 in the Production environment
dotnet publish      # Release output
```

- The project **builds Production only**. The csproj sets Release as the only
  configuration, and its `ProductionOnly` target fails the build for any other
  configuration. Do not add Debug back or pass `-c Debug`.
- Both launch profiles set `ASPNETCORE_ENVIRONMENT=Production`, so the developer
  exception page is not used. There is no `appsettings.Development.json`; do not add one.
- There are no tests yet. If you add them, put them in a sibling `tests/` project and
  keep the calculator logic testable through `DateDurationCalculator.Calculate`.

## Code map

- `Program.cs`: minimal hosting, `AddRazorPages`, static files, and the
  `/api/duration` minimal API endpoint.
- `Services/DateDurationCalculator.cs`: the calculation. Pure static function
  returning a `DateDurationResult` record. Keep it free of ASP.NET dependencies.
- `Services/TimeZones.cs`: IANA zone ids for the search list, the host's local zone,
  and a tolerant `Find` that returns null for unknown ids.
- `Pages/Index.cshtml` and `Index.cshtml.cs`: the form. `OnGet` defaults both dates to
  today; `OnPost` validates and calls the calculator. The time and time zone sections
  are toggled client-side and their open state rides along in the hidden `ShowTime`
  and `ShowZones` fields, so a postback re-renders them as the user left them.
- `Pages/Error.cshtml`, `Pages/Privacy.cshtml`, `Pages/Shared/`: standard template
  pages and layout.
- `wwwroot/lib/`: vendored Bootstrap, jQuery and jQuery Validation. Do not edit.

## Conventions

- `DateOnly` and `TimeOnly` at the boundaries. `DateTime` with `Kind.Unspecified` is
  used only inside the calculator for wall-clock arithmetic.
- Time zone conversion converts the end instant into the start zone, then counts on
  that one calendar. A single zone given for one side applies to both.
- Closed sections send nothing: `OnPost` nulls the times when `ShowTime` is false and
  ignores zones when `ShowZones` is false.
- "Include end date" is implemented by shifting the effective end date forward by one
  day before calculating. Keep that semantic if you touch the calculator.
- When end is before start, the calculator swaps the dates and negates every result
  field (`Sign` is -1). Preserve this so the API stays symmetric.
- Workdays are Monday to Friday with no holiday handling. `Workdays + WeekendDays`
  always equals `TotalDays`.
- Format dates and numbers in the views with `CultureInfo.InvariantCulture`; the UI is
  English and must not follow the server's locale.
- The "Today" links are wired in `wwwroot/js/site.js` via the `js-today` class and a
  `data-target` pointing at the input id.
- Nullable reference types and implicit usings are enabled. Keep warnings at zero.
