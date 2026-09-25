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

- The project **builds Production only**. `Directory.Build.props` sets Release as the
  default and only configuration, and a `ProductionOnly` target in the csproj fails
  the build for any other configuration. Do not add Debug back or pass `-c Debug`.
- Both launch profiles set `ASPNETCORE_ENVIRONMENT=Production`, so the developer
  exception page and `appsettings.Development.json` are not used.
- There are no tests yet. If you add them, put them in a sibling `tests/` project and
  keep the calculator logic testable through `DateDurationCalculator.Calculate`.

## Code map

- `Program.cs`: minimal hosting, `AddRazorPages`, static files, and the
  `/api/duration` minimal API endpoint.
- `Services/DateDurationCalculator.cs`: the only business logic. Pure static function
  returning a `DateDurationResult` record. Keep it free of ASP.NET dependencies.
- `Pages/Index.cshtml` and `Index.cshtml.cs`: the form. `OnGet` defaults both dates to
  today; `OnPost` validates and calls the calculator.
- `Pages/Error.cshtml`, `Pages/Privacy.cshtml`, `Pages/Shared/`: standard template
  pages and layout.
- `wwwroot/lib/`: vendored Bootstrap, jQuery and jQuery Validation. Do not edit.

## Conventions

- `DateOnly` everywhere for dates. No `DateTime` in the calculation path.
- "Include end date" is implemented by shifting the effective end date forward by one
  day before calculating. Keep that semantic if you touch the calculator.
- When end is before start, the calculator swaps the dates and negates every result
  field (`Sign` is -1). Preserve this so the API stays symmetric.
- Nullable reference types and implicit usings are enabled. Keep warnings at zero.
