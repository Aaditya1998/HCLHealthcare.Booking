using HCLHealthcare.Booking.Data;
using HCLHealthcare.Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Controllers;

public class DoctorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DoctorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Doctors
    public async Task<IActionResult> Index(int? centerId, int? specializationId)
    {
        // Populate dropdowns
        ViewData["Centers"] = new SelectList(_context.Centers, "Id", "Name", centerId);
        ViewData["Specializations"] = new SelectList(_context.Specializations, "Id", "Name", specializationId);

        var doctorsQuery = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Center)
            .Include(d => d.Specialization)
            .Include(d => d.TimeSlots.Where(t => t.StartTime >= DateTime.Today && t.IsAvailable).OrderBy(t => t.StartTime)) // Fetch upcoming slots
            .AsQueryable();

        if (centerId.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(d => d.CenterId == centerId);
        }

        if (specializationId.HasValue)
        {
            doctorsQuery = doctorsQuery.Where(d => d.SpecializationId == specializationId);
        }

        var doctors = await doctorsQuery.ToListAsync();
        return View(doctors);
    }

    // GET: Doctors/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Center)
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (doctor == null)
        {
            return NotFound();
        }

        // Get available slots for the next 7 days
        var slots = await _context.TimeSlots
            .Where(t => t.DoctorId == id && t.IsAvailable && t.StartTime > DateTime.Now)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        // "Next Available Slot" Logic
        if (!slots.Any(s => s.StartTime.Date == DateTime.Today))
        {
            var nextSlot = slots.FirstOrDefault();
            if (nextSlot != null)
            {
                ViewBag.NextAvailableMessage = $"No slots available today. Next available: {nextSlot.StartTime:MMM dd, hh:mm tt}";
            }
            else
            {
                ViewBag.NextAvailableMessage = "No upcoming slots available.";
            }
        }

        ViewBag.TimeSlots = slots;

        return View(doctor);
    }
}
