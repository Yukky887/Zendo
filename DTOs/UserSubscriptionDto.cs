namespace YukkyServiceWeb.DTOs;

public class UserSubscriptionDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysRemaining { get; set; }
}