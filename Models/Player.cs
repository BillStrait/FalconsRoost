using System;
using System.Collections;
using DSharpPlus.Entities;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{


    public class Player : Stats
    {
        public ulong Id { get; set; }
        public int Experience { get; set; }

        public Player(DiscordUser user)
            : base(user.Username)
        {
            Id = user.Id;
            Weapon standardWeapon = new Weapon("Short Sword", "a glimmering steel short sword", 1, 6, 1, 5, 10);
            base.Weapons.Add(standardWeapon);
            base.EquippedWeapon = standardWeapon;
            Random rand = new Random((int)DateTime.Now.Ticks);
            base.Strength = rand.Next(3, 18);
            base.Dexterity = rand.Next(3, 18);
            base.Wisdom = rand.Next(3, 18);
            base.Intelligence = rand.Next(3, 18);
            base.HitPoints = rand.Next(4, 8);
        }
    }

}
