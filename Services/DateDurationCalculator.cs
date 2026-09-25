using System;

namespace DateDurationRazor.Services;

public sealed record DateDurationResult(
    DateOnly Start,
    DateOnly End,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? StartZone,      // IANA id, null when no time zone conversion was requested
    string? EndZone,
    bool IncludeEndDate,
    bool CountOnlyWorkdays,
    int Sign,               // +1 if End>=Start, -1 otherwise
    int Years,
    int Months,
    int Days,
    int Hours,
    int Minutes,
    int Seconds,
    int TotalDays,          // whole days in the range
    long TotalSeconds,      // exact length of the range
    int Workdays,           // Monday-Friday days in the range
    int WeekendDays         // Saturday and Sunday days in the range
)
{
    /// <summary>True when the caller supplied a time of day on either side.</summary>
    public bool HasTime => StartTime is not null || EndTime is not null;

    /// <summary>The headline count: workdays when CountOnlyWorkdays is set, otherwise every day.</summary>
    public int CountedDays => CountOnlyWorkdays ? Workdays : TotalDays;

    /// <summary>Seconds represented by CountedDays plus the h:m:s remainder.</summary>
    public long CountedSeconds => (long)CountedDays * 86_400 + (Hours * 3_600L + Minutes * 60L + Seconds);
}

public static class DateDurationCalculator
{
    public static DateDurationResult Calculate(DateOnly start, DateOnly end, bool includeEndDate, bool countOnlyWorkdays = false)
        => Calculate(start, end, null, null, null, null, includeEndDate, countOnlyWorkdays);

    /// <param name="startZone">Zone the start date/time is expressed in. Null means "same zone as the end".</param>
    /// <param name="endZone">Zone the end date/time is expressed in. Null means "same zone as the start".</param>
    public static DateDurationResult Calculate(
        DateOnly start, DateOnly end,
        TimeOnly? startTime, TimeOnly? endTime,
        TimeZoneInfo? startZone, TimeZoneInfo? endZone,
        bool includeEndDate, bool countOnlyWorkdays = false)
    {
        // Both wall-clock values live on the start side's calendar. If zones differ, the end
        // instant is converted into the start zone so that Y/M/D counting uses one calendar.
        var startLocal = start.ToDateTime(startTime ?? TimeOnly.MinValue, DateTimeKind.Unspecified);
        var endLocal = end.ToDateTime(endTime ?? TimeOnly.MinValue, DateTimeKind.Unspecified);

        if (startZone is not null && endZone is not null && !startZone.Equals(endZone))
        {
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, endZone);
            endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, startZone);
        }

        // timeanddate: include end date => add 1 day to the calculation
        // (implemented by shifting the effective end by +1 day)
        if (includeEndDate) endLocal = endLocal.AddDays(1);

        int sign = 1;
        var a = startLocal;
        var b = endLocal;

        if (b < a)
        {
            sign = -1;
            (a, b) = (b, a);
        }

        var span = b - a;
        long totalSeconds = (long)Math.Floor(span.TotalSeconds);
        int totalDays = (int)(totalSeconds / 86_400);

        // Calendar difference: add years, then months, then days, then the h:m:s remainder.
        int years = b.Year - a.Year;
        if (a.AddYears(years) > b) years--;
        years = Math.Max(0, years);
        var afterYears = a.AddYears(years);

        int months = (b.Year - afterYears.Year) * 12 + (b.Month - afterYears.Month);
        if (months < 0) months = 0;
        while (afterYears.AddMonths(months + 1) <= b) months++;
        while (months > 0 && afterYears.AddMonths(months) > b) months--;
        var afterMonths = afterYears.AddMonths(months);

        int days = (int)Math.Floor((b - afterMonths).TotalDays);
        var afterDays = afterMonths.AddDays(days);

        var rest = b - afterDays;
        int hours = rest.Hours;
        int minutes = rest.Minutes;
        int seconds = rest.Seconds;

        // Workdays are counted over the whole days that start at a's time of day.
        int weekendDays = CountWeekendDays(DateOnly.FromDateTime(a), totalDays);
        int workdays = totalDays - weekendDays;

        return new DateDurationResult(
            Start: start,
            End: end,
            StartTime: startTime,
            EndTime: endTime,
            StartZone: startZone?.Id,
            EndZone: endZone?.Id,
            IncludeEndDate: includeEndDate,
            CountOnlyWorkdays: countOnlyWorkdays,
            Sign: sign,
            Years: years * sign,
            Months: months * sign,
            Days: days * sign,
            Hours: hours * sign,
            Minutes: minutes * sign,
            Seconds: seconds * sign,
            TotalDays: totalDays * sign,
            TotalSeconds: totalSeconds * sign,
            Workdays: workdays * sign,
            WeekendDays: weekendDays * sign
        );
    }

    /// <summary>Counts Saturdays and Sundays in the half-open range [from, from + totalDays).</summary>
    private static int CountWeekendDays(DateOnly from, int totalDays)
    {
        int fullWeeks = totalDays / 7;
        int count = fullWeeks * 2;

        var cursor = from.AddDays(fullWeeks * 7);
        for (int i = fullWeeks * 7; i < totalDays; i++, cursor = cursor.AddDays(1))
        {
            if (cursor.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                count++;
        }

        return count;
    }
}
