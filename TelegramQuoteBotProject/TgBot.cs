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
    
    private static readonly ReplyKeyboardMarkup Subscribe = new
    (
        new[] { new[] { new KeyboardButton("Получать цитатки") } }
    );
    
    private static readonly ReplyKeyboardMarkup Unsub = new
    (
        new[] { new[] { new KeyboardButton("Отписаться") } }
    );

    private static IScheduler _scheduler;
    
    private static bool _noQuotes = false;

    public static void TgBotStart(models.TelegramQuoteDbContext context, string telegramToken, string cronTimeExpression)
    {
        Subscribe.ResizeKeyboard = true;
        Unsub.ResizeKeyboard = true;
        _context = context;
        var handler = new Action(NoQuotesHandler);
        var quoteShuffle = new Stack<int>(_context.Quotes
            .Where(quote => quote.Used == 0).AsEnumerable()
            .Shuffle()
            .Select(yeet => yeet.Id));

        var client = new TelegramBotClient(telegramToken);

        ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        _scheduler = schedulerFactory.GetScheduler().Result;
        _scheduler.Start();
        
        var jobDataMap = new JobDataMap();
        jobDataMap.Put("context", _context);
        jobDataMap.Put("client", client);
        jobDataMap.Put("quoteShuffle", quoteShuffle);
        jobDataMap.Put("unsubKeyboard", Unsub);
        jobDataMap.Put("handler", handler);
        
        var jobDetail = JobBuilder.Create<SendQuoteJob>().SetJobData(jobDataMap).Build();
        
        var trigger = TriggerBuilder.Create()
            .WithIdentity("NineAM", "Time")
            .WithSchedule(CronScheduleBuilder.CronSchedule(cronTimeExpression))  //TODO: CHANGE AFTER TESTING!
            .Build();
        
        _scheduler.ScheduleJob(jobDetail, trigger);
        
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
                Console.WriteLine($"{DateTime.Now}:  {message.Chat.Id}  {message.Chat.Username}, Message: {message.Text}"); //TODO!!!!!
                switch (message.Text)
                {
                    case "/start":
                        var keyboard = await _context.Users.FindAsync(message.Chat.Id) == null ? Subscribe : Unsub;
                        await client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.Username}! \U0001F607", cancellationToken: CancellationToken.None);
                        if(!_noQuotes) await client.SendTextMessageAsync(message.Chat.Id, $"Я присылаю своим пользователям по одной цитатке каждое утро с интересненькими мыслями! Если хочешь, чтобы я присылал их и тебе, просто нажми на кнопку внизу или напиши в чат 'Получать цитатки'! \U0001F60A", replyMarkup: keyboard, cancellationToken: CancellationToken.None);
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, "Я присылаю своим пользователям по одной цитатке каждое утро с интересненькими мыслями!", cancellationToken: CancellationToken.None);
                            await client.SendTextMessageAsync(message.Chat.Id, "На данный момент я уже исчерпал все свои любимые цитатки, поэтому на какое-то время отлучусь за новыми. Если хочешь, можешь подписаться заранее, нажав на кнопочку внизу или же отправив сообщение 'Получать цитатки'! Когда я вернусь с новыми цитатками, я обязательно поделюсь ими с тобой!", cancellationToken: CancellationToken.None);
                            await client.SendTextMessageAsync(message.Chat.Id, "\U0001F917", replyMarkup: keyboard, cancellationToken: CancellationToken.None);
                        }
                        break;
                    
                    case "Получать цитатки":
                        if (await _context.Users.FindAsync(message.Chat.Id) == null)
                        {
                            await _context.Users.AddAsync(new models.User(message.Chat.Id, message.Chat.Username ?? "unknown"), cancellationToken: CancellationToken.None);
                            await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
                            if(!_noQuotes) await client.SendTextMessageAsync(message.Chat.Id, $"Ты успешно подписался! Хорошего дня \U00002728", replyMarkup: Unsub, cancellationToken: CancellationToken.None);
                            else
                            {
                                await client.SendTextMessageAsync(message.Chat.Id, $"Ты успешно подписался! Как только вернусь из своего путешествия со свежими цитатками, сразу дам знать! \U00002728", replyMarkup: Unsub, cancellationToken: CancellationToken.None);
                            }
                        }
                        else
                        {
                            if(!_noQuotes) await client.SendTextMessageAsync(message.Chat.Id, $"Не переживай, Ты уже подписался на получение цитаток \U0000270C", replyMarkup: Unsub, cancellationToken: CancellationToken.None);
                            else await client.SendTextMessageAsync(message.Chat.Id, $"Не переживай, Ты уже подписался на получение цитаток{Environment.NewLine}Скоро вернусь с новыми цитатками, не скучай! \U0000270C", replyMarkup: Unsub, cancellationToken: CancellationToken.None);
                        }
                        break;
                    
                    case "Отписаться":
                        var userToRemove = await _context.Users.FindAsync(message.Chat.Id);
                        if (userToRemove != null)
                        {
                            _context.Users.Remove(userToRemove);
                            await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
                            await client.SendTextMessageAsync(message.Chat.Id, $"Ты успешно отписался! Больше докучать не буду \U0001F609", replyMarkup: Subscribe, cancellationToken: CancellationToken.None);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, $"Все хорошо, Ты уже отписался \U0001F44C", replyMarkup: Subscribe, cancellationToken: CancellationToken.None);
                        }
                        break;
                    
                    default:
                        var keyboardDefault = await _context.Users.FindAsync(message.Chat.Id) == null ? Subscribe : Unsub;
                        await client.SendTextMessageAsync(message.Chat.Id, "\U0001F440", cancellationToken: CancellationToken.None);
                        if(!_noQuotes) await client.SendTextMessageAsync(message.Chat.Id, "Я плохо понимаю отдельные фразы, лучше используй кнопочки внизу \U0001F607", replyMarkup: keyboardDefault, cancellationToken: CancellationToken.None);
                        else await client.SendTextMessageAsync(message.Chat.Id, $"Извини, я плохо понимаю отдельные фразы \U0001F613{Environment.NewLine}Пока я в поисках интересных цитаток, можешь подписаться с помощью кнопочек снизу \U0001F607", replyMarkup: keyboardDefault, cancellationToken: CancellationToken.None);
                        break;
                    
                }
            }
        }
    }
    
    private static Task Error(ITelegramBotClient client, Exception ex, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private static void NoQuotesHandler()
    {
        _scheduler.PauseAll();
        _noQuotes = true;
        Console.WriteLine("event in TgBot");   //TODO: !!!!!!!!!!!!!!!!!!!
    }

}