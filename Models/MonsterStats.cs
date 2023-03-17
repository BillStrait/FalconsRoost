using System;
using System.Collections;
using FalconsRoost.Models;


namespace FalconsRoost.Models
{
    public abstract class MonsterStats : Stats
    {
        public int LowGold { get; set; }
        public int HighGold { get; set; }
        public int LowHP { get; set; }
        public int HighHP { get; set; }
        public int ExpValue { get; set; }

        public MonsterStats(string name)
            : base(name)
        {
            base.Name = name;
        }

        public void SetGoldAndHP()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            base.GoldOnHand = rand.Next(LowGold, HighGold);
            base.HitPoints = rand.Next(LowHP, HighHP);
        }
    }
}
