// FalconsRoost.Program
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using FalconsRoost;
using FalconsRoost.Bots;
using FalconsRoost.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FalconsRoost
{


    internal class Program
    {
        private static GPT3Bot bot;

        private static string versionNumber = "0.0.0.5";

        private static IConfigurationRoot _config;
        private static bool _trace = false;

        private static void Main(string[] args)
        {
            if (args.Any(c => c.ToLower() == "trace"))
            {
                _trace = true;
                Console.WriteLine("Tracing is enabled.");
            }
            if (args.Any((string c) => c.ToLower().StartsWith("dt=")) && args.Any((string c) => c.ToLower().StartsWith("oa=")))
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
            string sqlP = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower().StartsWith("dt="))
                {
                    dt = args[i].Substring(3);
                }
                if (args[i].ToLower().StartsWith("oa="))
                {
                    oa = args[i].Substring(3);
                }
                if (args[i].ToLower().StartsWith("sqlpassword="))
                {
                    sqlP = args[i].Substring("sqlpassword=".Length);
                }
            }
            if (!string.IsNullOrEmpty(dt) && !string.IsNullOrEmpty(oa))
            {
                if (_trace)
                {
                    Console.WriteLine($"It appears we are good to go with the command line arguments. We got {dt} for the discord token and {oa} for the OpenAI key.");
                }

                _config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddInMemoryCollection
                    (
                        new List<KeyValuePair<string, string?>>
                        {
                                new KeyValuePair<string, string?>("DiscordToken", dt),
                                new KeyValuePair<string, string?>("OpenAI", oa),
                                new KeyValuePair<string, string?>("FRDBConnection", $"server=mysql; database=falconsroostdb; user=root; password={sqlP}"),
                                new KeyValuePair<string, string?>("versionNumber", versionNumber),
                                new KeyValuePair<string, string?>("Trace", _trace.ToString())
{

                                }
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
            _config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<Program>()
                .AddInMemoryCollection
                    (
                        new List<KeyValuePair<string, string?>>
                        {
                            new KeyValuePair<string, string?>("VersionNumber", versionNumber),
                            new KeyValuePair<string, string?>("Trace", _trace.ToString())
                        }
                    )
                .Build();
        }

        private static async Task MainAsync()
        {
            DiscordClient discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.GetValue<string>("DiscordToken"),
                TokenType = TokenType.Bot,
                Intents = (DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents)
            });

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<IConfigurationRoot>(_config)
                .BuildServiceProvider();

            Console.WriteLine("I've started up.");
            discord.MessageCreated += async delegate (DiscordClient s, MessageCreateEventArgs e)
            {
                Console.WriteLine("I've caught a message. It says " + e.Message);
            };

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                Services = services,
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<BaseBot>();
            commands.RegisterCommands<GPT3Bot>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }


    }
}