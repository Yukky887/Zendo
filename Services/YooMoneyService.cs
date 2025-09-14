using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Services;

public class YooMoneyService
{
    private readonly YooMoneySettings _settings;
    private readonly HttpClient _httpClient;

    public YooMoneyService(IOptions<YooMoneySettings> settings, HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    // Метод для получения OAuth токена
    public async Task<string> GetAccessTokenAsync(string code)
    {
        var requestData = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _settings.RedirectUri
        };

        var response = await _httpClient.PostAsync("https://yoomoney.ru/oauth/token", 
            new FormUrlEncodedContent(requestData));

        var responseContent = await response.Content.ReadAsStringAsync();
        // Парсим ответ и возвращаем access_token
        return responseContent;
    }

    // Метод для создания платежа
    public string CreatePaymentUrl(decimal amount, string label, string paymentId)
    {
        return $"https://yoomoney.ru/quickpay/confirm.xml?" +
               $"receiver={_settings.WalletNumber}" +
               $"&quickpay-form=button" +
               $"&sum={amount}" +
               $"&label={paymentId}" +
               $"&successURL={_settings.RedirectUri}?success=true" +
               $"&failURL={_settings.RedirectUri}?success=false";
    }
    
    public bool VerifyNotificationSignature(string notificationData, string sha1Hash)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(notificationData + _settings.ClientSecret));
        var computedHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
    
        return computedHash == sha1Hash.ToLower();
    }
    
}

// Добавьте в Models/YooMoneySettings.cs
public class YooMoneySettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string NotificationUri { get; set; }
    public string WalletNumber { get; set; }
}