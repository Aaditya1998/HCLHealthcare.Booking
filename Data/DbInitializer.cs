using HCLHealthcare.Booking.Models;
using Microsoft.AspNetCore.Identity;

namespace HCLHealthcare.Booking.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        // Look for any centers.
        if (context.Centers.Any())
        {
            // Even if DB is seeded, ensure Admin exists
            await SeedAdminUser(userManager);
            return;   // DB has been seeded
        }

        // 1. Seed Roles
        string[] roles = { "Admin", "Doctor", "Patient" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Centers
        var centers = new Center[]
        {
            new Center { Name = "HCL Health City", Location = "Noida, Sector 126", PhotoUrl = "https://images.unsplash.com/photo-1586773860418-d37222d8fce3?auto=format&fit=crop&w=800&q=80", Timings = "Mon-Sat: 09:00 AM - 08:00 PM" },
            new Center { Name = "HCL Wellness Hub", Location = "Chennai, OMR", PhotoUrl = "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?auto=format&fit=crop&w=800&q=80", Timings = "Mon-Sun: 10:00 AM - 09:00 PM" },
            new Center { Name = "HCL Care Clinic", Location = "Bangalore, Electronic City", PhotoUrl = "https://images.unsplash.com/photo-1516549655169-df83a092fc43?auto=format&fit=crop&w=800&q=80", Timings = "Mon-Fri: 08:00 AM - 06:00 PM" }
        };
        context.Centers.AddRange(centers);
        await context.SaveChangesAsync();

        // 3. Seed Specializations
        var specializations = new Specialization[]
        {
            new Specialization { Name = "Cardiology" },
            new Specialization { Name = "Dermatology" },
            new Specialization { Name = "General Physician" },
            new Specialization { Name = "Pediatrics" }
        };
        context.Specializations.AddRange(specializations);
        await context.SaveChangesAsync();

        // 4. Seed Doctors (and their User accounts)
        var doctorUser = new ApplicationUser
        {
            UserName = "doctor@hcl.com",
            Email = "doctor@hcl.com",
            FullName = "Dr. Rajesh Koothrappali",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(doctorUser, "Pass@123");
        await userManager.AddToRoleAsync(doctorUser, "Doctor");

        var doctorUser2 = new ApplicationUser
        {
            UserName = "doctor2@hcl.com",
            Email = "doctor2@hcl.com",
            FullName = "Dr. Sheldon Cooper",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(doctorUser2, "Pass@123");
        await userManager.AddToRoleAsync(doctorUser2, "Doctor");

        var doctors = new Doctor[]
        {
            new Doctor
            {
                UserId = doctorUser.Id,
                SpecializationId = specializations[0].Id, // Cardiology
                CenterId = centers[0].Id, // Noida
                Experience = "10 Years",
                Qualifications = "MBBS, MD",
                ConsultationFee = 800,
                Rating = 96,
                PatientStoriesCount = 124,
                IsPrime = true
            },
            new Doctor
            {
                UserId = doctorUser2.Id,
                SpecializationId = specializations[2].Id, // General Physician
                CenterId = centers[1].Id, // Chennai
                Experience = "15 Years",
                Qualifications = "MBBS, PhD",
                ConsultationFee = 500,
                Rating = 88,
                PatientStoriesCount = 45,
                IsPrime = false
            }
        };
        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync();

        // 5. Seed TimeSlots (Next 7 days)
        var timeSlots = new List<TimeSlot>();
        var today = DateTime.Today;
        
        foreach (var doctor in doctors)
        {
            for (int i = 0; i < 7; i++)
            {
                var day = today.AddDays(i);
                // 3 slots per day for simplicity
                timeSlots.Add(new TimeSlot { DoctorId = doctor.Id, StartTime = day.AddHours(9), EndTime = day.AddHours(10), IsAvailable = true });
                timeSlots.Add(new TimeSlot { DoctorId = doctor.Id, StartTime = day.AddHours(11), EndTime = day.AddHours(12), IsAvailable = true });
                timeSlots.Add(new TimeSlot { DoctorId = doctor.Id, StartTime = day.AddHours(14), EndTime = day.AddHours(15), IsAvailable = true });
            }
        }
        context.TimeSlots.AddRange(timeSlots);
        await context.SaveChangesAsync();

        await SeedAdminUser(userManager);
    }

    private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@hcl.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "Pass@123");
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
