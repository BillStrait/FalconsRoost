using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{
    public class Weapon
    {
        public string Name { get; set; } = "Fists";
        public string ActionTextName { get; set; } = "the weapons they were born with";
        public int DiceSize { get; set; } = 2;
        public int NumberOfDice { get; set; } = 1;
        public int DamageBonus { get; set; } = 0;
        public int SellValue { get; set; } = 0;
        public int BuyValue { get; set; } = 0;

        public Weapon()
        {
        }

        public Weapon(string name, string actionTextName, int numberOfDice, int diceSize, int damageBonus, int sellValue, int buyValue)
        {
            Name = name;
            ActionTextName = actionTextName;
            NumberOfDice = numberOfDice;
            DiceSize = diceSize;
            DamageBonus = damageBonus;
            SellValue = sellValue;
            BuyValue = buyValue;
        }

        public AttackDescription AttackRole(Stats source, Stats target)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 6);
            defaultInterpolatedStringHandler.AppendFormatted(source.Name);
            defaultInterpolatedStringHandler.AppendLiteral(" attacked ");
            defaultInterpolatedStringHandler.AppendFormatted(target.Name);
            defaultInterpolatedStringHandler.AppendLiteral(" with ");
            defaultInterpolatedStringHandler.AppendFormatted(ActionTextName);
            defaultInterpolatedStringHandler.AppendLiteral(" (");
            defaultInterpolatedStringHandler.AppendFormatted(NumberOfDice);
            defaultInterpolatedStringHandler.AppendLiteral("d");
            defaultInterpolatedStringHandler.AppendFormatted(DiceSize);
            defaultInterpolatedStringHandler.AppendLiteral("+");
            defaultInterpolatedStringHandler.AppendFormatted(DamageBonus);
            defaultInterpolatedStringHandler.AppendLiteral("). ");
            AttackDescription description = new AttackDescription(defaultInterpolatedStringHandler.ToStringAndClear());
            bool flag = true;
            List<int> dmgRolls = new List<int>();
            for (int i = 0; i < NumberOfDice; i++)
            {
                dmgRolls.Add(rand.Next(1, DiceSize));
            }
            int baseDamage = dmgRolls.Sum();
            int strBonus = 0;
            int strength = source.Strength;
            int num = strength;
            strBonus = ((num <= 16) ? ((num <= 8) ? ((num > 4) ? (-1) : (-2)) : ((num > 12) ? 1 : 0)) : ((num <= 18) ? 2 : 0));
            int totalDamage = baseDamage + DamageBonus + strBonus;
            if (totalDamage < 0)
            {
                totalDamage = 0;
            }
            string diceRolls = "[";
            foreach (int roll in dmgRolls)
            {
                string text = diceRolls;
                defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
                defaultInterpolatedStringHandler.AppendLiteral(" ");
                defaultInterpolatedStringHandler.AppendFormatted(roll);
                defaultInterpolatedStringHandler.AppendLiteral(" ");
                diceRolls = text + defaultInterpolatedStringHandler.ToStringAndClear();
            }
            diceRolls += "]";
            string description2 = description.Description;
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 4);
            defaultInterpolatedStringHandler.AppendLiteral("They **hit** for a total of **");
            defaultInterpolatedStringHandler.AppendFormatted(totalDamage);
            defaultInterpolatedStringHandler.AppendLiteral("** (base damage: ");
            defaultInterpolatedStringHandler.AppendFormatted(diceRolls);
            defaultInterpolatedStringHandler.AppendLiteral(", weapon bonus: ");
            defaultInterpolatedStringHandler.AppendFormatted(DamageBonus);
            defaultInterpolatedStringHandler.AppendLiteral(", strBonus: ");
            defaultInterpolatedStringHandler.AppendFormatted(strBonus);
            defaultInterpolatedStringHandler.AppendLiteral(").\n");
            description.Description = description2 + defaultInterpolatedStringHandler.ToStringAndClear();
            if (target.DealDamage(totalDamage))
            {
                description.Description = description.Description + target.Name + " has died!\n";
                description.Lethal = true;
            }
            return description;
        }
    }
}
