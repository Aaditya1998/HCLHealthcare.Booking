using HCLHealthcare.Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Controllers;

public class SymptomCheckerController : Controller
{
    private readonly ApplicationDbContext _context;

    public SymptomCheckerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(List<string> symptoms)
    {
        if (symptoms == null || !symptoms.Any())
        {
            ViewBag.Error = "Please select at least one symptom.";
            return View("Index");
        }

        string suggestedSpecialization = "General Physician";
        
        // Simple Rule-based AI Logic
        if (symptoms.Contains("Chest Pain") || symptoms.Contains("Shortness of Breath"))
        {
            suggestedSpecialization = "Cardiology";
        }
        else if (symptoms.Contains("Skin Rash") || symptoms.Contains("Itching"))
        {
            suggestedSpecialization = "Dermatology";
        }
        else if (symptoms.Contains("Fever") && symptoms.Contains("Cough") && symptoms.Contains("Child"))
        {
            suggestedSpecialization = "Pediatrics";
        }

        var specialization = await _context.Specializations
            .FirstOrDefaultAsync(s => s.Name == suggestedSpecialization);

        if (specialization != null)
        {
            return RedirectToAction("Index", "Doctors", new { specializationId = specialization.Id });
        }

        ViewBag.Message = $"Based on your symptoms, we recommend visiting a {suggestedSpecialization}.";
        return View("Result");
    }
}
