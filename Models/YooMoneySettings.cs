namespace YukkyServiceWeb.Models;

public class YooMoneySettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string NotificationUri { get; set; }
    public string WalletNumber { get; set; } 
}