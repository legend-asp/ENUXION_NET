using MessengerApp1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Friendship Logic
            builder.Entity<Friendship>(entity =>
            {
                entity.HasOne(f => f.User)
                    .WithMany(u => u.Friends)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Friend)
                    .WithMany()
                    .HasForeignKey(f => f.FriendId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. Personal Messages Logic
            builder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                    .WithMany()
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Group Members Logic
            builder.Entity<GroupMember>(entity =>
            {
                entity.HasOne(gm => gm.User)
                    .WithMany()
                    .HasForeignKey(gm => gm.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gm => gm.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(gm => gm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Group Messages Logic
            builder.Entity<GroupMessage>(entity =>
            {
                entity.HasOne(gm => gm.Sender)
                    .WithMany()
                    .HasForeignKey(gm => gm.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gm => gm.Group)
                    .WithMany(g => g.Messages)
                    .HasForeignKey(gm => gm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 5. ENU TRANSACTIONS (Фикс ошибки 1785)
            builder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.Sender)
                    .WithMany()
                    .HasForeignKey(t => t.SenderId)
                    .OnDelete(DeleteBehavior.Restrict); // Важно: Restrict вместо Cascade

                entity.HasOne(t => t.Receiver)
                    .WithMany()
                    .HasForeignKey(t => t.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict); // Важно: Restrict вместо Cascade

                // Настройка точности для валюты
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            });

            // Настройка точности баланса в таблице пользователей
            builder.Entity<ApplicationUser>()
                .Property(u => u.Balance)
                .HasColumnType("decimal(18,2)");
        }
    }
}