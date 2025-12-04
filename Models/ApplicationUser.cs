using Microsoft.AspNetCore.Identity;

namespace HCLHealthcare.Booking.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
