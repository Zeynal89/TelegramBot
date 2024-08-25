using System;
using MyTelegramBot.Enums;

namespace MyTelegramBot.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ServiceStatus Status { get; set; }
    public List<ServiceDetail> ServiceDetails { get; set; }
    public DateTime EndDate { get; set; }
    public string BotLink { get; set; }
}
