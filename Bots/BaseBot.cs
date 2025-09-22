using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FalconsRoost.Models;
using FalconsRoost.Models.db;
using FalconsRoost.WebScrapers;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Bots
{
    public class BaseBot : ExtendedCommandModule //ExtendedCommandModule handles DB Interactions.
    {
        private IConfigurationRoot _config;
        

        public BaseBot(IConfigurationRoot config, FalconsRoostDBContext? context) : base(context)
        {
            _config = config;
        }

        [Command("awake"), Description("Make sure the bot is awake.")]
        public async Task AwakeCommand(CommandContext ctx)
        {
            LogRequest(ctx);

            var responseMessage = "I'm here boss. You got me on the internet. Good job. My version number is " + _config.GetValue<string>("versionNumber") + ".";
            LogResponse(ctx, responseMessage);
            await ctx.RespondAsync(responseMessage);
        }

        [Command("name"), Description("Make sure the bot knows your name.")]
        public async Task NameCommand(CommandContext ctx)
        {
            LogRequest(ctx);

            var responseMessage = "I think your name is " + ctx.User.Username + ".";

            LogResponse(ctx, responseMessage);
            await ctx.RespondAsync(responseMessage);
        }

        [Command("quickfight"), Description("Have the bot generate a battle for you.")]
        public async Task QuickFightCommand(CommandContext ctx)
        {
            LogRequest(ctx);
            Battle battle = new Battle(ctx.User);
            var response = battle.QuickBattle();
            LogResponse(ctx, response);
            await ctx.RespondAsync(response);
        }

        [Command("dice"), Description("Roll dice using notation like 2d6+5")]
        public async Task RollCommand(CommandContext ctx, [RemainingText] string diceNotation)
        {
            LogRequest(ctx);

            try
            {
                // Default if nothing provided
                if (string.IsNullOrWhiteSpace(diceNotation))
                {
                    await ctx.RespondAsync("Usage: !dice 2d6+5");
                    return;
                }

                // Parse the dice notation (basic: XdY+Z)
                var match = System.Text.RegularExpressions.Regex.Match(diceNotation.Trim(), @"^(\d*)d(\d+)([+-]\d+)?$");
                if (!match.Success)
                {
                    await ctx.RespondAsync("Invalid dice format. Example: 2d6+5");
                    return;
                }

                int numDice = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
                int sides = int.Parse(match.Groups[2].Value);
                int modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

                if (numDice <= 0 || sides <= 0)
                {
                    await ctx.RespondAsync("Number of dice and sides must be positive.");
                    return;
                }

                Random rng = new Random();
                List<int> rolls = new List<int>();

                for (int i = 0; i < numDice; i++)
                    rolls.Add(rng.Next(1, sides + 1));

                int total = rolls.Sum() + modifier;

                string response = $"{ctx.User.Username} rolled {diceNotation}: " +
                                  $"[{string.Join(", ", rolls)}]" +
                                  (modifier != 0 ? $" {match.Groups[3].Value}" : "") +
                                  $" = **{total}**";

                LogResponse(ctx, response);
                await ctx.RespondAsync(response);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Error parsing dice notation.");
                Console.WriteLine(ex);
            }
        }

    }
}
