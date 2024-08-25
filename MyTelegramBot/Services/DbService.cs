using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Data;
using MyTelegramBot.Dtos;
using MyTelegramBot.Enums;
using MyTelegramBot.Models;
using System.ComponentModel;
using System.Reflection;

namespace MyTelegramBot.Services;

public class DbService
{
    public async Task SeedData()
    {
        using var context = new AppDbContext();

        context.Database.EnsureDeleted();
        // Ensure database is created
        context.Database.EnsureCreated();

        var services = CreateServices();

        await context.Services.AddRangeAsync(services);

        // Add a new entity
        // var entity = new YourEntity { Name = "Sample Entity" };
        // context.YourEntities.Add(entity);
        await context.SaveChangesAsync();

        await context.ServiceDetails.AddRangeAsync(CreateServiceDetails());
        await context.SaveChangesAsync();

        // Query and display entities
        // var entities = context.YourEntities.ToList();
        // foreach (var e in entities)
        // {
        //     Console.WriteLine($"Id: {e.Id}, Name: {e.Name}");
        // }
    }

    public List<Service> GetAllServices()
    {
        using var context = new AppDbContext();
        return context.Services.ToList();
    }

    public List<ServiceDetail> GetAllServiceDetails()
    {
        using var context = new AppDbContext();
        return context.ServiceDetails.ToList();
    }

    public async Task<ServiceDetail> GetServiceDetailAsync(int serviceDetailId)
    {
        using var context = new AppDbContext();
        return await context.ServiceDetails.FirstOrDefaultAsync(x => x.Id == serviceDetailId);
    }

    // public async Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(long userId)
    // {
    //     using var context = new AppDbContext();
    //     var user = await context.Users.FirstOrDefaultAsync(p=>p.Id == userId);  


    // }



    private List<Service> CreateServices()
    {
        return new List<Service>(){
            new Service {
                Id = 1,
                Name = "c# tutorial",
                Description = "Teaching c# for beginners",
                Status = ServiceStatus.Active,
                EndDate = DateTime.Now.AddYears(2),
                BotLink = "https://t.me/ExampleBot1"
            },
            new Service {
                Id = 2,
                Name = "c# advanced",
                Description = "Teaching c# for developers",
                Status = ServiceStatus.Active,
                EndDate = DateTime.Now.AddYears(2),
                BotLink = "https://t.me/ExampleBot2"
            },
            new Service {
                Id = 3,
                Name = "c# from zero to hero",
                Description = "Teaching c# for beginners and masters",
                Status = ServiceStatus.Active,
                EndDate = DateTime.Now.AddYears(2),
                BotLink = "https://t.me/ExampleBot3"
            }
        };
    }

    private List<ServiceDetail> CreateServiceDetails()
    {
        return new List<ServiceDetail>(){
            new ServiceDetail {
                Id = 1,
                Duration = Duration.Week,
                Cost = 100.56,
                ServiceId = 1,
            },
            new ServiceDetail {
                Id = 2,
                Duration = Duration.TwoWeek,
                Cost = 150,
                ServiceId = 1,
            },
            new ServiceDetail {
                Id = 3,
                Duration = Duration.Month,
                Cost = 200,
                ServiceId = 1,
            },
             new ServiceDetail {
                Id = 4,
                Duration = Duration.ThreeMonth,
                Cost = 250,
                ServiceId = 1,
            },
            new ServiceDetail {
                Id = 5,
                Duration = Duration.SixMonth,
                Cost = 300,
                ServiceId = 1,
            },
            new ServiceDetail {
                Id = 6,
                Duration = Duration.Year,
                Cost = 350,
                ServiceId = 1,
            },

            new ServiceDetail {
                Id = 7,
                Duration = Duration.Week,
                Cost = 10,
                ServiceId = 2,
            },
            new ServiceDetail {
                Id = 8,
                Duration = Duration.TwoWeek,
                Cost = 20,
                ServiceId = 2,
            },
            new ServiceDetail {
                Id = 9,
                Duration = Duration.Month,
                Cost = 30,
                ServiceId = 2,
            },
             new ServiceDetail {
                Id = 10,
                Duration = Duration.ThreeMonth,
                Cost = 40,
                ServiceId = 2,
            },
            new ServiceDetail {
                Id = 11,
                Duration = Duration.SixMonth,
                Cost = 50,
                ServiceId = 2,
            },
            new ServiceDetail {
                Id = 12,
                Duration = Duration.Year,
                Cost = 60,
                ServiceId = 2,
            },

            new ServiceDetail {
                Id = 13,
                Duration = Duration.Week,
                Cost = 70,
                ServiceId = 3,
            },
            new ServiceDetail {
                Id = 14,
                Duration = Duration.TwoWeek,
                Cost = 80,
                ServiceId = 3,
            },
            new ServiceDetail {
                Id = 15,
                Duration = Duration.Month,
                Cost = 30,
                ServiceId = 3,
            },
             new ServiceDetail {
                Id = 16,
                Duration = Duration.ThreeMonth,
                Cost = 40,
                ServiceId = 3,
            },
            new ServiceDetail {
                Id = 17,
                Duration = Duration.SixMonth,
                Cost = 50,
                ServiceId = 3,
            },
            new ServiceDetail {
                Id = 18,
                Duration = Duration.Year,
                Cost = 60,
                ServiceId = 3,
            },
        };
    }

