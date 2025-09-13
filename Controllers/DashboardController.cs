using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.DTOs;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSubscriptionDto>>> GetUserSubscriptions()
    {
        var userSubscriptions = await _context.UserSubscriptions
            .Include(us => us.Service)
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

        return Ok(userSubscriptions);
    }
}