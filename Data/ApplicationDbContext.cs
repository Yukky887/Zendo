using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services { get; set; } 
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<User> Users { get; set; }
}