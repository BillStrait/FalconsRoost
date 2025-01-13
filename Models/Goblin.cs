namespace FalconsRoost.Models
{
    public class Goblin : MonsterStats
    {
        public Goblin(string name)
            : base(name)
        {
            base.Strength = 7;
            base.Dexterity = 9;
            base.Intelligence = 3;
            base.Wisdom = 1;
            base.LowGold = 2;
            base.HighGold = 4;
            base.LowHP = 5;
            base.HighHP = 8;
            base.ExpValue = 10;
            SetGoldAndHP();
            Weapon standardWeapon = new Weapon("Dagger", "a worn rusty dagger", 1, 4, 0, 1, 5);
            base.Weapons.Add(standardWeapon);
        }
    }
}