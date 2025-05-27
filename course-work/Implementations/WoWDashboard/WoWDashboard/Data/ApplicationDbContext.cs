using Microsoft.EntityFrameworkCore;
using WoWDashboard.Models;

namespace WoWDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<GearItem> GearItems { get; set; }
        public DbSet<RaidProgression> RaidProgressions { get; set; }
        public DbSet<UserCharacter> UserCharacters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalRealm).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalRegion).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Realm).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Level).IsRequired();
                entity.Property(e => e.Race).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Guild).HasMaxLength(255);
                entity.Property(e => e.CharacterClass).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RaiderIoScore).HasColumnType("double");
                entity.Property(e => e.AvatarUrl).HasMaxLength(500);

                entity.HasMany(c => c.GearItems)
                      .WithOne(g => g.Character)
                      .HasForeignKey(g => g.CharacterId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.RaidProgression)
                      .WithOne(r => r.Character)
                      .HasForeignKey<RaidProgression>(r => r.CharacterId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.UserCharacters)
                      .WithOne(uc => uc.Character)
                      .HasForeignKey(uc => uc.CharacterId);
            });

            modelBuilder.Entity<GearItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Slot).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Rarity).HasMaxLength(255);
                entity.Property(e => e.ItemLevel).IsRequired();
                entity.Property(e => e.ItemId).IsRequired();
                entity.HasOne(e => e.Character)
                      .WithMany(c => c.GearItems)
                      .HasForeignKey(e => e.CharacterId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RaidProgression>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RaidName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Summary).HasColumnType("text");
                entity.HasOne(e => e.Character)
                      .WithOne(c => c.RaidProgression)
                      .HasForeignKey<RaidProgression>(r => r.CharacterId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserCharacter>(entity =>
            {
                entity.HasKey(uc => new { uc.UserId, uc.CharacterId });

                entity.HasOne(uc => uc.User)
                      .WithMany(u => u.UserCharacters)
                      .HasForeignKey(uc => uc.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uc => uc.Character)
                      .WithMany(c => c.UserCharacters)
                      .HasForeignKey(uc => uc.CharacterId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}