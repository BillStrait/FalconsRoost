using System;
using System.Collections;
using DSharpPlus.Entities;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{


    public class Player : Stats
    {
        public string UserName { get; set; } = string.Empty;
        public ulong DiscordId { get; set; }
        public int Experience { get; set; } = 0;
        public string LeagueOfComicGeeksName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastSeen { get; set; }

        public Player() : base("Unknown User")
        {
            Id = Guid.NewGuid();
            LastSeen = DateTime.Now;
        }
        public Player(DiscordUser user)
            : base(user.Username)
        {
            DiscordId = user.Id;            
            Random rand = new Random((int)DateTime.Now.Ticks);
            base.Strength = rand.Next(3, 18);
            base.Dexterity = rand.Next(3, 18);
            base.Wisdom = rand.Next(3, 18);
            base.Intelligence = rand.Next(3, 18);
            base.HitPoints = rand.Next(4, 8);
        }

        
    }

}
