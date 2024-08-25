using MyTelegramBot.Data;
using MyTelegramBot.Dtos;
using MyTelegramBot.Enums;
using MyTelegramBot.Services;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var dbService = new DbService();

await dbService.SeedData();

var services = dbService.GetAllServices();

var serviceDetails = dbService.GetAllServiceDetails();

var userStates = new ConcurrentDictionary<long, string>(); // Используем ConcurrentDictionary для потокобезопасности


var token = Environment.GetEnvironmentVariable("TOKEN") ?? "7375868535:AAG9zVMaSxfwsqMuAw16B1IDhlQeMTYKWt0";

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMeAsync();
await bot.DropPendingUpdatesAsync();
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
cts.Cancel(); // stop the bot

async Task OnMessage(Message msg, UpdateType type)
{
    await ShowMainMenu(msg.Chat);
}

async Task OnUpdate(Update update)
{
    await OnCallbackQuery(update.CallbackQuery);
}

// Обработчик нажатий на кнопки
async Task OnCallbackQuery(CallbackQuery callbackQuery)
{
    var userId = callbackQuery.From.Id;

    var callbackData = callbackQuery.Data;

    var callbackQueryFrom = callbackQuery.From;

    if (callbackQuery.Data == "Личный кабинет")
    {
        await ShowPersonalAccountMenu(callbackQuery);
    }


    if (callbackQuery.Data == "Назад")
    {
        if (userStates.TryGetValue(userId, out var previousState))
        {
            // Переход к предыдущему состоянию
            userStates.TryRemove(userId, out _); // Удаляем текущее состояние
            await HandlePreviousState(callbackQuery, previousState);
        }
        else
        {
            // Если предыдущее состояние не найдено, возвращаем в главное меню
            await ShowMainMenu(callbackQuery.Message!.Chat);
        }
    }
    else if (callbackData.Contains('_'))
    {
        var parts = callbackData.Split('_');

        var action = parts[0]; // "resume", "stop", or "change"
        var serviceDetailId = int.Parse(parts[1]); // The service ID

        int oldServiceDetailId = 0;

        if (parts.Length > 2)
        {
            oldServiceDetailId = int.Parse(parts[2]);
        }

        var responseMessage = string.Empty;

        var serviceDetail = await dbService.GetServiceDetailAsync(serviceDetailId);
        if (action.Equals("resume"))
        {
            await dbService.ChangeUserSubscriptionStatusAsync(serviceDetailId, userId);
            await ShowServiceDetails(callbackQuery, serviceDetail.ServiceId);

            responseMessage = "Подписка возобновлена.";
        }
        else if (action.Equals("stop"))
        {
            await dbService.ChangeUserSubscriptionStatusAsync(serviceDetailId, userId);

            responseMessage = "Подписка остановлена.";
        }
        else if (action.Equals("change"))
        {
            await ShowServiceDetails(callbackQuery, serviceDetail.ServiceId, true, serviceDetail.Id);

            responseMessage = "Изменение подписки.";
            // Logic to change subscription
        }
        else if (action.Equals("isChanged"))
        {
            await dbService.UpdateUserSubscription(oldServiceDetailId, serviceDetailId, userId);
            responseMessage = "Подписка изменена.";
        }

        await bot.AnswerCallbackQueryAsync(callbackQuery.Id, responseMessage);
    }
    else if (int.TryParse(callbackQuery.Data, out int serviceId))
    {
        using var context = new AppDbContext();


        var user = context.Users.FirstOrDefault(p => p.Username == callbackQueryFrom.Username);

        if (user is null)
        {
            user = await dbService.CreateUser(callbackQueryFrom.Id, callbackQueryFrom.FirstName, callbackQueryFrom.LastName, callbackQueryFrom.Username);
        }

        if (callbackQuery.Message.Text == "Сервисы:")
        {
            var userSubscriptionDto = await dbService.GetUserSubscriptionDtoAsync(serviceId, userId);

            if (userSubscriptionDto is not null)
            {
                await ShowServiceSubscriptionManagement(callbackQuery, userSubscriptionDto);
            }
            else
            {
                await ShowServiceDetails(callbackQuery, serviceId);
            }
            //userStates[userId] = "ServiceDetails"; // Сохраняем состояние текущего меню
        }
        else
        {
            var userSubscription = context.UserSubscriptions.FirstOrDefault(p => p.ServiceDetailId == serviceId);

            UserSubscriptionDto userSubscriptionDto;

            if (userSubscription is null)
            {
                userSubscriptionDto = await dbService.CreateUserSubscription(user.Id, serviceId);
                await bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"Ваш платеж прошел успешно.");
            }
            else
            {
                userSubscriptionDto = await dbService.GetUserSubscriptionDtoAsync(userSubscription);
            }
            await ShowServiceSubscriptionManagement(callbackQuery, userSubscriptionDto);
        }
    }
    else
    {
        await ServicesCallback(callbackQuery);
        userStates[userId] = "ServicesMenu"; // Сохраняем состояние текущего меню
    }
}

