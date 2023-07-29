using Microsoft.EntityFrameworkCore;

namespace TelegramQuoteBotProject;


public static class QuoteDbConfiguration
{

    public static void ParseIntoQuoteDb(string url, models.TelegramQuoteDbContext context)
    {
        context.Database.ExecuteSqlRaw("DELETE FROM Quotes");

        using var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = System.Net.DecompressionMethods.All
        };
        using var client = new HttpClient(handler);
        using var response = client.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
            var html = response.Content.ReadAsStringAsync().Result;
            if (!string.IsNullOrEmpty(html))
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var quotes = doc.DocumentNode.SelectNodes(
                    ".//div[@class='_1eYTt']" +
                        "//p[@class]");
                if (quotes != null && quotes.Count > 0)
                {
                    var quotesForDb = new List<models.Quote>();
                    var contentText = "!none!";
                    foreach (var quote in quotes)
                    {
                        var singleQuote = quote.SelectSingleNode(".//span");
                        if (singleQuote != null)
                        {
                            var content = singleQuote.SelectSingleNode(".//b");
                            if (content != null)
                            {
                                contentText = content.InnerText;
                                if (contentText.Contains("&nbsp;"))
                                {
                                    contentText = contentText.Replace("&nbsp;", "");
                                }
                                while(contentText.Length > 0 &&
                                      (char.IsDigit(contentText[0]) || contentText[0] == '.'))
                                    contentText = contentText[1..];
                                if (contentText.Length == 0)
                                {
                                    content = content.NextSibling;
                                    contentText = content.InnerText;
                                    if (contentText.Contains("&nbsp;"))
                                    {
                                        contentText = contentText.Replace("&nbsp;", "");
                                    }
                                    while(contentText.Length > 0 &&
                                          (char.IsDigit(contentText[0]) || contentText[0] == '.'))
                                        contentText = contentText[1..];
                                }
                                if (contentText[0] == ' ') contentText = contentText[1..];

                                if (contentText.Length < 8)
                                {
                                    content = content.NextSibling.NextSibling;
                                    var contentTextP2 = content.InnerText;
                                    if (contentTextP2.Contains("&nbsp;"))
                                        contentTextP2 = contentTextP2.Replace("&nbsp;", "");
                                    content = content.NextSibling.NextSibling;
                                    var contentTextP3 = content.InnerText;
                                    contentText = contentText + " - " + contentTextP2 + " - " + contentTextP3;
                                }
                            }
                            else
                            {
                                var author = singleQuote.SelectSingleNode(".//em");
                                if (author != null)
                                {
                                    var authorText = singleQuote.InnerText;
                                    if (authorText.Contains("&nbsp;"))
                                    {
                                        authorText = authorText.Replace("&nbsp;", "");
                                    }
                                    quotesForDb.Add(new models.Quote(0, contentText, authorText));
                                    contentText = "!none!";
                                }
                                else
                                {
                                    contentText = singleQuote.InnerText;
                                    if (contentText.Contains("&nbsp;"))
                                    {
                                        contentText = contentText.Replace("&nbsp;", "");
                                    }
                                    while(contentText.Length > 0 &&
                                          (char.IsDigit(contentText[0]) || contentText[0] == '.'))
                                        contentText = contentText[1..];
                                    if (contentText[0] == ' ') contentText = contentText[1..];
                                }
                            }
                        }
                    }
                    context.Quotes.AddRange(quotesForDb);
                }
            }
        }

        context.SaveChanges();
    }

    public static void ResetUsedField(models.TelegramQuoteDbContext context)
    {
        if (context.Quotes.Any())
            foreach (var quote in context.Quotes)
            {
                if(quote.Used != 0) quote.Used = 0;
            }

        context.SaveChanges();
    }
    
    public static void FillUsedField(models.TelegramQuoteDbContext context)
    {
        var i = 2;
        if (context.Quotes.Any())
            foreach (var quote in context.Quotes)
            {
                if (i > 0)
                {
                    i--;
                    quote.Used = 0;
                    continue;
                }
                if(quote.Used != 1) quote.Used = 1;
            }

        context.SaveChanges();
    }
    
}