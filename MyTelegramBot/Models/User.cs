using System;

namespace MyTelegramBot.Models;

public class User
{
    public long Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Username { get; set; }
    public List<UserSubscription> UserSubscriptions { get; set; }
}