async Task HandlePreviousState(CallbackQuery callbackQuery, string previousState)
{
    switch (previousState)
    {
        case "ServicesMenu":
            await ServicesCallback(callbackQuery);
            break;
        case "ServiceDetails":
            // Используем безопасное извлечение данных
            var callbackData = callbackQuery.Message?.ReplyMarkup?.InlineKeyboard
                .SelectMany(row => row)
                .FirstOrDefault(button => button.CallbackData == callbackQuery.Data)?.CallbackData;

            if (int.TryParse(callbackData, out int serviceId))
            {
                await ShowServiceDetails(callbackQuery, serviceId);
            }
            else
            {
                // Обработка ошибки, если ID не удалось получить
                await ShowMainMenu(callbackQuery.Message!.Chat);
            }
            break;
        default:
            await ShowMainMenu(callbackQuery.Message!.Chat);
            break;
    }
}

async Task ShowMainMenu(Chat chat)
{
    var mainMenuMarkup = new InlineKeyboardMarkup()
            .AddNewRow()
                .AddButton("Сервисы")
                .AddButton("Личный кабинет");
    await bot.SendTextMessageAsync(chat, "Главное меню:", replyMarkup: mainMenuMarkup);
}

async Task ServicesCallback(CallbackQuery callbackQuery)
{
    var mainMenuMarkup = new InlineKeyboardMarkup();

    foreach (var item in services)
    {
        mainMenuMarkup.AddNewRow();
        mainMenuMarkup.AddButton(item.Name, item.Id.ToString());
    }

    mainMenuMarkup.AddNewRow();
    mainMenuMarkup.AddButton("Назад");

    await bot.SendTextMessageAsync(callbackQuery.Message!.Chat, $"{callbackQuery.Data}:", replyMarkup: mainMenuMarkup);
}

async Task ShowServiceDetails(CallbackQuery callbackQuery, int serviceId, bool isChanged = false, int oldServiceDetailId = default)
{
    using var context = new AppDbContext();

    var service = context.Services.FirstOrDefault(p => p.Id == serviceId);
    var servicesDetail = context.ServiceDetails.Where(sd => sd.ServiceId == serviceId).ToList();

    var mainMenuMarkup = new InlineKeyboardMarkup();

    var detailMessage = $"Названия: {service.Name}\n" +
                        $"Описания: {service.Description}";

    foreach (var serviceDetail in servicesDetail.Where(p => p.Id != oldServiceDetailId))
    {
        string duration = ((DescriptionAttribute)serviceDetail.Duration.GetType().GetField(serviceDetail.Duration.ToString()).GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
        var detailButtons = $"Период: {duration}, Цена: ${serviceDetail.Cost:00}";

        var buttonCallbackData = isChanged ? $"isChanged_{serviceDetail.Id}_{oldServiceDetailId}" : serviceDetail.Id.ToString();

        mainMenuMarkup.AddNewRow();
        mainMenuMarkup.AddButton(detailButtons, buttonCallbackData);
    }
    mainMenuMarkup.AddNewRow();
    mainMenuMarkup.AddButton("Назад");


    await bot.SendTextMessageAsync(callbackQuery.Message!.Chat, detailMessage, replyMarkup: mainMenuMarkup);
}

async Task ShowServiceSubscriptionManagement(CallbackQuery callbackQuery, UserSubscriptionDto userSubscription)
{
    var mainMenuMarkup = new InlineKeyboardMarkup();

    var serviceDetailId = userSubscription.ServiceDetail.Id.ToString();

    var detailMessage = $"Названия: {userSubscription.ServiceDetail.ServiceDto.Name}\n" +
                        $"Описания: {userSubscription.ServiceDetail.ServiceDto.Description}\n" +
                        $"Ссылка на Telegram бота: {userSubscription.ServiceDetail.ServiceDto.BotLink}\n" +
                        $"Период подписки: {userSubscription.ServiceDetail.Duration}\n" +
                        $"Цена: ${userSubscription.ServiceDetail.Cost:00}\n" +
                        $"Текущий статус: {userSubscription.UserSubscriptionStatus}\n" +
                        $"Дата начала подписки: {userSubscription.StartDate}\n" +
                        $"Дата следующей оплаты подписки {userSubscription.EndDate}";

    if (userSubscription.UserSubscriptionStatus == UserSubscriptionStatus.Stopped)
    {
        mainMenuMarkup.AddNewRow();
        mainMenuMarkup.AddButton("Возобновить", $"resume_{serviceDetailId}");
    }
    else
    {
        mainMenuMarkup.AddNewRow();
        mainMenuMarkup.AddButton("Остановить", $"stop_{serviceDetailId}");
        mainMenuMarkup.AddNewRow();
        mainMenuMarkup.AddButton("Изменить", $"change_{serviceDetailId}");
    }
    await bot.SendTextMessageAsync(callbackQuery.Message!.Chat, detailMessage, replyMarkup: mainMenuMarkup);
}

async Task ShowPersonalAccountMenu(CallbackQuery callbackQuery)
{

}