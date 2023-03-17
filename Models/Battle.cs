using System;
using System.Collections.Generic;
using System.Numerics;
using DSharpPlus.Entities;
using FalconsRoost.Models;

namespace FalconsRoost.Models
{

    public class Battle
    {
        public int Id { get; set; }

        public Player Player { get; set; }

        public List<MonsterStats> Monsters { get; set; }

        public string Story { get; set; }

        public Battle(DiscordUser user)
        {
            Player = new Player(user);
            Monsters = new List<MonsterStats>();
            Story = string.Empty;
        }

        public string QuickBattle()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int monsterCount = rand.Next(1, 5);
            int gobCount = 0;
            int ratCount = 0;
            for (int i = 0; i < monsterCount; i++)
            {
                int chance = rand.Next(1, 100);
                if (chance >= 75)
                {
                    gobCount++;
                }
                else
                {
                    ratCount++;
                }
            }
            if (gobCount == 1)
            {
                Monsters.Add(new Goblin("Goblin"));
            }
            else
            {
                for (int k = 0; k < gobCount; k++)
                {
                    Monsters.Add(new Goblin("Goblin " + (k + 1)));
                }
            }
            if (ratCount == 1)
            {
                Monsters.Add(new Rat("Rat"));
            }
            else
            {
                for (int j = 0; j < ratCount; j++)
                {
                    Monsters.Add(new Rat("Rat " + (j + 1)));
                }
            }
            Story = "Once upon a time... " + Player.Name + " was walking through the woods when they encountered:";
            foreach (MonsterStats monster2 in Monsters)
            {
                Story = Story + " " + monster2.Name;
            }
            Story += " - and they faught.\n";
            while (Player.HitPoints > 0 && Monsters.Count > 0)
            {
                AttackDescription res = Player.EquippedWeapon.AttackRole(Player, Monsters[0]);
                Story += res.Description;
                if (res.Lethal)
                {
                    Monsters.RemoveAt(0);
                }
                foreach (MonsterStats monster in Monsters)
                {
                    if (Player.HitPoints > 0)
                    {
                        res = monster.EquippedWeapon.AttackRole(monster, Player);
                        Story += res.Description;
                    }
                }
            }
            return Story;
        }
    }

}
