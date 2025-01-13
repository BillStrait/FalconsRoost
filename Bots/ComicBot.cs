using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using FalconsRoost.WebScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FalconsRoost.Models.db;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace FalconsRoost.Bots
{
    public class ComicBot : ExtendedCommandModule
    {
        IConfigurationRoot _config;
        public ComicBot(IConfigurationRoot config, FalconsRoostDBContext? context) : base(context)
        {
            _config = config;
        }

        //[Command("bcadd"), Description("Add a comic to the vote for the channel's book club. Provide a link to the comic on league of comic geeks.")]
        //public async Task BookClubAddCommand(CommandContext ctx, [RemainingText] string locString)
        //{
        //    var player = await GetPlayerAndUpdateMessageHistory(ctx);
        //    string response = string.Empty;
        //    if (string.IsNullOrWhiteSpace(locString))
        //    {
        //        response = "You need to provide a link to the comic on League of Comic Geeks.";
        //        LogResponse(ctx, response);
        //        await ctx.RespondAsync(response);
        //        return;
        //    }

        //    if (!locString.StartsWith("https://leagueofcomicgeeks.com/comic/"))
        //    {
        //        response = "That doesn't look like a link to a comic on League of Comic Geeks. The link should begin with https://leagueofcomicgeeks.com/comic/";
        //        LogResponse(ctx, response);
        //        await ctx.RespondAsync(response);
        //        return;
        //    }

        //    if (!Uri.TryCreate(locString, UriKind.Absolute, result: out Uri? uriResult) && uriResult != null && uriResult.Scheme == Uri.UriSchemeHttps)
        //    {
        //        response = "That is not a valid URL.";
        //        LogResponse(ctx, response);
        //        await ctx.RespondAsync(response);
        //        return;
        //    }


        //    var stripped = $"{uriResult!.Scheme}://{uriResult!.Host}{uriResult.AbsolutePath}"; //exclamation marks to suppress stupid null reference compiler warning.

        //    //this may still have javascript on it, but we're not using javascript so it should be fine.
        //    var scraper = new LeagueOfComicGeeksScraper();

        //    response = await scraper.ComicToBookClub(ctx, stripped);

        //    LogResponse(ctx, response);

        //    await ctx.RespondAsync(response);

        //}

        [Command("pulllist"), Description("Retrieve the pull list from League of Comic Geeks for the specified user.")]
        public async Task PullListCommand(CommandContext ctx, [RemainingText] string userName)
        {
            var user = await GetPlayerAndUpdateMessageHistory(ctx);

            if (string.IsNullOrEmpty(userName))
            {
                if (!string.IsNullOrWhiteSpace(user.LeagueOfComicGeeksName))
                {
                    userName = user.LeagueOfComicGeeksName;
                }
                else
                {
                    var errorMessage = "You need to provide a username or use the LoCGRegister command.";
                    LogResponse(ctx, errorMessage);
                    await ctx.RespondAsync(errorMessage);
                    return;
                }
            }
            var scraper = new LeagueOfComicGeeksScraper();
            var embeds = scraper.GetPullList(ctx, userName);

            foreach (var embed in embeds)
            {
                LogResponse(ctx, embed.Title);
                await ctx.RespondAsync(embed: embed);
            }
        }

        [Command("LoCGRegister"), Description("Register your League of Comic Geeks username with the bot.")]
        public async Task LoCGRegisterCommand(CommandContext ctx, [RemainingText] string userName)
        {
            var player = await GetPlayerAndUpdateMessageHistory(ctx);
            string response = string.Empty;

            if (string.IsNullOrWhiteSpace(userName) || userName.Any(c => !char.IsLetterOrDigit(c) || c == '_'))
            {
                response = "You need to provide a valid username.";
                LogResponse(ctx, response);
                await ctx.RespondAsync(response);
                return;
            }
            player.LeagueOfComicGeeksName = userName;
            await SavePlayer(player);
            response = $"Your League of Comic Geeks username has been set to {userName}.";
            LogResponse(ctx, response);
            await ctx.RespondAsync(response);

        }
    }
}
