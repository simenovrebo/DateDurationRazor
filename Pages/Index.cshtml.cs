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

    [BindProperty]
    public bool IncludeEndDate { get; set; }

    public DateDurationResult? Result { get; private set; }

    public void OnGet()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        Start = today;
        End = today;
    }

    public void OnPost()
    {
        if (!ModelState.IsValid || Start is null || End is null)
            return;

        Result = DateDurationCalculator.Calculate(Start.Value, End.Value, IncludeEndDate);
    }
}
