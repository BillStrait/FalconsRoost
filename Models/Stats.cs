using System.Collections.Generic;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{

    public abstract class Stats
    {
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int HitPoints { get; set; }
        public int GoldOnHand { get; set; }
        public Weapon EquippedWeapon { get; set; }
        public List<Weapon> Weapons { get; set; }

        public bool DealDamage(int dmg)
        {
            HitPoints -= dmg;
            return HitPoints < 0;
        }

        public Stats(string name)
        {
            Name = name;
            Weapons = new List<Weapon>
        {
            new Weapon()
        };
            EquippedWeapon = Weapons[0];
        }
    }
}
