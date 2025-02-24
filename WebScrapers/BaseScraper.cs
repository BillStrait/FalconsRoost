using DSharpPlus;
using FalconsRoost.Models.Alerts;
using FalconsRoost.Models.DB.Migrations;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace FalconsRoost.WebScrapers
{
    public abstract class BaseScraper
    {
        protected ScrapingBrowser _browser = new ScrapingBrowser();
        private IConfiguration _config;

        public BaseScraper(IConfiguration config)
        {
            _config = config;
        }

        protected HtmlNode GetHtml(string url)
        {
            _browser.Timeout = TimeSpan.FromMinutes(5);
            _browser.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
            _browser.AllowAutoRedirect = true;
            _browser.AllowMetaRedirect = true;
            _browser.IgnoreCookies = true;
            WebPage pageResult = null;
            try
            {
                pageResult = _browser.NavigateToPage(new Uri(url));
                //lets respect our server, sleep so we don't call more than 5 times a second.
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error getting the page.", ex);
            }

            return pageResult.Html;
        }

        protected async Task SendMessage(AlertMessage message, string? content)
        {
            var connection = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.GetValue<string>("DiscordToken"),
                TokenType = TokenType.Bot,
                Intents = (DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents),
            });

            var channel = await connection.GetChannelAsync(message.ChannelId);
            var messageContent = message.GenerateAlertMessage(content);
            await channel.SendMessageAsync(messageContent);
        }
    }
}
