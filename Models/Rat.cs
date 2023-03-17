using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models
{
    public class Rat : MonsterStats
    {
        public Rat(string name)
            : base(name)
        {
            base.Strength = 1;
            base.Dexterity = 12;
            base.Intelligence = 1;
            base.Wisdom = 1;
            base.LowGold = 0;
            base.HighGold = 1;
            base.LowHP = 1;
            base.HighHP = 3;
            base.ExpValue = 1;
            SetGoldAndHP();
            Weapon standardWeapon = (base.EquippedWeapon = new Weapon("Teeth", "slavia frothed teeth", 1, 4, 0, 0, 0));
            base.Weapons.Add(standardWeapon);
        }
    }
}
