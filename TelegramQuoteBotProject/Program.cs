using Microsoft.Extensions.Configuration;

namespace TelegramQuoteBotProject;


//TODO:     3. MAKE FINAL DIALOG
//TODO:     5. CHANGE/DELETE CODE FOR TESTING
//TODO:     8. ECHO @all FUNCTION
//TODO:     9. LOGGING
//TODO:     9. MOMENT WITH BLOCKING BOT


public static class Program
{
    
    public static void Main()
    {
        
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build() as IConfiguration;
        
        var connectionString = config.GetSection("ConnectionStrings")["QuoteDBConnection"];
        var enableParsing = config.GetSection("Parsing")["EnableParsing"]?.ToLower();
        var enableResetUsedField = config["UsedReset"]?.ToLower();
        var enableFillUsedField = config["UsedFill"]?.ToLower();
        var telegramToken = config["TelegramToken"];
        var cronTimeExpression = config["CronTimeExpression"];

        using var context = new models.TelegramQuoteDbContext(connectionString!);

        if (enableParsing?.Equals("true") ?? false)
        {
            try
            {
                var url = config.GetSection("Parsing")["Url"];
                if (url != null)
                {
                    QuoteDbConfiguration.ParseIntoQuoteDb(url, context);
                    Console.WriteLine("Parsing successful!");
                }
                else Console.WriteLine("Empty Url!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Caught: {ex.Message}");
                return;
            }
        }

        if (enableResetUsedField?.Equals("true") ?? false)
        {
            try
            {
                QuoteDbConfiguration.ResetUsedField(context);
                Console.WriteLine("Reset Used Field successful!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Caught: {ex.Message}");
                return;
            }
        }
        
        if (enableFillUsedField?.Equals("true") ?? false)
        {
            try
            {
                QuoteDbConfiguration.FillUsedField(context);
                Console.WriteLine("Fill Used Field successful!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Caught: {ex.Message}");
                return;
            }
        }
        
        if (telegramToken is { Length: > 0 } && cronTimeExpression is { Length: > 0 })
        {
            try
            {
                TgBot.TgBotStart(context, telegramToken, cronTimeExpression);
            }
            catch(Exception ex) { Console.WriteLine($"Exception: {ex}, Message: {ex.Message}"); }
        }
        else Console.WriteLine("Empty Configuration!");
        
    }
    
}