namespace TelegramQuoteBotProject.models;


public class Quote
{
    
    public int Id { get; set; }

    public int Used { get; set; }

    public string Content { get; set; }
    
    public string Author { get; set; }

    public Quote(int used, string content, string author)
    {
        Used = used;
        Content = content;
        Author = author;
    }
    
}