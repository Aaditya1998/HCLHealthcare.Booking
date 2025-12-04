using System.ComponentModel.DataAnnotations.Schema;

namespace HCLHealthcare.Booking.Models;

public class Appointment
{
    public int Id { get; set; }
    
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public string PatientId { get; set; } = string.Empty;
    [ForeignKey("PatientId")]
    public ApplicationUser? Patient { get; set; }

    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = "Booked"; // Booked, Completed, Cancelled
    public string AppointmentType { get; set; } = "In-Person"; // In-Person, Video
    public string? MeetingLink { get; set; } // For Video Consultations
}
