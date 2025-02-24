using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
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
    public class LeagueOfComicGeeksScraper : BaseScraper
    {
        public LeagueOfComicGeeksScraper(IConfiguration config) : base(config)
        {
        }

        public List<DiscordEmbedBuilder> GetPullList(CommandContext ctx, string userName)
        {
            var embeds = new List<DiscordEmbedBuilder>();

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
                return new List<DiscordEmbedBuilder> { embed };
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
                
                if(embed.Fields.Count == 25 && comicNodes.Count > 25)
                {
                    embeds.Add(embed);
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
            }
            else
            {
                embed.WithFooter("No comics found.");
            }

            //We can't send more than 25 embed fields.
            embeds.Add(embed);
            return embeds;
        }

        public async Task<string> ComicToBookClub(CommandContext ctx, string url)
        {
            //this method needs to verify the url accurately pulls a comic book from league of comic geeks.
            //if it does, we should get the title, the isbn, the distributor sku, the channel, the server, and the user and save it to the database.
            HtmlNode html = null;
            try
            {
                html = GetHtml(url);
            }
            catch (Exception ex)
            {
                return "There was an error getting the page. It must be in a similar format to: https://leagueofcomicgeeks.com/comic/5959203/something-is-killing-the-children-vol-1-tp";
            }

            //lets grab the title from the h1 tag.
            var title = html.SelectSingleNode("//h1")?.InnerText ?? "Unknown Title";

            //select divs with the class 'details-addtl-block'
            var details = html.SelectNodes("//div[contains(@class, 'details-addtl-block')]");
            //In each details div there is a 'name' div and a 'value' div. If the details name div contains 'ISBN' we save the value to isbn.
            var isbn = details.FirstOrDefault(x => x.SelectSingleNode(".//div[contains(@class, 'name')]").InnerText.Contains("ISBN", StringComparison.OrdinalIgnoreCase))?.SelectSingleNode(".//div[contains(@class, 'value')]")?.InnerText ?? "Unknown ISBN";
            //If the details name div contains 'Distributor SKU' we save the value to distributorSku.
            var distributorSku = details.FirstOrDefault(x => x.SelectSingleNode(".//div[contains(@class, 'name')]").InnerText.Contains("Distributor SKU", StringComparison.OrdinalIgnoreCase))?.SelectSingleNode(".//div[contains(@class, 'value')]")?.InnerText ?? "Unknown Distributor SKU";

            //we also want to pull the canonical link from the head.
            var canonicalLink = html.SelectSingleNode("//link[@rel='canonical']")?.Attributes["href"]?.Value ?? url;

            return "Your comic has been added to the book club's next vote.";
        }


    }
}
