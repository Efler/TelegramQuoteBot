namespace TelegramQuoteBotProject.models;

public class User
{
    
    public long Id { get; set; }

    public string Nickname { get; set; }

    public User(long id, string nickname)
    {
        Id = id;
        Nickname = nickname;
    }
    
}