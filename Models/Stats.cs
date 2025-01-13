using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{

    public abstract class Stats
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Unknown";
        public int Strength { get; set; } = 8;
        public int Dexterity { get; set; } = 8;
        public int Intelligence { get; set; } = 8;
        public int Wisdom { get; set; } = 8;
        public int HitPoints { get; set; } = 3;
        public int GoldOnHand { get; set; } = 1;
        public List<Weapon> Weapons { get; set; }

        public bool DealDamage(int dmg)
        {
            HitPoints -= dmg;
            return HitPoints < 0;
        }

        public Stats(string name)
        {
            Name = name;
            Weapons = new List<Weapon>();
        }
    }
}
