using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {

    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=SocialMediaAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
            options => options.EnableRetryOnFailure());
    }

    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatsUsers> ChatsUsers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageStatus> MessageStatuses { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostVote> PostVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>().HasKey(chat => chat.Id);
        modelBuilder.Entity<ChatsUsers>().HasKey(chatsUsers => chatsUsers.Id);
        modelBuilder.Entity<Message>().HasKey(message => message.Id);
        modelBuilder.Entity<Notification>().HasKey(notification => notification.Id);
        modelBuilder.Entity<Post>().HasKey(post => post.Id);
        modelBuilder.Entity<PostVote>().HasKey(postVote => postVote.Id);
        modelBuilder.Entity<MessageStatus>().HasKey(messageStatus => messageStatus.Id);

        modelBuilder.Entity<Post>()
            .HasMany(post => post.PostVotes)
            .WithOne(postVote => postVote.Post)
            .HasForeignKey(postVote => postVote.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Chat>()
            .HasMany(chat => chat.Messages)
            .WithOne(message => message.Chat)
            .HasForeignKey(message => message.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Chat>()
            .HasMany(chat => chat.ChatsUsers)
            .WithOne(chatsUsers => chatsUsers.Chat)
            .HasForeignKey(chatsUsers => chatsUsers.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasMany(message => message.Notifications)
            .WithOne(notification => notification.Message)
            .HasForeignKey(notification => notification.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Message>()
            .HasMany(message => message.MessageStatuses)
            .WithOne(messageStatus => messageStatus.Message)
            .HasForeignKey(messageStatus => messageStatus.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
