using System.Runtime.CompilerServices;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels.ResponseModels.ImageResponseModel;
using OpenAI.GPT3.ObjectModels.SharedModels;

namespace FalconsRoost.Bots
{
    public class GPT3Bot
    {
        private OpenAIService _ai;
        private ChatContextManager _chatContextManager;

        public GPT3Bot()
        {
        }

        public GPT3Bot(IConfigurationRoot config)
        {
            _ai = new OpenAIService(new OpenAiOptions
            {
                ApiKey = config.GetValue<string>("OpenAI")
            });
            _chatContextManager = new ChatContextManager();
        }

        public async Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            DiscordMessage msg = e.Message;
            if (!(msg == null) && !msg.Author.IsBot && msg.Content.StartsWith("$"))
            {
                string command = msg.Content.Substring(1);
                if (command.ToLower().StartsWith("write"))
                {
                    await Completion(e, command.Substring(5));
                }
                else if (command.ToLower().StartsWith("draw"))
                {
                    await Draw(e, command.Substring(4));
                }
                else if (command.ToLower().StartsWith("edit"))
                {
                    await Edit(e, command.Substring(4));
                }
                else if (command.ToLower().StartsWith("chat"))
                {
                    await Chat(e, command.Substring(4));
                }
            }
        }

        public async Task Edit(MessageCreateEventArgs e, string prompt)
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
                await e.Message.RespondAsync("The edit command was not formatted correctly. We need to see '-i' followed by instructions and '-t' with the text.");
                return;
            }
            EditCreateRequest request = new EditCreateRequest
            {
                Input = input,
                Temperature = 0.5f,
                Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV3,
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
                    await e.Message.RespondAsync(sb.ToString());
                }
                else
                {
                    await e.Message.RespondAsync(response.Choices[0].Text);
                }
            }
            else
            {
                await BotError(e, response);
            }
        }

        public async Task Draw(MessageCreateEventArgs e, string prompt)
        {
            ImageCreateResponse response = await _ai.Image.CreateImage(new ImageCreateRequest
            {
                Prompt = prompt,
                N = 4,
                Size = StaticValues.ImageStatics.Size.Size1024,
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = e.Author.Username
            });

            Console.WriteLine($"We're creating an image for {e.Author.Username}. Their prompt was: {prompt}");
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
                await e.Message.RespondAsync(builder);
            }
            else
            {
                await BotError(e, response);
            }
        }

        public async Task Completion(MessageCreateEventArgs e, string prompt)
        {
            CompletionCreateResponse response = await _ai.Completions.CreateCompletion(new CompletionCreateRequest
            {
                Prompt = prompt,
                Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV3
            });
            Console.WriteLine(response);
            if (response.Successful)
            {
                Console.WriteLine($"These are the {response.Choices.Count()} choices I got back... ");

                foreach (ChoiceResponse choice in response.Choices)
                {
                    Console.WriteLine(choice);
                }
                await e.Message.RespondAsync(response.Choices[0].Text);
            }
            else
            {
                await BotError(e, response);
            }
        }

        public async Task Chat(MessageCreateEventArgs e, string prompt)
        {
            MessageCreateEventArgs e2 = e;
            ChatContext chatContext = new ChatContext();
            if (_chatContextManager.ChatContexts.Any((ChatContext c) => c.UserId == e2.Author.Username && c.TimeStamp.CompareTo(DateTime.Now.AddHours(-1.0)) >= 0))
            {
                chatContext = _chatContextManager.ChatContexts.First((ChatContext c) => c.UserId == e2.Author.Username && c.TimeStamp.CompareTo(DateTime.Now.AddHours(-1.0)) >= 0);
            }
            else
            {
                _chatContextManager.ChatContexts.Add(new ChatContext
                {
                    UserId = e2.Author.Username
                });
            }
            chatContext.Messages.Add(ChatMessage.FromUser(e2.Message.Content));
            chatContext.TimeStamp = DateTime.Now;
            ChatCompletionCreateResponse completionResult = await _ai.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = chatContext.Messages,
                Model = "gpt-4"
            });
            if (completionResult.Successful)
            {
                string response = completionResult.Choices.First().Message.Content;
                chatContext.Messages.Add(ChatMessage.FromAssistant(response));
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
                            await e2.Message.RespondAsync("I'm sorry, part of this response is just too long to send over discord. Try asking for something simplier.");
                            break;
                        }
                        smallerResponses.Add(smallerResponse);
                        smallerResponse = sentence;
                    }
                    foreach (string sResponse in smallerResponses)
                    {
                        await e2.Message.RespondAsync(sResponse);
                    }
                }
                else
                {
                    await e2.Message.RespondAsync(response);
                }
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                await BotError(e2, completionResult);
            }
        }

        private async Task BotError(MessageCreateEventArgs e, BaseResponse response)
        {
            Console.WriteLine(response);
            await e.Message.RespondAsync("There was a problem working with OpenAI. Please contact my maker if this keeps happening.");
        }
    }
}