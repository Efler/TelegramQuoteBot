using Quartz;
using Telegram.Bot;

namespace TelegramQuoteBotProject;


public class SendQuoteJob : IJob
{
    private static models.TelegramQuoteDbContext _context;
    private static TelegramBotClient _client;
    private static int i = 1;       //TODO: DELETE AFTER TESTING !!!
    
    
    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;
        _context = jobDataMap.Get("context") as models.TelegramQuoteDbContext ?? throw new ArgumentException("Invalid context!");
        _client = jobDataMap.Get("client") as TelegramBotClient ?? throw new ArgumentException("Invalid client!");
        
        //TODO: CHANGE AFTER TESTING !!!
        foreach (var user in _context.Users)
        {
            await _client.SendTextMessageAsync(user.Id, $"Ку, {user.Nickname}! \U0001F607 #{i}", cancellationToken: CancellationToken.None);
        }
        i++;
    }
}