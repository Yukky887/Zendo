using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); // Очищаем все провайдеры логирования
builder.Logging.AddConsole();     // Добавляем только консольное логирование
builder.Logging.AddDebug();  

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<YooMoneySettings>(
    builder.Configuration.GetSection("YuMoney")); 

builder.Services.AddHttpClient<YooMoneyService>();
builder.Services.AddScoped<YooMoneyService>();

// Настраиваем PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// НАСТРОЙКА CORS - разрешаем все для разработки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  // Разрешаем все origins
                .AllowAnyHeader()   // Разрешаем все headers
                .AllowAnyMethod();  // Разрешаем все методы
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ⚠️ ВАЖНЫЙ ПОРЯДОК MIDDLEWARE:
app.UseCors("AllowAll");  // ДОЛЖНО БЫТЬ ПЕРВЫМ!
app.UseAuthorization();
app.MapControllers();

app.Run();