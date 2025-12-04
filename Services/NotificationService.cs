namespace HCLHealthcare.Booking.Services;

public interface INotificationService
{
    Task SendEmailAsync(string toEmail, string subject, string message);
    Task SendSmsAsync(string phoneNumber, string message);
}

public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(ILogger<MockNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string message)
    {
        _logger.LogInformation($"[Mock Email] To: {toEmail}, Subject: {subject}, Body: {message}");
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation($"[Mock SMS] To: {phoneNumber}, Message: {message}");
        return Task.CompletedTask;
    }
}
