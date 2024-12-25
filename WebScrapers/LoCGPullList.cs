using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using ScrapySharp;
using ScrapySharp.Core;
using ScrapySharp.Html.Forms;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.WebScrapers
{
    public class LoCGPullList
    {
        ScrapingBrowser _browser = new ScrapingBrowser();

        public LoCGPullList()
        {

        }

        public async Task GetPullList(CommandContext ctx, string userName)
        {
            var response = new StringBuilder();
            response.AppendLine($"Here is the pull list for {userName}:\n");

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Pull List for {userName}",
                Color = DiscordColor.PhthaloGreen
            };


            //the url is pretty straight forward.
            var target = $"https://leagueofcomicgeeks.com/profile/{userName}/pull-list";
            var pullListPage = GetHtml(target);
            //there is a ul with id "comic-list-issues" - inside of that ul are the comics.
            var comicNodes = pullListPage.SelectNodes("//ul[@id='comic-list-issues']//li");

            //if there are no comicNodes, we should try to find the first ul that has the class 'comic-list-thumbs' - this is the old layout.
            if (comicNodes == null || comicNodes.Count == 0)
            {
                comicNodes = pullListPage.SelectNodes("//ul[contains(@class, 'comic-list-thumbs')]//li");
            }


            decimal total = 0;
            //if we still have no comicNodes, we should just return.
            if (comicNodes == null || comicNodes.Count == 0)
            {
                embed.WithFooter("No comics found.");
                response.AppendLine("\n\nNo comics found.");
                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            //we have comics, let's loop through them.
            foreach (var node in comicNodes)
            {
                //there is a div called 'cover' - inside it is an a tag, with an img tag inside. We want the src of the img tag, without any arguments.
                var imageUrl = node.SelectSingleNode(".//div[contains(@class, 'cover')]//a//img").Attributes["data-src"]?.Value ?? "Unknown image url"; //This is an amazon s3 link.
                //let's trim off the ?34215 or whatever is at the end.
                imageUrl = imageUrl.Split('?')[0];

                //next, there's a div called 'publisher' - we want the inner text.
                var publisher = node.SelectSingleNode(".//div[contains(@class, 'publisher')]")?.InnerText ?? "Unknown Publisher";
                publisher = publisher.Trim();
                //next there's a div called 'title' - it has a link inside it, we want the inner text of that link.
                var title = node.SelectSingleNode(".//div[contains(@class, 'title')]//a")?.InnerText ?? "Unknown Title";
                title = title.Trim();
                //we also want the link target.
                var link = $"https://leagueofcomicgeeks.com" + node.SelectSingleNode(".//div[contains(@class, 'title')]//a")?.Attributes["href"]?.Value?? "/unknown";
                //finally there's a div called 'details' and it has a div called price inside. It's text is a not clean, but there should be $xx.xx in there.
                var price = node.SelectSingleNode(".//div[contains(@class,'details')]//span[contains(@class, 'price')]")?.InnerText ?? "$0.00";
                //we need to clean that up. Let's use regex to get the decimal from the string.
                var priceDecimal = decimal.Parse(System.Text.RegularExpressions.Regex.Match(price, @"(\d+\.\d+)").Value);
                total += priceDecimal;

                var mainText = $"[{title} - {publisher} - {priceDecimal.ToString("C", CultureInfo.GetCultureInfo("en-US"))}]({link})";
                embed.AddField(title, $"[{title} - {publisher} - {priceDecimal.ToString("C", CultureInfo.GetCultureInfo("en-US"))}]({link})", false);
                response.Append("\n* " + mainText);

                if(embed.Fields.Count == 25 && comicNodes.Count > 25)
                {
                    await ctx.RespondAsync(embed);
                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"Continued Pull List for {userName}",
                        Color = DiscordColor.PhthaloGreen
                    };
                }
            }
            if(total>0)
            {
                embed.WithFooter($"Total: {total.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");
                response.AppendLine($"\n\nTotal: {total.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");
            }
            else
            {
                embed.WithFooter("No comics found.");
                response.AppendLine("\n\nNo comics found.");
            }

            //We can't send more than 25 embed fields.
            await ctx.RespondAsync(embed: embed.Build());
        }

        private HtmlNode GetHtml(string url)
        {
            _browser.Timeout = TimeSpan.FromMinutes(5);
            _browser.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
            var pageResult = _browser.NavigateToPage(new Uri(url));
            //lets respect our server, sleep so we don't call more than 5 times a second.
            Thread.Sleep(200);
            return pageResult.Html;
        }
    }
}
