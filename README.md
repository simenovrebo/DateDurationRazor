# DateDurationRazor

A small ASP.NET Core Razor Pages app that calculates the duration between two dates,
in the style of the timeanddate.com date duration calculator. It reports the calendar
difference in years, months and days, plus the total number of days.

## Features

- Web form with start date, end date, "Today" links for both fields, an optional
  "include end date" toggle (adds one day to the calculation), and a "count only
  workdays" toggle (Monday to Friday).
- Optional time fields (hour, minute, second) with "Now", "Start of Day" and "Noon"
  links, opened with "Add time fields".
- Optional time zone conversion, opened with "Add time zone conversion": pick an IANA
  zone for each side and the end instant is converted into the start zone before the
  duration is computed.
- Result shows the calendar duration (down to seconds when times are given), total
  days, workdays, weekend days, and the count converted to seconds, minutes, hours,
  and weeks.
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
GET /api/duration?start=2024-01-31&end=2024-03-01&includeEndDate=false&countOnlyWorkdays=false
GET /api/duration?start=2024-01-01&end=2024-01-01&startTime=12:00&endTime=12:00&startZone=Europe/Oslo&endZone=America/New_York
```

Response:

```json
{
  "start": "2024-01-31",
  "end": "2024-03-01",
  "startTime": null,
  "endTime": null,
  "startZone": null,
  "endZone": null,
  "includeEndDate": false,
  "countOnlyWorkdays": false,
  "sign": 1,
  "years": 0,
  "months": 1,
  "days": 1,
  "hours": 0,
  "minutes": 0,
  "seconds": 0,
  "totalDays": 30,
  "totalSeconds": 2592000,
  "workdays": 22,
  "weekendDays": 8,
  "hasTime": false,
  "countedDays": 30,
  "countedSeconds": 2592000
}
```

Parameters:

| Name             | Type       | Required | Description                                |
| ---------------- | ---------- | -------- | ------------------------------------------ |
| `start`          | ISO date   | yes      | Start date                                 |
| `end`            | ISO date   | yes      | End date                                   |
| `includeEndDate` | bool       | no       | Count the end date as a full day (default false) |
| `countOnlyWorkdays` | bool    | no       | Make `countedDays` the Monday to Friday count (default false) |
| `startTime`      | HH:mm[:ss] | no       | Time of day on the start date (default midnight) |
| `endTime`        | HH:mm[:ss] | no       | Time of day on the end date (default midnight) |
| `startZone`      | IANA id    | no       | Zone of the start date/time, e.g. `Europe/Oslo` |
| `endZone`        | IANA id    | no       | Zone of the end date/time. One zone given applies to both sides |

Unknown zone ids return 400. `GET /api/timezones` lists the accepted ids.

## How the calculation works

The calendar difference is computed largest unit first: the maximum whole number of
years is added to the start date, then the maximum whole number of months, and the
remainder is the day count. `totalDays` is the plain day difference and is independent
of the calendar breakdown. `workdays` and `weekendDays` split `totalDays` by weekday,
and `countedDays` is whichever of `totalDays` or `workdays` the caller asked for.

With times, the range is measured to the second and the calendar breakdown continues
into hours, minutes and seconds. With two different zones, the end wall-clock time is
converted into the start zone first, so the whole calculation runs on one calendar.

## Project layout

```
Program.cs                      App startup, Razor Pages and the /api/duration endpoint
Services/DateDurationCalculator.cs  Pure calculation logic
Services/TimeZones.cs           IANA zone list, local zone, and id lookup
Pages/Index.cshtml(.cs)         The calculator form
```

## License

MIT. See [LICENSE](LICENSE).
