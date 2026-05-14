using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.TimelineOwner)
            .WithMany()
            .HasForeignKey(p => p.TimelineOwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DirectMessage>()
            .HasOne(d => d.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(d => d.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DirectMessage>()
            .HasOne(d => d.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(d => d.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}