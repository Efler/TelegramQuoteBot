using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramQuoteBotProject;


public static class TgBot
{
    
    private static models.TelegramQuoteDbContext _context;
    
    private static ReplyKeyboardMarkup _subscribe = new ReplyKeyboardMarkup
    (
        new[] { new[] { new KeyboardButton("Получать цитатки") } }
    );
    
    private static ReplyKeyboardMarkup _unsub = new ReplyKeyboardMarkup
    (
        new[] { new[] { new KeyboardButton("Отписаться") } }
    );

    public static void TgBotStart(models.TelegramQuoteDbContext context, string telegramToken)
    {
        _subscribe.ResizeKeyboard = true;
        _unsub.ResizeKeyboard = true;
        _context = context;

        var client = new TelegramBotClient(telegramToken);

        ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        var scheduler = schedulerFactory.GetScheduler().Result;
        scheduler.Start();
        
        var jobDataMap = new JobDataMap();
        jobDataMap.Put("context", _context);
        jobDataMap.Put("client", client);
        
        var jobDetail = JobBuilder.Create<SendQuoteJob>().SetJobData(jobDataMap).Build();
        
        var trigger = TriggerBuilder.Create()
            .WithIdentity("NineAM", "Time")
            .WithSchedule(CronScheduleBuilder.CronSchedule("0 * * ? * * *"))  //TODO: CHANGE AFTER TESTING!
            .Build();
        
        scheduler.ScheduleJob(jobDetail, trigger);
        
        client.StartReceiving(Update, Error);
        Console.ReadLine();
    }

    private static async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            if (message is { Text: not null })
            {
                switch (message.Text)
                {
                    case "/start":
                        var keyboard = await _context.Users.FindAsync(message.Chat.Id) == null ? _subscribe : _unsub;
                        await client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.Username}! \U0001F607", cancellationToken: CancellationToken.None);
                        await client.SendTextMessageAsync(message.Chat.Id, $"Я умею ежедневно присылать по одной цитатке с умными идеями! Если хочешь, чтобы я присылал их и тебе, просто нажми на кнопку внизу или напиши в чат 'Получать цитатки'!", replyMarkup: keyboard, cancellationToken: CancellationToken.None);
                        break;
                    
                    case "Получать цитатки":
                        if (await _context.Users.FindAsync(message.Chat.Id) == null)
                        {
                            await _context.Users.AddAsync(new models.User(message.Chat.Id, message.Chat.Username ?? "unknown"), cancellationToken: CancellationToken.None);
                            await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы успешно подписались! Хорошего дня \U0001F619", replyMarkup: _unsub, cancellationToken: CancellationToken.None);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы уже подписались на получение цитаток", replyMarkup: _unsub, cancellationToken: CancellationToken.None);
                        }
                        break;
                    
                    case "Отписаться":
                        var userToRemove = await _context.Users.FindAsync(message.Chat.Id);
                        if (userToRemove != null)
                        {
                            _context.Users.Remove(userToRemove);
                            await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы успешно отписались! Больше докучать не буду \U0001F609", replyMarkup: _subscribe, cancellationToken: CancellationToken.None);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы уже отписались", replyMarkup: _subscribe, cancellationToken: CancellationToken.None);
                        }
                        break;
                    
                    default:
                        var keyboardDefault = await _context.Users.FindAsync(message.Chat.Id) == null ? _subscribe : _unsub;
                        await client.SendTextMessageAsync(message.Chat.Id, "\U0001F440", replyMarkup: keyboardDefault, cancellationToken: CancellationToken.None);
                        break;
                    
                }
            }
        }
    }
    
    private static Task Error(ITelegramBotClient client, Exception ex, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    
    
    
}