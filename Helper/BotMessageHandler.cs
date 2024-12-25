using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Helper
{
    public static class BotMessageHandler
    {

        public static async Task SendPotentallyLongResponse(string response, CommandContext ctx)
        {
            if (response.Length > 2000)
            {
                string[] sentences = response.Split('.');
                string smallerResponse = "";
                List<string> smallerResponses = new List<string>();
                string[] array = sentences;
                foreach (string sentence in array)
                {
                    if (smallerResponse.Length + sentence.Length <= 2000)
                    {
                        smallerResponse = smallerResponse + " " + sentence;
                        continue;
                    }
                    if (sentence.Length <= 2000)
                    {
                        await ctx.RespondAsync("I'm sorry, part of this response is just too long to send over discord. Try asking for something simplier.");
                        break;
                    }
                    smallerResponses.Add(smallerResponse);
                    smallerResponse = sentence;
                }
                foreach (string sResponse in smallerResponses)
                {
                    await ctx.RespondAsync(sResponse);
                }
            }
            else
            {
                await ctx.RespondAsync(response);
            }
        }
    }
}
