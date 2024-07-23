using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using OpenAI.ObjectModels.ResponseModels.ImageResponseModel;
using OpenAI.ObjectModels.SharedModels;

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
            _ai = new OpenAIService(new OpenAiOptions
            {
                ApiKey = config.GetValue<string>("OpenAI")
            });
            _chatContextManager = new ChatContextManager();
        }

        [Command("edit"), Description("-i <instructions> -t <text> - This will attempt to edit the text using the instructions provided.")]
        public async Task Edit(CommandContext ctx, [RemainingText] string prompt)
        {
            string[] strings = prompt.Split('-');
            string input = string.Empty;
            string command = string.Empty;
            string[] array = strings;
            foreach (string s in array)
            {
                if (s.ToLower().StartsWith("text "))
                {
                    input = s.Substring(5);
                }
                else if (s.ToLower().StartsWith("t "))
                {
                    input = s.Substring(2);
                }
                else if (s.ToLower().StartsWith("instructions "))
                {
                    command = s.Substring(13);
                }
                else if (s.ToLower().StartsWith("i "))
                {
                    command = s.Substring(2);
                }
            }
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(command))
            {
                await ctx.RespondAsync("The edit command was not formatted correctly. We need to see '-i' followed by instructions and '-t' with the text.");
                return;
            }
            EditCreateRequest request = new EditCreateRequest
            {
                Input = input,
                Temperature = 0.5f,
                Model = OpenAI.ObjectModels.Models.TextDavinciV3,
                Instruction = command
            };
            Console.WriteLine(request);
            EditCreateResponse response = await _ai.Edit.CreateEdit(request);
            if (response.Successful)
            {
                if (response.Choices.Count > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("We have multiple choices here. Yay.\n");
                    for (int i = 0; i < response.Choices.Count; i++)
                    {
                        sb.AppendLine(i + 1 + response.Choices[i].Text);
                    }
                    await ctx.RespondAsync(sb.ToString());
                }
                else
                {
                    await ctx.RespondAsync(response.Choices[0].Text);
                }
            }
            else
            {
                await BotError(ctx, response);
            }
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
            }
        }

        [Command("write"), Description("<sentence fragment> - currently this command attempts to complete a sentence.")]
        public async Task Completion(CommandContext ctx, [RemainingText] string prompt)
        {
            CompletionCreateResponse response = await _ai.Completions.CreateCompletion(new CompletionCreateRequest
            {
                Prompt = prompt,
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_Instruct
            });
            Console.WriteLine(response);
            if (response.Successful)
            {
                Console.WriteLine($"These are the {response.Choices.Count()} choices I got back... ");

                foreach (ChoiceResponse choice in response.Choices)
                {
                    Console.WriteLine(choice);
                }
                await ctx.RespondAsync(response.Choices[0].Text);
            }
            else
            {
                await BotError(ctx, response);
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
                Model = OpenAI.ObjectModels.Models.Gpt_4
            });
            if (completionResult.Successful)
            {
                string response = completionResult.Choices?.First()?.Message?.Content ?? "There was an error.";
                chatContext.Messages.Add(ChatMessage.FromAssistant(response));
                await SendPotentallyLongResponse(response, ctx);
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
                Model = OpenAI.ObjectModels.Models.Gpt_4,
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
                SendPotentallyLongResponse(responseMessage, ctx);
            }
            else
            {
                await BotError(ctx, response);
            }
        }

        private async Task SendPotentallyLongResponse(string response, CommandContext ctx)
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
            await ctx.RespondAsync("There was a problem working with OpenAI. Please contact my maker if this keeps happening.");
        }
    }
}