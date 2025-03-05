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
using FalconsRoost.Models.Alerts;
using FalconsRoost.Models.db;
using FalconsRoost.WebScrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

namespace FalconsRoost
{


    internal class Program : IDesignTimeDbContextFactory<FalconsRoostDBContext>
    {

        private static string versionNumber = "0.0.0.9";

        private static IConfigurationRoot _config;
        private static bool _trace = false;
        private static bool dbEnabled = false;
        private static DiscordClient discord;
        private static FalconsRoostDBContext? db;

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
            ulong adminId = 0;
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
                    dbEnabled = true;
                }
                if (args[i].ToLower().StartsWith("adminId="))
                {
                    var adminIdString = args[i].Substring("adminId=".Length);
                    if (ulong.TryParse(adminIdString, out ulong result))
                    {
                        adminId = result;
                    }
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
                                new KeyValuePair<string, string?>("sqlpassword", sqlP),
                                new KeyValuePair<string, string?>("versionNumber", versionNumber),
                                new KeyValuePair<string, string?>("Trace", _trace.ToString()),
                                new KeyValuePair<string, string?>("connectionString", $"server=mysql;port=3306;database=falconsroostdb;user=root;password={sqlP??string.Empty};"),
                                new KeyValuePair<string, string?>("DiscordAdminId", adminId.ToString())
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

            dbEnabled = !string.IsNullOrWhiteSpace(_config.GetValue<string>("connectionString"));
        }

        private static async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.GetValue<string>("DiscordToken"),
                TokenType = TokenType.Bot,
                Intents = (DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents),

            });
            var connectionString = _config.GetValue<string>("connectionString");

             var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfigurationRoot>(_config);


            
            if (dbEnabled)
            {
                serviceCollection.AddDbContext<FalconsRoostDBContext>(options => options.UseMySQL(connectionString ?? throw new ArgumentException("We could not find an sqlpassword or a connectionString during startup.")).EnableSensitiveDataLogging().EnableDetailedErrors());
            }

            Console.WriteLine("I've started up.");
            discord.MessageCreated += async delegate (DiscordClient s, MessageCreateEventArgs e)
            {
                Console.WriteLine("I've caught a message. It says " + e.Message);
            };

            ServiceProvider services = serviceCollection.BuildServiceProvider();
            db = services.GetRequiredService<FalconsRoostDBContext>();

            if (db == null)
            {
                Console.WriteLine("I couldn't connect to the database.");
                return;
            }

            await db.Database.MigrateAsync();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                Services = services,
                StringPrefixes = new[] { "!" },                
            });

            commands.RegisterCommands<BaseBot>();
            commands.RegisterCommands<GPT3Bot>();
            commands.RegisterCommands<ComicBot>();

            

            await discord.ConnectAsync();

            

            var log = new SimpleLogEntry();
            log.Message = "I've started up.";
            log.Version = versionNumber;
            if (dbEnabled)
            {
                db.LaunchLogs.Add(log);
                await db.SaveChangesAsync();
            }
            

            //execute RunScheduledTasks every minute.
            var timer = new System.Timers.Timer(60000);
            timer.Elapsed += async (sender, e) => await RunScheduledTasks();
            timer.Start();



            await Task.Delay(-1);
        }

        public FalconsRoostDBContext CreateDbContext(string[] args)
        {
            if (args.Any((string c) => c.ToLower().StartsWith("dt=")) && args.Any((string c) => c.ToLower().StartsWith("oa=")))
            {
                MemoryConfigSetUp(args);
            }
            else
            {
                SecretConfigSetUp();
            }

            

            var configurationBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddConfiguration(_config);
            var connectionString = _config.GetValue<string>("connectionString") ?? string.Empty;
            IConfigurationRoot configuration = configurationBuilder.Build();
            DbContextOptionsBuilder<FalconsRoostDBContext> optionsBuilder = new DbContextOptionsBuilder<FalconsRoostDBContext>()
                .UseMySQL(connectionString);

            return new FalconsRoostDBContext(optionsBuilder.Options);


        }

        /// <summary>
        /// This method checks for tasks that need to fire on a regular basis.
        /// It should be called from the MainAsync method using a timer.
        /// </summary>
        public static async Task RunScheduledTasks()
        {
            if (!dbEnabled)
                return;

            var tasks = db.AlertTasks.Include("AlertMessages").Where(t => t.Enabled && t.NextRun < DateTime.Now).ToList();
            if (!tasks.Any())
                return;
            try
            {
                foreach (var task in tasks)
                {
                    //we need to run the task.
                    switch (task.AlertType)
                    {
                        case AlertType.MCSNCBD:
                            if (task.CurrentlyRunning)
                                break;
                            if (task.DayToRunOn != (int)DateTime.Now.DayOfWeek)
                                break;
                            var centralTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            if (task.HourStartTime > DateTime.Now.Hour || task.HourEndTime < DateTime.Now.Hour)
                                //mark the task as running.
                                task.CurrentlyRunning = true;
                            db.Update(task);
                            await db.SaveChangesAsync();
                            var mcsNCBDScraper = new MyComicShopScraper(_config, db);
                            var result = await mcsNCBDScraper.NCBDCheck(task);
                            task.CurrentlyRunning = false;
                            db.Update(task);
                            break;
                        case AlertType.MCSRatio:
                            var mscRatioScraper = new MyComicShopScraper(_config, db);
                            break;
                        default:
                            var simpleLog = new SimpleLogEntry();
                            simpleLog.Message = $"I don't know what to do with this task. It's AlertType is {task.AlertType}";
                            db.LaunchLogs.Add(simpleLog);
                            break;
                    }
                    task.LastRun = DateTime.Now;
                    db.AlertTasks.Update(task);
                }
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var simpleLog = new SimpleLogEntry();
                simpleLog.Message = $"I had an error running a scheduled task. {ex.Message}";
                db.LaunchLogs.Add(simpleLog);
                await db.SaveChangesAsync();
            }
        }
    }
}