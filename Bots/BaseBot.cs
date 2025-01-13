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
    }
}
