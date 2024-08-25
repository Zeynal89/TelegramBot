using MyTelegramBot.Enums;

namespace MyTelegramBot.Dtos
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime EndDate { get; set; }
        public string BotLink { get; set; }
    }
}
