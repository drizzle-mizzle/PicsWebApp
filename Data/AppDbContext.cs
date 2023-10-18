using Microsoft.EntityFrameworkCore;
using PicsWebApp.Models.Database;
using System.Runtime.Intrinsics.X86;

namespace PicsWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Friendship>()
                .HasOne(f => f.Proposer)
                .WithMany(u => u.FriendshipsOut)
                .HasForeignKey(f => f.ProposerId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Friendship>()
                .HasOne(f => f.Receiver)
                .WithMany(u => u.FriendshipsInc)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(mb);
        }
    }
}
