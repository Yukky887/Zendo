namespace YukkyServiceWeb.Models;

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