    public async Task<User> CreateUser(long id, string firstName, string lastName, string username)
    {
        using var context = new AppDbContext();

        var user = new User
        {
            Id = id,
            Firstname = firstName,
            Lastname = lastName,
            Username = username,
        };
        context.Users.Add(user);
        context.SaveChanges(true);

        return user;
    }

    public async Task<UserSubscriptionDto> CreateUserSubscription(long userId, int serviceDetailId)
    {
        using var context = new AppDbContext();

        var serviceDetail = await context.ServiceDetails.FirstOrDefaultAsync(p => p.Id == serviceDetailId);


        var userSubscription = new UserSubscription
        {
            UserId = userId,
            ServiceDetailId = serviceDetailId,
            StartDate = DateTime.Now,
            EndDate = serviceDetail.Duration.GetEndTime(),
            UserSubscriptionStatus = UserSubscriptionStatus.Active,
        };

        context.UserSubscriptions.Add(userSubscription);
        context.SaveChanges(true);

        context.Dispose();

        var userSubscriptionDto = await GetUserSubscriptionDtoAsync(userSubscription);

        return userSubscriptionDto;
    }

    public async Task UpdateUserSubscription(int oldServiceDetailId, int newServiceDetailId, long userId)
    {
        using var context = new AppDbContext();
        var userSubscription = await context.UserSubscriptions.FirstOrDefaultAsync(y => y.ServiceDetailId == oldServiceDetailId && y.UserId == userId && y.EndDate >= DateTime.Now);

        var newServiceDetail = await context.ServiceDetails.FirstOrDefaultAsync(p => p.Id == newServiceDetailId);

        userSubscription.UpdateSubscription(newServiceDetail.Id, newServiceDetail.Duration);

        context.UserSubscriptions.Update(userSubscription);

        await context.SaveChangesAsync();
    }

    public async Task ChangeUserSubscriptionStatusAsync(int serviceDetailId, long userId)
    {
        using var context = new AppDbContext();
        var userSubscription = await context.UserSubscriptions.FirstOrDefaultAsync(y => y.ServiceDetailId == serviceDetailId && y.UserId == userId && y.EndDate >= DateTime.Now);
        userSubscription.StopSubscription();
        await context.SaveChangesAsync();
    }

    public async Task<UserSubscriptionDto> GetUserSubscriptionDtoAsync(int serviceId, long userId)
    {
        using var context = new AppDbContext();
        var serviceDetail = await context.ServiceDetails.FirstOrDefaultAsync(p => p.ServiceId == serviceId);
        var userSubscription = await context.UserSubscriptions.FirstOrDefaultAsync(p => p.ServiceDetailId == serviceDetail.Id && p.UserId == userId && p.EndDate >= DateTime.Now);

        if (userSubscription is null)
        {
            return null;
        }

        return await GetUserSubscriptionDtoAsync(userSubscription);
    }

    public async Task<UserSubscriptionDto> GetUserSubscriptionDtoAsync(UserSubscription userSubscription)
    {
        using var context = new AppDbContext();
        var user = await context.Users.FindAsync(userSubscription.UserId);
        var userDto = new UserDto
        {
            Id = user.Id,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Username = user.Username
        };

        var serviceDetail = await context.ServiceDetails.FindAsync(userSubscription.ServiceDetailId);
        var service = await context.Services.FindAsync(serviceDetail.ServiceId);
        var serviceDto = new ServiceDto
        {
            Id = service.Id,
            BotLink = service.BotLink,
            Description = service.Description,
            EndDate = service.EndDate,
            Name = service.Name,
            Status = service.Status,
        };

        var serviceDetailDto = new ServiceDetailDto
        {
            Id = serviceDetail.Id,
            Cost = serviceDetail.Cost,
            Duration = ((DescriptionAttribute)serviceDetail.Duration.GetType().GetField(serviceDetail.Duration.ToString()).GetCustomAttribute(typeof(DescriptionAttribute)))?.Description,
            ServiceDto = serviceDto
        };


        var userSubscriptionDto = new UserSubscriptionDto
        {
            Id = userSubscription.Id,
            StartDate = userSubscription.StartDate,
            EndDate = userSubscription.EndDate,
            UserSubscriptionStatus = userSubscription.UserSubscriptionStatus,
            User = userDto,
            ServiceDetail = serviceDetailDto
        };
        return userSubscriptionDto;
    }
}
