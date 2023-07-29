using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramQuoteBotProject;


public class SendQuoteJob : IJob
{
    private static models.TelegramQuoteDbContext _context;
    private static TelegramBotClient _client;
    private static ReplyKeyboardMarkup _unsub;

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("execute in SendQuoteJob"); //TODO: !!!!!!!!!!!!!!!!!!
        var jobDataMap = context.JobDetail.JobDataMap;
        _context = jobDataMap.Get("context") as models.TelegramQuoteDbContext ?? throw new ArgumentException("Invalid context!");
        _client = jobDataMap.Get("client") as TelegramBotClient ?? throw new ArgumentException("Invalid client!");
        _unsub = jobDataMap.Get("unsubKeyboard") as ReplyKeyboardMarkup ?? throw new ArgumentException("Invalid Unsubscribe Keyboard!");
        var quoteShuffle = jobDataMap.Get("quoteShuffle") as Stack<int> ?? throw new ArgumentException("Invalid Quote Shuffle!");
        var quoteCount = quoteShuffle.Count;
        
        if (_context.Users.Any() && quoteShuffle.Any())
        {
            var quoteId = quoteShuffle.Pop(); //TODO: EMPTY STACK !!
            quoteCount = quoteShuffle.Count;
            var quoteTarget = await _context.Quotes.FindAsync(quoteId) ?? throw new IndexOutOfRangeException("Invalid Quote Id from Shuffle Stack!");
            var quoteContent = quoteTarget.Content;
            var quoteAuthor = $"_{quoteTarget.Author} \U000000A9_";
            quoteTarget.Used = 1;
            await _context.SaveChangesAsync();

            var message = $"{quoteContent}{Environment.NewLine}{Environment.NewLine}{quoteAuthor}";
            foreach (var user in _context.Users)
            {
                await _client.SendTextMessageAsync(user.Id, message, replyMarkup: _unsub, parseMode: ParseMode.Markdown,cancellationToken: CancellationToken.None);
                if (quoteCount == 0)
                {
                    await _client.SendTextMessageAsync(user.Id, $"К сожалению, это была последняя цитатка из моего записного блокнотика \U0001F613{Environment.NewLine}Я сейчас же побегу искать новые и не менее занимательные цитатки, но это может занять какое то время.{Environment.NewLine}Прошу, не скучай без меня! Скоро вернусь! \U0001F601", parseMode: ParseMode.Markdown, cancellationToken: CancellationToken.None);
                    await _client.SendTextMessageAsync(user.Id, "\U0001F643", replyMarkup: _unsub, parseMode: ParseMode.Markdown,cancellationToken: CancellationToken.None);
                }
            }
        }
        if (quoteCount == 0)
        {
            var handler = jobDataMap.Get("handler") as Action ?? throw new ArgumentException("Invalid NoQuotesHandler!");
            handler.Invoke();
            Console.WriteLine("event in SendQuoteJob");         //TODO: !!!!!!!!!!
        }
    }
}