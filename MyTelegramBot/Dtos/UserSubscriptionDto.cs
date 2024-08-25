using MyTelegramBot.Enums;

namespace MyTelegramBot.Dtos
{
    public class UserSubscriptionDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
        public ServiceDetailDto ServiceDetail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public UserSubscriptionStatus UserSubscriptionStatus { get; set; }
    }
}
