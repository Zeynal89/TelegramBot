using MyTelegramBot.Enums;

namespace MyTelegramBot.Models;

public class ServiceDetail
{
    public int Id { get; set; }
    public Duration Duration { get; set; }
    public double Cost { get; set; }
    public int ServiceId { get; set; }
    public List<UserSubscription> UserSubscriptions { get; set; }
}
