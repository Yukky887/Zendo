using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.DTOs;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSubscriptionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserSubscriptionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSubscriptionDto>>> GetSubscriptions()
    {
        var userId = 1;

        var subscriptions = await _context.UserSubscriptions
            .Include(us => us.Service)
            .Where(us => us.UserId == userId)
            .Select(us => new UserSubscriptionDto 
            {
                Id = us.Id,
                ServiceName = us.Service.Name,
                ServiceDescription = us.Service.Description,
                IsActive = us.IsActive,
                ExpiryDate = us.ExpiryDate,
                CreatedAt = us.CreatedAt,
                DaysRemaining = (int)(us.ExpiryDate - DateTime.UtcNow).TotalDays
            })
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserSubscriptionDto>>> GetActiveSubscriptions()
    {
        var userId = 1;

        var activeSubscriptions = await _context.UserSubscriptions
            .Include(us => us.Service)
            .Where(us => us.UserId == userId && us.IsActive && us.ExpiryDate > DateTime.UtcNow)
            .Select(us => new UserSubscriptionDto
            {
                Id = us.Id,
                ServiceName = us.Service.Name,
                ServiceDescription = us.Service.Description,
                IsActive = us.IsActive,
                ExpiryDate = us.ExpiryDate,
                CreatedAt = us.CreatedAt,
                DaysRemaining = (int)(us.ExpiryDate - DateTime.UtcNow).TotalDays
            })
            .ToListAsync();

        return Ok(activeSubscriptions);
    }
}