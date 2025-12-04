using HCLHealthcare.Booking.Data;
using HCLHealthcare.Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Services.SlotService _slotService;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Services.SlotService slotService)
    {
        _context = context;
        _userManager = userManager;
        _slotService = slotService;
    }

    public IActionResult Index()
    {
        return View();
    }

    // --- Manage Doctors ---
    public async Task<IActionResult> ManageDoctors()
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Center)
            .Include(d => d.Specialization)
            .ToListAsync();
        return View(doctors);
    }

    public IActionResult CreateDoctor()
    {
        ViewData["CenterId"] = new SelectList(_context.Centers, "Id", "Name");
        ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name");
        ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email"); 
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDoctor([Bind("UserId,SpecializationId,CenterId,Experience,Qualifications")] Doctor doctor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(doctor);
            await _context.SaveChangesAsync();
            
            var user = await _userManager.FindByIdAsync(doctor.UserId);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                await _userManager.AddToRoleAsync(user, "Doctor");
            }

            return RedirectToAction(nameof(ManageDoctors));
        }
        ViewData["CenterId"] = new SelectList(_context.Centers, "Id", "Name", doctor.CenterId);
        ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
        ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", doctor.UserId);
        return View(doctor);
    }

    public async Task<IActionResult> EditDoctor(int? id)
    {
        if (id == null) return NotFound();

        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        ViewData["CenterId"] = new SelectList(_context.Centers, "Id", "Name", doctor.CenterId);
        ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDoctor(int id, [Bind("Id,UserId,SpecializationId,CenterId,Experience,Qualifications,ConsultationFee,Rating,IsPrime")] Doctor doctor, string fullName)
    {
        if (id != doctor.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(doctor);
                await _context.SaveChangesAsync();

                // Update User's Full Name
                var user = await _userManager.FindByIdAsync(doctor.UserId);
                if (user != null)
                {
                    user.FullName = fullName;
                    await _userManager.UpdateAsync(user);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Doctors.Any(e => e.Id == doctor.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(ManageDoctors));
        }
        ViewData["CenterId"] = new SelectList(_context.Centers, "Id", "Name", doctor.CenterId);
        ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Name", doctor.SpecializationId);
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor != null)
        {
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageDoctors));
    }

    // --- Manage Slots (Admin) ---

    public async Task<IActionResult> ManageDoctorSlots(int id)
    {
        var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor == null) return NotFound();

        var slots = await _context.TimeSlots
            .Where(t => t.DoctorId == doctor.Id && t.StartTime >= DateTime.Today)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        ViewBag.DoctorId = doctor.Id;
        ViewBag.DoctorName = doctor.User?.FullName;
        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSlots(int doctorId, DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime, int interval)
    {
        int count = await _slotService.GenerateSlotsAsync(doctorId, startDate, endDate, startTime, endTime, interval);
        
        TempData["Success"] = $"Successfully generated {count} slots.";
        return RedirectToAction(nameof(ManageDoctorSlots), new { id = doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlot(int id, int doctorId)
    {
        var slot = await _context.TimeSlots.FindAsync(id);
        if (slot != null)
        {
            _context.TimeSlots.Remove(slot);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Slot deleted.";
        }
        return RedirectToAction(nameof(ManageDoctorSlots), new { id = doctorId });
    }

    // --- Manage Centers ---
    public async Task<IActionResult> ManageCenters()
    {
        return View(await _context.Centers.ToListAsync());
    }

    public IActionResult CreateCenter()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCenter([Bind("Id,Name,Location")] Center center)
    {
        if (ModelState.IsValid)
        {
            _context.Add(center);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCenters));
        }
        return View(center);
    }

    public async Task<IActionResult> EditCenter(int? id)
    {
        if (id == null) return NotFound();

        var center = await _context.Centers.FindAsync(id);
        if (center == null) return NotFound();
        return View(center);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCenter(int id, [Bind("Id,Name,Location")] Center center)
    {
        if (id != center.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(center);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Centers.Any(e => e.Id == center.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(ManageCenters));
        }
        return View(center);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCenter(int id)
    {
        var center = await _context.Centers.FindAsync(id);
        if (center != null)
        {
            _context.Centers.Remove(center);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageCenters));
    }
}
