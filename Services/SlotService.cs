using HCLHealthcare.Booking.Data;
using HCLHealthcare.Booking.Models;
using Microsoft.EntityFrameworkCore;

namespace HCLHealthcare.Booking.Services;

public class SlotService
{
    private readonly ApplicationDbContext _context;

    public SlotService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GenerateSlotsAsync(int doctorId, DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime, int intervalMinutes)
    {
        var slotsToAdd = new List<TimeSlot>();
        var currentDay = startDate.Date;

        while (currentDay <= endDate.Date)
        {
            var currentSlotStart = currentDay.Add(startTime);
            var dayEndTime = currentDay.Add(endTime);

            while (currentSlotStart.AddMinutes(intervalMinutes) <= dayEndTime)
            {
                var slotEnd = currentSlotStart.AddMinutes(intervalMinutes);

                // Check for overlap with existing slots
                bool isOverlapping = await _context.TimeSlots.AnyAsync(s => 
                    s.DoctorId == doctorId &&
                    ((s.StartTime < slotEnd && s.EndTime > currentSlotStart)));

                if (!isOverlapping)
                {
                    slotsToAdd.Add(new TimeSlot
                    {
                        DoctorId = doctorId,
                        StartTime = currentSlotStart,
                        EndTime = slotEnd,
                        IsAvailable = true
                    });
                }

                currentSlotStart = currentSlotStart.AddMinutes(intervalMinutes);
            }

            currentDay = currentDay.AddDays(1);
        }

        if (slotsToAdd.Any())
        {
            _context.TimeSlots.AddRange(slotsToAdd);
            await _context.SaveChangesAsync();
        }

        return slotsToAdd.Count;
    }
}
