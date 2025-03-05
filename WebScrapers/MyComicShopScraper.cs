using FalconsRoost.Models.Alerts;
using FalconsRoost.Models.db;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Tls;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.WebScrapers
{
    public class MyComicShopScraper : BaseScraper
    {
        private readonly TimeZoneInfo centralTimeZone;
        public MyComicShopScraper(IConfiguration config, FalconsRoostDBContext context) : base(config, context)
        {
            centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Central Standard Time" : "America/Chicago");
        }

        public async Task<bool> NCBDCheck(AlertTask task)
        {
            var updated = false;
            
            while (task.ShouldRun())
            {
                var target = "https://www.mycomicshop.com/newreleases?dw=-1";
                var page = GetHtml(target);

                var errorNode = page.SelectSingleNode("//div[@id='FRERRORPARSEERROR']");
                if (errorNode != null && task.RunOnce)
                {
                    //if we detect an error I want to know about it.
                    var message = errorNode.InnerText;
                    await SendMessage(task.AlertMessages.First(), message);
                    return false;
                }

                var comicsList = page.SelectNodes("//div[@class='addcart']//a");
                updated = comicsList?.Any() ?? false;

                
                var centralTime = TimeZoneInfo.ConvertTime(DateTime.Now, centralTimeZone);

                if (updated || task.RunOnce)
                {
                    break;
                }
                else
                {
                    //we want to sleep between 5 and 18 seconds.
                    var sleepTime = new Random().Next(5000, 18000);
                    Thread.Sleep(sleepTime);
                }
            }

            if (updated)
            {
                foreach (var message in task.AlertMessages)
                {
                    if (message != null)
                        await SendMessage(message, null);
                }
            }

            //finally, we need to set the task to run next time.
            task.LastRun = DateTime.Now;
            task.NextRun = task.RecurrenceUnit switch
            {
                Recurrence.Second => task.LastRun.AddSeconds(task.RecurrenceInterval),
                Recurrence.Minute => task.LastRun.AddMinutes(task.RecurrenceInterval),
                Recurrence.Daily => task.LastRun.AddDays(task.RecurrenceInterval),
                Recurrence.Weekly => task.LastRun.AddDays(task.RecurrenceInterval * 7),
                Recurrence.Monthly => task.LastRun.AddMonths(task.RecurrenceInterval),
                Recurrence.Yearly => task.LastRun.AddYears(task.RecurrenceInterval),
                _ => task.LastRun
            };

            return updated;
        }

    }
}
