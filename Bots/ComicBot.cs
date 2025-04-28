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
using FalconsRoost.Models;
using DSharpPlus;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FalconsRoost.Models.Alerts;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace FalconsRoost.Bots
{
    public class ComicBot : ExtendedCommandModule
    {
        private IConfigurationRoot _config;
        private FalconsRoostDBContext _dbcontext;
        private IShopifyAlert _thirdEyeScraper;
        public ComicBot(IConfigurationRoot config, FalconsRoostDBContext context, IShopifyAlert thirdEyeScraper) : base(context)
        {
            _config = config;
            _dbcontext = context;
            _thirdEyeScraper = thirdEyeScraper;
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
                    await LogResponse(ctx, errorMessage);
                    await ctx.RespondAsync(errorMessage);
                    return;
                }
            }
            var scraper = new LeagueOfComicGeeksScraper(_config, _dbcontext);
            var embeds = scraper.GetPullList(ctx, userName);

            foreach (var embed in embeds)
            {
                await LogResponse(ctx, embed.Title);
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
                await LogResponse(ctx, response);
                await ctx.RespondAsync(response);
                return;
            }
            player.LeagueOfComicGeeksName = userName;
            await SavePlayer(player);
            response = $"Your League of Comic Geeks username has been set to {userName}.";
            await LogResponse(ctx, response);
            await ctx.RespondAsync(response);

        }

        [Command("rmcsalert"), Description("Allows admin to register a channel to get notifications when MCS new releases are posted.")]
        public async Task RegisterAlertCommand(CommandContext ctx)
        {
            //We only want the admin to be able to set this up.
            var adminId = _config.GetValue<ulong>("DiscordAdminId");
            if (adminId != 0 && ctx.User.Id != adminId)
            {
                var response = "You do not have permission to use this command.";
                await LogResponse(ctx, response);
                await ctx.RespondAsync(response);
                return;
            }
            var task = _dbContext.AlertTasks.Include("AlertMessages").FirstOrDefault(t => t.AlertType == AlertType.MCSNCBD);
            if (task == null)
            {
                var response = "This alert has not been set up. Please contact the bot admin.";
                await LogResponse(ctx, response);
                await ctx.RespondAsync("This alert has not been set up. Please contact the bot admin.");
                return;
            }

            task.AlertMessages.Add(new AlertMessage()
            {
                ChannelId = ctx.Channel.Id,
                AlertTarget = "here",
                Message = "New releases have been posted on [MyComicShop](https://www.mycomicshop.com). Check them out!",
            });
            await _dbContext.SaveChangesAsync();

            await ctx.RespondAsync("This channel will now receive alerts when new releases are posted on MyComicShop.");
        }

        [Command("testalert"), Description("Allows an admin to test the scraper service. Will @ the invoker.")]
        public async Task TestMCSScraper(CommandContext ctx)
        {
            //We only want the admin to be able to set this up.
            var adminId = _config.GetValue<ulong>("DiscordAdminId");
            if (adminId != 0 && ctx.User.Id != adminId)
            {
                var response = "You do not have permission to use this command.";
                await LogResponse(ctx, response);
                await ctx.RespondAsync(response);
                return;
            }

            try
            {
                var centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Central Standard Time" : "America/Chicago");
                var centralTime = TimeZoneInfo.ConvertTime(DateTime.Now, centralTimeZone);
                var message = $"The current time is {centralTime.ToString("HH:mm:ss")} and we're giving it a shot.";
                await LogResponse(ctx, message);
                await ctx.RespondAsync(message);

                var tasks = _dbContext.AlertTasks.Include("AlertMessages").Where(t => t.Enabled).ToList();
                var taskMessage = new StringBuilder();
                foreach (var dbTask in tasks)
                {
                    taskMessage.AppendLine($"Task {dbTask.Id} is {(dbTask.ShouldRun() ? "running" : "not running")} and it's next run is {dbTask.NextRun}.");
                    await LogResponse(ctx, taskMessage.ToString());
                    await ctx.RespondAsync(taskMessage.ToString());
                }

            }
            catch (Exception ex)
            {
                var message = $"An error occurred getting the time.: {ex.Message}";
                await LogResponse(ctx, message);
                await ctx.RespondAsync(message);

            }
            var task = new AlertTask()
            {
                AlertType = AlertType.MCSNCBD,
                AlertMessages = new List<AlertMessage>() { new AlertMessage() { AlertTarget = ctx.User.Id.ToString(), Message = "MCS has books for sale.", ChannelId = ctx.Channel.Id } },
                RunOnce = true
            };

            var scraper = new MyComicShopScraper(_config, _dbcontext);
            var newReleases = await scraper.NCBDCheck(task);

            var finalMessage = $"The scraper has run. If you did not receive a message, something went wrong. The check returned {newReleases}.";
            await LogResponse(ctx, finalMessage);
            await ctx.RespondAsync(finalMessage);
        }

        [Command("3esearch"), Description("Search for a comic on Third Eye Comics. Enables you to create a stock alert.")]
        public async Task ThirdEyeSearchCommand(CommandContext ctx, [RemainingText] string searchString)
        {
            var player = await GetPlayerAndUpdateMessageHistory(ctx);
            string response = string.Empty;
            if (string.IsNullOrWhiteSpace(searchString))
            {
                response = "You need to provide a search string.";
                await LogResponse(ctx, response);
                await ctx.RespondAsync(response);
                return;
            }

            var userId = ctx.User.Id;
            var channelId = ctx.Channel.Id;
            var embeds = await _thirdEyeScraper.SearchForComic(searchString, channelId, userId);


            foreach (var embed in embeds)
            {
                await LogResponse(ctx, embed.Title);
                await ctx.RespondAsync(embed: embed);
            }
        }

        [Command("3eAlert"), Description("Register a stock alert for a comic on Third Eye Comics.")]
        public async Task ThirdEyeAlertCommand(CommandContext ctx, [RemainingText] string targetGuid)
        {
            var player = await GetPlayerAndUpdateMessageHistory(ctx);
            string message = string.Empty;
            if (string.IsNullOrWhiteSpace(targetGuid))
            {
                message = "You need to provide a target id. The full command is listed after you search for a comic with 3esearch";
                await LogResponse(ctx, message);
                await ctx.RespondAsync(message);
                return;
            }

            var alert = TempAlertCache.Get(targetGuid);
            if(alert == null)
            {
                message = "That target does not exist or has expired. Please try again.";
                await LogResponse(ctx, message);
                await ctx.RespondAsync(message);
                return;
            }

            if(alert.IsWatched)
            {
                message = "That target is already being watched. This channel will be notified if the item becomes available within 24 hours.";
                await LogResponse(ctx, message);
                await ctx.RespondAsync(message);
                return;
            }

            TempAlertCache.Activate(targetGuid);

            message = $"You are now watching {alert.Title} for availability. This channel will be notified if the item becomes available within 24 hours.";
            await LogResponse(ctx, message);
            await ctx.RespondAsync(message);

        }
    }
}
