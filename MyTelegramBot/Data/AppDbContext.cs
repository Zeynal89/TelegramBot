using System;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Models;

namespace MyTelegramBot.Data;

public class AppDbContext : DbContext
{
    public DbSet<Service> Services { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ServiceDetail> ServiceDetails { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db");
    }
}
