namespace MyTelegramBot.Dtos
{
    public class ServiceDetailDto
    {
        public int Id { get; set; }
        public string Duration { get; set; }
        public double Cost { get; set; }
        public ServiceDto ServiceDto { get; set; }
    }
}
