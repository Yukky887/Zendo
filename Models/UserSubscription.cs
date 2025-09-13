using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace YukkyServiceWeb.Models;

public class UserSubscription
{
    public int Id { get; set; }

    // Связь с пользователем (один-ко-многим)
    public int UserId { get; set; }
    public User User { get; set; }

    // Связь с услугой (один-ко-многим)
    public int ServiceId { get; set; }
    public Service Service { get; set; }

    public bool IsActive { get; set; } = false;
    public DateTime ExpiryDate { get; set; } // До какой даты активна подписка
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}