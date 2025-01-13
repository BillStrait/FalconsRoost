using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.db
{
    public class FalconsRoostDBContext : DbContext
    {
        public DbSet<LaunchLog> LaunchLogs { get; set; } = null!;
        public DbSet<ChatMessageLog> ChatMessageLogs { get; set; } = null!; 
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Weapon> Weapons { get; set; } = null!;


        public FalconsRoostDBContext(DbContextOptions<FalconsRoostDBContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //use the modelBuilder to create the table
            modelBuilder.Entity<LaunchLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.TimeStamp).IsRequired();
                entity.Property(e => e.Message).IsRequired(false);
            });

            modelBuilder.Entity<ChatMessageLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Source).IsRequired();
                entity.Property(e => e.TimeStamp).IsRequired();
                entity.Property(e => e.Message).IsRequired(false);
                entity.Property(e => e.Channel).IsRequired(false);
                entity.Property(e => e.ChannelId).IsRequired(false);
                entity.Property(e => e.Guild).IsRequired(false);
                entity.Property(e => e.GuildId).IsRequired(false);
            });

            modelBuilder.Entity<Stats>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Strength).IsRequired();
                entity.Property(e => e.Dexterity).IsRequired();
                entity.Property(e => e.Intelligence).IsRequired();
                entity.Property(e => e.Wisdom).IsRequired();
                entity.Property(e => e.HitPoints).IsRequired();
                entity.Property(e => e.GoldOnHand).IsRequired();
                entity.HasMany(e => e.Weapons).WithOne();
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasBaseType<Stats>();
                entity.Property(e => e.DiscordId).IsRequired();
                entity.Property(e => e.UserName).IsRequired();
                entity.Property(e => e.LastMessage).IsRequired();
                entity.Property(e => e.LastSeen).IsRequired();
                entity.Property(e=> e.LeagueOfComicGeeksName).IsRequired(false);
                entity.Property(e=>e.Experience).IsRequired();
            });

            modelBuilder.Entity<Weapon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.DiceSize).IsRequired();
                entity.Property(e=>e.ActionTextName).IsRequired();
                entity.Property(e => e.NumberOfDice).IsRequired();
                entity.Property(e => e.DamageBonus).IsRequired();
                entity.Property(e => e.SellValue).IsRequired();
                entity.Property(e => e.BuyValue).IsRequired();
                entity.Property(e => e.OwnerId).IsRequired();
                entity.HasOne(c=>c.Owner).WithMany().HasForeignKey(c => c.OwnerId);
            });
        }
    }
}
