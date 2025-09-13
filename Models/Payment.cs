using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace YukkyServiceWeb.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? YooMoneyOperationId { get; set; } // ID операции в ЮMoney
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}