using Microsoft.Extensions.Configuration;

namespace TelegramQuoteBotProject;


//TODO:     1. ADD QUOTES INTO SENDING
//TODO:     2. CHANGE USED COLUMN
//TODO:     3. MAKE FINAL DIALOG
//TODO:     4. ERROR WITH USER'S DELETING CHAT
//TODO:     5. CHANGE/DELETE CODE FOR TESTING
//TODO:     5. MAKE RESETTING USED COLUMN FUNCTION


public static class Program
{
    
    public static void Main()
    {
        
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build() as IConfiguration;
        
        var connectionString = config.GetSection("ConnectionStrings")["QuoteDBConnection"];
        var enableParsing = config.GetSection("Parsing")["EnableParsing"]?.ToLower();
        var telegramToken = config["TelegramToken"];


        using var context = new models.TelegramQuoteDbContext(connectionString!);

        if (enableParsing?.Equals("true") ?? false)
        {
            try
            {
                var url = config.GetSection("Parsing")["Url"];
                if (url != null) Parsing.ParseIntoQuoteDb(url, context);
                else Console.WriteLine("Empty Url!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Caught: {ex.Message}");
            }
        }


        if (telegramToken != null)
        {
            try
            {
                TgBot.TgBotStart(context, telegramToken);
            }
            catch(Exception ex) { Console.WriteLine($"Exception: {ex}, Message: {ex.Message}"); }
        }
        else Console.WriteLine("Empty Telegram Token!");
        
    }
    
}