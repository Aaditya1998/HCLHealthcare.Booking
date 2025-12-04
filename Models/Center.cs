namespace HCLHealthcare.Booking.Models;

public class Center
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }

    // Phase 4: Ultra-Premium Enhancements
    public string? PhotoUrl { get; set; }
    public string? Timings { get; set; }
}
