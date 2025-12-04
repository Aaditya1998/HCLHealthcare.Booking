using HCLHealthcare.Booking.Data;
using HCLHealthcare.Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorDashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Services.SlotService _slotService;

    public DoctorDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Services.SlotService slotService)
    {
        _context = context;
        _userManager = userManager;
        _slotService = slotService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Find the Doctor entity linked to this user
        var doctor = await _context.Doctors
            .Include(d => d.Center)
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.UserId == user.Id);

        if (doctor == null)
        {
            return View("Error", new ErrorViewModel { RequestId = "Doctor Profile Not Found" });
        }

        // Get today's and future appointments
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= DateTime.Today)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        ViewBag.DoctorName = user.FullName;
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCompleted(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        // Security check: Ensure this appointment belongs to the logged-in doctor
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
        
        if (doctor == null || appointment.DoctorId != doctor.Id)
        {
            return Forbid();
        }

        appointment.Status = "Completed";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Appointment marked as completed.";
        return RedirectToAction(nameof(Index));
    }

    // --- Slot Management ---

    public async Task<IActionResult> ManageSlots()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return NotFound("Doctor profile not found.");

        var slots = await _context.TimeSlots
            .Where(t => t.DoctorId == doctor.Id && t.StartTime >= DateTime.Today)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSlots(DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime, int interval)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
        if (doctor == null) return NotFound();

        int count = await _slotService.GenerateSlotsAsync(doctor.Id, startDate, endDate, startTime, endTime, interval);
        
        TempData["Success"] = $"Successfully generated {count} slots.";
        return RedirectToAction(nameof(ManageSlots));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        var slot = await _context.TimeSlots.FindAsync(id);
        if (slot != null)
        {
            // Security check
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
            
            if (doctor != null && slot.DoctorId == doctor.Id)
            {
                _context.TimeSlots.Remove(slot);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Slot deleted.";
            }
        }
        return RedirectToAction(nameof(ManageSlots));
    }
}
