using System.ComponentModel.DataAnnotations.Schema;

namespace HCLHealthcare.Booking.Models;

public class Doctor
{
    public int Id { get; set; }
    
    // Link to Identity User
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public int SpecializationId { get; set; }
    public Specialization? Specialization { get; set; }

    public int CenterId { get; set; }
    public Center? Center { get; set; }

    public string Experience { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;

    // Phase 4: Ultra-Premium Enhancements
    public decimal ConsultationFee { get; set; }
    public int Rating { get; set; } // 0-100
    public int PatientStoriesCount { get; set; }
    public bool IsPrime { get; set; }

    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
