using Microsoft.EntityFrameworkCore;

namespace TelegramQuoteBotProject.models;


public class TelegramQuoteDbContext : DbContext
{
    
    private readonly string _connectionString;
    
    public TelegramQuoteDbContext() {}

    public TelegramQuoteDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public TelegramQuoteDbContext(DbContextOptions<TelegramQuoteDbContext> options)
        : base(options) {}

    public DbSet<Quote> Quotes { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(_connectionString);

}