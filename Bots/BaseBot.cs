using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Bots
{
    public class BaseBot : BaseCommandModule
    {
        private IConfigurationRoot _config;

        public BaseBot(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("awake"), Description("Make sure the bot is awake.")]
        public async Task AwakeCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("I'm here boss. You got me on the internet. Good job. My version number is " + _config.GetValue<string>("versionNumber") + ".");
        }

        [Command("name"), Description("Make sure the bot knows your name.")]
        public async Task NameCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("I think your name is " + ctx.User.Username + ".");
        }

        [Command("quickfight"), Description("Have the bot generate a battle for you.")]
        public async Task QuickFightCommand(CommandContext ctx)
        {
            Battle battle = new Battle(ctx.User);
            await ctx.RespondAsync(battle.QuickBattle());
        }

    }
}
