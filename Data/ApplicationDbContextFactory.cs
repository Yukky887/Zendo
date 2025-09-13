using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace YukkyServiceWeb.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Настройка конфигурации для чтения appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Убедитесь, что это путь к проекту Web API
                .AddJsonFile("appsettings.json")
                .Build();

            // Настройка построителя опций для DbContext
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            optionsBuilder.UseNpgsql(connectionString); // Используем PostgreSQL

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}