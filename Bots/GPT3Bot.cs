using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FalconsRoost.Helper;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;
using Betalgo.Ranul.OpenAI;
using Betalgo.Ranul.OpenAI.Extensions;
using Betalgo.Ranul.OpenAI.Managers;
using Betalgo.Ranul.OpenAI.ObjectModels;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels.ImageResponseModel;
using Betalgo.Ranul.OpenAI.ObjectModels.SharedModels;

namespace FalconsRoost.Bots
{
    public class GPT3Bot : BaseCommandModule
    {
        private OpenAIService _ai;
        private ChatContextManager _chatContextManager;
        private IConfigurationRoot _config;
        public GPT3Bot(IConfigurationRoot config)
        {
            _config = config;
            _ai = new OpenAIService(new OpenAIOptions()
            {
                ApiKey = config.GetValue<string>("OpenAI") ?? throw new System.Exception("OpenAI key not found.")
            });

            _chatContextManager = new ChatContextManager();
        }

        
        [Command("draw"), Description("<prompt> - This asks Dall-E 2 to generate an image based on the prompt.")]
        public async Task Draw(CommandContext ctx, [RemainingText] string prompt)
        {
            ImageCreateResponse response = await _ai.Image.CreateImage(new ImageCreateRequest
            {
                Prompt = prompt,
                N = 4,
                Size = StaticValues.ImageStatics.Size.Size1024,
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = ctx.User.Username
            });

            Console.WriteLine($"We're creating an image for {ctx.User.Username}. Their prompt was: {prompt}");
            Console.WriteLine(response);
            if (response.Successful)
            {
                DiscordMessageBuilder builder = new DiscordMessageBuilder();
                HttpClient client = new HttpClient();
                Dictionary<string, Stream> files = new Dictionary<string, Stream>();
                for (int i = 0; i < response.Results.Count(); i++)
                {
                    ImageCreateResponse.ImageDataResult result = response.Results[i];
                    if (!string.IsNullOrEmpty(result.B64))
                    {
                        byte[] bytes = Convert.FromBase64String(response.Results[0].B64);
                        MemoryStream stream2 = new MemoryStream(bytes);
                        files.Add($"dall-e_image_{i}.jpg", stream2);
                    }
                    else if (!string.IsNullOrEmpty(result.Url))
                    {
                        Stream stream = await client.GetStreamAsync(result.Url);
                        files.Add($"dall-e_image_{i}.jpg", stream);
                    }
                }
                builder.AddFiles(files);
                builder.Content = "These are the results we got back from OpenAI's Dall-E 2:";
                await ctx.RespondAsync(builder);
            }
            else
            {
                await BotError(ctx, response);
                return;   
            }
        }

        
        [Command("chat"), Description("<prompt> - This will ask ChatGPT to respond based on your current context. Context resets after 1 hour of inactivity.")]
        public async Task Chat(CommandContext ctx, [RemainingText] string prompt)
        {
            ChatContext chatContext = GetChatContext(ctx);

            chatContext.Messages.Add(ChatMessage.FromUser(prompt));
            chatContext.TimeStamp = DateTime.Now;
            ChatCompletionCreateResponse completionResult = await _ai.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = chatContext.Messages,
                Model = Betalgo.Ranul.OpenAI.ObjectModels.Models.Gpt_4o
            });
            if (completionResult.Successful)
            {
                string response = completionResult.Choices?.First()?.Message?.Content ?? "There was an error.";
                chatContext.Messages.Add(ChatMessage.FromAssistant(response));
                await BotMessageHandler.SendPotentallyLongResponse(response, ctx);
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                await BotError(ctx, completionResult);
            }
        }

        [Command("whois"), Description("<person> - Attempts to provide information about a person or company.")]
        public async Task WhoIs(CommandContext ctx, [RemainingText] string Prompt)
        {
            var response = await _ai.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Model = Betalgo.Ranul.OpenAI.ObjectModels.Models.Gpt_4o,
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser($"Please return a grounded response. I want details on {Prompt}. If they are a fictional person, include who created them and when. If they are a comic character, include their first appearance. If they are a company, tell me their revenue and primary leadership.")
                }
            });

            Console.WriteLine(response);
            if (response.Successful)
            {
                var context = GetChatContext(ctx);
                var responseMessage = response.Choices?.First()?.Message?.Content ?? "There was an error.";
                context.Messages.Add(ChatMessage.FromAssistant(responseMessage));
                await BotMessageHandler.SendPotentallyLongResponse(responseMessage, ctx);
            }
            else
            {
                await BotError(ctx, response);
            }
        }

        


        private ChatContext GetChatContext(CommandContext ctx)
        {
            ChatContext chatContext = new ChatContext();
            if (_chatContextManager.ChatContexts.Any((ChatContext c) => c.UserId == ctx.User.Username && c.TimeStamp.CompareTo(DateTime.Now.AddHours(-1.0)) >= 0))
            {
                chatContext = _chatContextManager.ChatContexts.First((ChatContext c) => c.UserId == ctx.User.Username && c.TimeStamp.CompareTo(DateTime.Now.AddHours(-1.0)) >= 0);
            }
            else
            {
                _chatContextManager.ChatContexts.Add(new ChatContext
                {
                    UserId = ctx.User.Username
                });
            }
            return chatContext;
        }

        private async Task BotError(CommandContext ctx, BaseResponse response)
        {
            Console.WriteLine(response);
            Console.WriteLine("Error type: " + response.Error?.Type ?? "unknown");
            await ctx.RespondAsync("There was a problem working with OpenAI. Frequently this is because we have sent too many commands or the response would violate OpenAI's policy. Please wait a few minutes and try again. Contact my maker if this keeps happening.");
        }
    }
}