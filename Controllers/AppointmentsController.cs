using HCLHealthcare.Booking.Data;
using HCLHealthcare.Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HCLHealthcare.Booking.Services.INotificationService _notificationService;

    public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, HCLHealthcare.Booking.Services.INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    // POST: Appointments/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int slotId, string appointmentType)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Start Transaction for Concurrency Control
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var slot = await _context.TimeSlots
                .Include(t => t.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(t => t.Id == slotId);

            if (slot == null)
            {
                TempData["Error"] = "Slot not found.";
                return RedirectToAction("Index", "Doctors");
            }

            if (!slot.IsAvailable)
            {
                TempData["Error"] = "This slot has just been booked by someone else.";
                return RedirectToAction("Details", "Doctors", new { id = slot.DoctorId });
            }

            // Create Appointment
            var appointment = new Appointment
            {
                DoctorId = slot.DoctorId,
                PatientId = user.Id,
                AppointmentDate = slot.StartTime,
                Status = "Booked",
                AppointmentType = appointmentType // Use selected type
            };

            if (appointment.AppointmentType == "Video")
            {
                appointment.MeetingLink = $"https://meet.jit.si/HCLHealthcare-{Guid.NewGuid()}";
            }

            _context.Appointments.Add(appointment);
            
            // Mark slot as unavailable
            slot.IsAvailable = false;
            _context.TimeSlots.Update(slot);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send Notification
            await _notificationService.SendEmailAsync(user.Email!, "Appointment Confirmed", $"Your appointment with Dr. {slot.Doctor?.User?.FullName} is confirmed for {slot.StartTime}.");

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(MyBookings));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "An error occurred while booking. Please try again.";
            return RedirectToAction("Index", "Doctors");
        }
    }

    // GET: Appointments/MyBookings
    public async Task<IActionResult> MyBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.Center)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.Specialization)
            .Where(a => a.PatientId == user.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    // POST: Appointments/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id);

        if (appointment == null)
        {
            return NotFound();
        }

        if (appointment.AppointmentDate < DateTime.Now)
        {
            TempData["Error"] = "Cannot cancel past appointments.";
            return RedirectToAction(nameof(MyBookings));
        }

        appointment.Status = "Cancelled";
        
        // Free up the slot (Optional logic: find the slot and make it available again)
        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.DoctorId == appointment.DoctorId && t.StartTime == appointment.AppointmentDate);
        
        if (slot != null)
        {
            slot.IsAvailable = true;
            _context.TimeSlots.Update(slot);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(MyBookings));
    }
}
