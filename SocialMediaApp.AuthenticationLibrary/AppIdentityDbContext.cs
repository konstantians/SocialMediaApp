using SocialMediaApp.SharedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SocialMediaApp.Authentication;

public class AppIdentityDbContext : IdentityDbContext
{
    public AppIdentityDbContext()
    {

    }

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=SocialMediaAppAuthenticationDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
            options => options.EnableRetryOnFailure());
    }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Friendship>().HasKey(friendship => friendship.Id);
        
        modelBuilder.Entity<AppUser>()
            .HasMany(user => user.Friendships)
            .WithOne(friendship => friendship.Friend)
            .HasForeignKey(friendship => friendship.FriendId);

        base.OnModelCreating(modelBuilder);
    }
}