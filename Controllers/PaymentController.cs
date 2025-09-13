using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.DTOs;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ApplicationDbContext context, ILogger<PaymentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentRequestDto request)
    {
        var service = await _context.Services.FindAsync(request.ServiceId);
        if (service == null)
            return NotFound("Service not found");

        var userId = 1;
        
        var payment = new Payment
        {
            UserId = userId,
            ServiceId = request.ServiceId,
            Amount = service.Price,
            Status = PaymentStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created payment {payment.Id} for service {service.Name}");

        return Ok(new
        {
            payment.Id,
            payment.Amount,
            payment.Status,
            Service = service.Name
        });
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult> ConfirmPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Service)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
            return NotFound("Payment not found");

        if (payment.Status == PaymentStatus.Paid)
            return BadRequest("Payment already confirmed");

        payment.Status = PaymentStatus.Paid;

        var userId = payment.UserId;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(us => us.UserId == userId && us.ServiceId == payment.ServiceId);

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                ServiceId = payment.ServiceId,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddDays(payment.Service.BillingPeriodDays),
                CreatedAt = DateTime.UtcNow
            };
            _context.UserSubscriptions.Add(subscription);
        }
        else
        {
            if (subscription.ExpiryDate < DateTime.UtcNow)
                subscription.ExpiryDate = DateTime.UtcNow.AddDays(payment.Service.BillingPeriodDays);
            else
                subscription.ExpiryDate = subscription.ExpiryDate.AddDays(payment.Service.BillingPeriodDays);
            
            subscription.IsActive = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Payment {payment.Id} confirmed. Subscription updated for user {userId}");

        return Ok(new
        {
            Message = "Payment confirmed successfully",
            Subscription = new
            {
                subscription.Id,
                subscription.IsActive,
                ExpiryDate = subscription.ExpiryDate.ToString("yyyy-MM-dd HH:mm:ss")
            }
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
    {
        var payments = await _context.Payments
            .Include(p => p.Service)
            .ToListAsync();
        return Ok(payments);
    }
}
