using MyTelegramBot.Enums;

namespace MyTelegramBot.Models;

public class UserSubscription
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int ServiceDetailId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public UserSubscriptionStatus UserSubscriptionStatus { get; set; }

    public void UpdateSubscription(int serviceDetailId, Duration duration)
    {
        ServiceDetailId = serviceDetailId;
        EndDate = duration.GetEndTime();
    }

    public void StopSubscription()
    {
        UserSubscriptionStatus = UserSubscriptionStatus == UserSubscriptionStatus.Active ? UserSubscriptionStatus.Stopped : UserSubscriptionStatus.Active;
    }

   
}

public static class DurationExtension
{
    public static DateTime GetEndTime(this Duration duration)
    {
        switch (duration)
        {
            case Duration.Week:
                return DateTime.Now.AddDays(7);
            case Duration.TwoWeek:
                return DateTime.Now.AddDays(14);
            case Duration.Month:
                return DateTime.Now.AddMonths(1);
            case Duration.ThreeMonth:
                return DateTime.Now.AddMonths(3);
            case Duration.SixMonth:
                return DateTime.Now.AddMonths(6);
            default:
                return DateTime.Now.AddYears(1);
        }
    }
}
