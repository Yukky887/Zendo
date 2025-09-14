using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.DTOs;
using YukkyServiceWeb.Models;
using YukkyServiceWeb.Services;

namespace YukkyServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentController> _logger;
    private readonly YooMoneyService _yooMoneyService;

    public PaymentController(ApplicationDbContext context, ILogger<PaymentController> logger,
        YooMoneyService yooMoneyService)
    {
        _context = context;
        _logger = logger;
        _yooMoneyService = yooMoneyService;
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

    [HttpPost("yoomoney/create")]
    public async Task<IActionResult> CreateYooMoneyPayment([FromBody] CreatePaymentRequestDto request)
    {
        var service = await _context.Services.FindAsync(request.ServiceId);
        
        if (request == null || request.ServiceId == 0)
            return BadRequest("ServiceId is required");

        if (service == null)
            return NotFound("Service not found");

        var userId = 1; // Заглушка, потом заменим на реальный ID из JWT

        // Создаем запись о платеже
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

        // Генерируем URL для оплаты через ЮMoney
        var paymentUrl = _yooMoneyService.CreatePaymentUrl(
            payment.Amount,
            $"Оплата услуги: {service.Name}",
            payment.Id.ToString()
        );

        _logger.LogInformation($"Created YooMoney payment {payment.Id} for service {service.Name}");

        return Ok(new
        {
            PaymentId = payment.Id,
            Amount = payment.Amount,
            Status = payment.Status,
            Service = service.Name,
            PaymentUrl = paymentUrl // Добавляем URL для перенаправления
        });
    }

    [HttpGet("yoomoney/callback")]
    public async Task<IActionResult> YooMoneyCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            // state содержит paymentId
            if (!int.TryParse(state, out int paymentId))
                return BadRequest("Invalid payment id");

            // Получаем токен доступа
            var tokenResponse = await _yooMoneyService.GetAccessTokenAsync(code);

            // Обновляем платеж
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                payment.Status = PaymentStatus.Paid;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Payment authorized successfully",
                Token = tokenResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in YooMoney callback");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("yoomoney/notification")]
    public async Task<IActionResult> YooMoneyNotification([FromBody] YooMoneyNotification notification)
    {
        // Здесь будет обработка вебхуков от ЮMoney о статусе платежей
        _logger.LogInformation("Notification received: {@Notification}", notification);

        return Ok();
    }
}

public class YooMoneyNotification
{
    public string OperationId { get; set; }
    public string NotificationType { get; set; }
    public string DateTime { get; set; }
    public string Sha1Hash { get; set; }
    public string Sender { get; set; }
    public string Codepro { get; set; }
    public string Currency { get; set; }
    public string Amount { get; set; }
    public string Label { get; set; }
}


    
