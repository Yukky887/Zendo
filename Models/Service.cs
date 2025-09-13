using System.ComponentModel.DataAnnotations;

namespace YukkyServiceWeb.Models;

public class Service
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } // Например, "VPN", "Vaultwarden"
    public string Description { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; } // Цена за период (месяц/год)

    [Required]
    public int BillingPeriodDays { get; set; } // На сколько дней продлевается подписка после оплаты (30, 365 и т.д.)
}