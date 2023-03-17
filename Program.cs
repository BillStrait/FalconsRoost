// FalconsRoost.Program
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using FalconsRoost;
using FalconsRoost.Bots;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;

namespace FalconsRoost
{


    internal class Program
    {
        private static GPT3Bot bot;

        private static string versionNumber = "0.0.0.4";

        private static IConfigurationRoot _config;

        private static void Main(string[] args)
        {
            if (args.Any((string c) => c.ToLower().StartsWith("--dt")) && args.Any((string c) => c.ToLower().StartsWith("--oa")))
            {
                MemoryConfigSetUp(args);
            }
            else
            {
                SecretConfigSetUp();
            }
            bot = new GPT3Bot(_config);
            MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Builds the config file with arguements passed in.
        /// Falls back to User Secrets if the parameters are missing.
        /// </summary>
        /// <param name="args"></param>
        private static void MemoryConfigSetUp(string[] args)
        {
            string dt = string.Empty;
            string oa = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower().StartsWith("--dt"))
                {
                    dt = args[i + 1];
                }
                if (args[i].ToLower().StartsWith("--oa"))
                {
                    oa = args[i + 1];
                }
            }
            if (!string.IsNullOrEmpty(dt) && !string.IsNullOrEmpty(oa))
            {
                _config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddInMemoryCollection
                    (
                        new List<KeyValuePair<string, string?>>
                        {
                                new KeyValuePair<string, string?>("DiscordToken", dt),
                                new KeyValuePair<string, string?>("OpenAI", oa)
                        }
                    )
                    .Build();
            }
            else
            {
                SecretConfigSetUp();
            }
        }

        /// <summary>
        /// Builds the _config with user secrets.
        /// </summary>
        private static void SecretConfigSetUp()
        {
            _config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddUserSecrets<Program>().Build();
        }

        private static async Task MainAsync()
        {
            DiscordClient discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.GetValue<string>("DiscordToken"),
                TokenType = TokenType.Bot,
                Intents = (DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents)
            });
            Console.WriteLine("I've started up.");
            discord.MessageCreated += async delegate (DiscordClient s, MessageCreateEventArgs e)
            {
                Console.WriteLine("I've caught a message. It says " + e.Message);
            };
            discord.MessageCreated += Discord_MessageCreated;
            discord.MessageCreated += bot.HandleCommandAsync;
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task Discord_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            string[] words = e.Message.Content.ToLower().Split(' ');
            string command = words[0];
            if (command.StartsWith('!'))
            {
                switch (command)
                {
                    case "!awake":
                        await e.Message.RespondAsync("I'm here boss. You got me on the internet. Good job. My version number is " + versionNumber + ".");
                        break;
                    case "!name":
                        await e.Message.RespondAsync("I think your name is " + e.Author.Username);
                        break;
                    case "!quickfight":
                        {
                            Battle battle = new Battle(e.Author);
                            await e.Message.RespondAsync(battle.QuickBattle());
                            break;
                        }
                    case "!commands":
                        await GetCommands(e);
                        break;
                    case "!help":
                        await GetCommands(e);
                        break;
                    case "!h":
                        await GetCommands(e);
                        break;
                    case "!command":
                        await GetCommands(e);
                        break;
                    default:
                        await e.Message.RespondAsync("I'm not sure what you said. Try something else, use !commands for a list of what I can do. For the record, you said " + command + ".");
                        break;
                }
            }
        }

        private static async Task GetCommands(MessageCreateEventArgs e)
        {
            List<string> commands = new List<string> { "There are two types of commands. ! are free. $ cost the bot a little bit of money.", "!awake - Make sure the bot is awake.", "!name - Make sure the bot knows your name.", "!quickfight - have the bot generate a battle for you.", "!commands - returns this dialog.", "$write <sentence fragment> - currently this command attempts to complete a sentence.", "$draw <prompt> - This asks Dall-E 2 to generate an image based on the prompt.", "$edit -i <instructions> -t <text> - This will attempt to edit the text using the instructions provided.", "$chat <prompt> - This will ask ChatGPT to respond based on your current context. Context resets after 1 hour of inactivity." };
            StringBuilder sb = new StringBuilder();
            foreach (string command in commands)
            {
                sb.AppendLine(command);
            }
            await e.Message.RespondAsync(sb.ToString());
        }
    }
}