using Engine.TelegramBot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Engine.TelegramBot;

public class TelegramBotDbContext : DbContext
{
    public DbSet<ProviderForChat> ProviderForChats { get; set; }
    public DbSet<TelegramUser> Users { get; set; }
    public DbSet<UserAttribute> UserAttributes { get; set; }
    public TelegramBotDbContext(DbContextOptions<TelegramBotDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TelegramUser>(entity =>
        {
            entity.HasKey(e => e.ChatId);
            entity.Property(p => p.ChatId)
                .IsRequired();
        });

        modelBuilder.Entity<UserAttribute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.HasOne(u => u.User)
                .WithMany(o => o.Attributes)
                .HasForeignKey(o => o.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
