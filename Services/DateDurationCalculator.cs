using System;

namespace DateDurationRazor.Services;

public sealed record DateDurationResult(
    DateOnly Start,
    DateOnly End,
    bool IncludeEndDate,
    int Sign,               // +1 if End>=Start, -1 otherwise
    int Years,
    int Months,
    int Days,
    int TotalDays
);

public static class DateDurationCalculator
{
    public static DateDurationResult Calculate(DateOnly start, DateOnly end, bool includeEndDate)
    {
        // timeanddate: include end date => add 1 day to the calculation
        // (implemented by shifting the effective end by +1 day)
        var effectiveEnd = includeEndDate ? end.AddDays(1) : end;

        int sign = 1;
        var a = start;
        var b = effectiveEnd;

        if (b.CompareTo(a) < 0)
        {
            sign = -1;
            (a, b) = (b, a);
        }

        // Total days (exclusive of end unless includeEndDate was applied above)
        int totalDays = b.DayNumber - a.DayNumber;

        // Calendar Y/M/D difference: add years, then months, then days (largest to smallest).
        int years = 0;
        {
            int guess = b.Year - a.Year;
            var t = a.AddYears(guess);
            if (t.CompareTo(b) > 0) guess--;
            years = Math.Max(0, guess);
        }

        var afterYears = a.AddYears(years);

        int months = 0;
        {
            int guess = (b.Year - afterYears.Year) * 12 + (b.Month - afterYears.Month);
            if (guess < 0) guess = 0;

            var t = afterYears.AddMonths(guess);
            if (t.CompareTo(b) > 0) guess--;

            // Ensure maximal months such that afterYears+months <= b
            while (guess < 0) guess = 0;
            while (afterYears.AddMonths(guess + 1).CompareTo(b) <= 0) guess++;
            while (afterYears.AddMonths(guess).CompareTo(b) > 0) guess--;

            months = Math.Max(0, guess);
        }

        var afterMonths = afterYears.AddMonths(months);

        int days = b.DayNumber - afterMonths.DayNumber;

        return new DateDurationResult(
            Start: start,
            End: end,
            IncludeEndDate: includeEndDate,
            Sign: sign,
            Years: years * sign,
            Months: months * sign,
            Days: days * sign,
            TotalDays: totalDays * sign
        );
    }
}
