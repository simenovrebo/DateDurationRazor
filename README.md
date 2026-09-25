# DateDurationRazor

A small ASP.NET Core Razor Pages app that calculates the duration between two dates,
in the style of the timeanddate.com date duration calculator. It reports the calendar
difference in years, months and days, plus the total number of days.

## Features

- Web form with start date, end date and an optional "include end date" toggle
  (adds one day to the calculation).
- JSON API at `/api/duration` for programmatic use.
- Handles reversed ranges: when the end date is before the start date, all values are
  returned negative.
- No database, no external services.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run

```bash
dotnet run
```

Then open http://localhost:5212.

The project builds **Production only**: Release is the sole build configuration and
the app starts in the `Production` hosting environment. Passing `-c Debug` fails on
purpose.

## API

```
GET /api/duration?start=2024-01-31&end=2024-03-01&includeEndDate=false
```

Response:

```json
{
  "start": "2024-01-31",
  "end": "2024-03-01",
  "includeEndDate": false,
  "sign": 1,
  "years": 0,
  "months": 1,
  "days": 1,
  "totalDays": 30
}
```

Parameters:

| Name             | Type       | Required | Description                                |
| ---------------- | ---------- | -------- | ------------------------------------------ |
| `start`          | ISO date   | yes      | Start date                                 |
| `end`            | ISO date   | yes      | End date                                   |
| `includeEndDate` | bool       | no       | Count the end date as a full day (default false) |

## How the calculation works

The calendar difference is computed largest unit first: the maximum whole number of
years is added to the start date, then the maximum whole number of months, and the
remainder is the day count. `totalDays` is the plain day difference and is independent
of the calendar breakdown.

## Project layout

```
Program.cs                      App startup, Razor Pages and the /api/duration endpoint
Services/DateDurationCalculator.cs  Pure calculation logic
Pages/Index.cshtml(.cs)         The calculator form
Directory.Build.props           Forces the Release configuration
```

## License

MIT. See [LICENSE](LICENSE).
