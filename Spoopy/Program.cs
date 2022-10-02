﻿using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Spoopy.Modules;
using Spoopy.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

namespace Spoopy
{
    public class Program
    {     
        private static DiscordSocketClient _client;
        private static IServiceProvider _services;
        private ExternalInteractions _externalInteractions;
        private CommandService _commands;
        private ApiService _apiService;
        private static LocalApiService _localApiService;
        private Utilities _utilities;
        private Timer _activitiesTimer;
        public static Stopwatch UptimeTimer;

        public static void Main(string[] args)
        {
            _services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<AudioService>()
                .AddSingleton<TwitterService>()
                .AddSingleton<ExternalInteractions>()
                .AddSingleton<ApiService>()
                .AddSingleton<LocalApiService>()
                .AddSingleton<Utilities>()
                .BuildServiceProvider();
            
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug, GatewayIntents =  Properties.GatewayPrivleges});
            _client.Log += Log;

            _externalInteractions = _services.GetService<ExternalInteractions>();
            _apiService = _services.GetService<ApiService>();
            _localApiService = _services.GetService<LocalApiService>();
            _utilities = new Utilities(_apiService, _client);
                
            _commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("/help", type: ActivityType.Watching);
            await _client.SetStatusAsync(UserStatus.Online);

            _client.Ready += _client_Ready;

            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);

            UptimeTimer = new Stopwatch();
            UptimeTimer.Start();

            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task _client_Ready()
        {
            Properties.SetPropertiesAtStartup(_client);
            await CreateSlashCommands();

            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.UserJoined += UserJoined;
            _client.LatencyUpdated += LatencyUpdated;
            _client.ButtonExecuted += ButtonExecuted;
            _client.ModalSubmitted += ModalSubmitted;
            _client.PresenceUpdated += PresenceUpdated;

            _activitiesTimer = new Timer();
            _activitiesTimer.Interval = 20000;
            _activitiesTimer.Elapsed += _activitiesTimer_Elapsed;
            _activitiesTimer.Start();

            //await _externalInteractions.ReorderVocalChannel();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static async Task ZLog(string message, bool isError = false, bool isLogger = false)
        {
            await Properties.BotLogChannel.SendMessageAsync(Utilities.FormatToCode($"{(isError ? "fix" : string.Empty)}{Environment.NewLine}{DateTime.Now.ToString("T")} | {message}"));
            if (!isLogger)
            {
                await _localApiService.PostSpoopyLogsAsync(new Models.SpoopyLogs(message, isError, DateTime.Now));
            }
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += MessageReceive;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task MessageReceive(SocketMessage msg)
        {
            if (msg.Author.IsBot)
            {
                return;
            }
            var message = (SocketUserMessage)msg;
            if (message == null) return;

            if (message.Channel.Name.Contains('@'))
            {
                await _externalInteractions.CheckDMsAsync(message);
                return;
            }
            if (message.Channel == Properties.YoutubeVideoChannel && message.Content.Contains("http"))
            {
                await _externalInteractions.HandleNewYoutubeVideoAsync(message);
                return;
            }
            if (message.Content.Contains("https://twitter.com/") && message.Content.Contains("/status/"))
            {
                await _externalInteractions.HandleNewTweetSent(message);
                return;
            }

            await HandleCommandAsync(message);
        }

        private async Task HandleCommandAsync(SocketUserMessage message)
        {
            int argPos = 0;
            if (!message.HasStringPrefix("z!", ref argPos)) return;
            var context = new SocketCommandContext(_client, message);
            await context.Guild.DownloadUsersAsync();
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                MessageReference messageRef = new MessageReference(messageId: message.Id);
                await context.Channel.SendMessageAsync(Format.Bold("Je ne reconnais pas cette commande"), messageReference: messageRef);
            }

        }

        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            await Properties.SlashCommandsDico.FirstOrDefault(x => x.Key == command.CommandName).Value.Invoke(command);
        }

        private async Task ButtonExecuted(SocketMessageComponent arg)
        {
            await Properties.ButtonHandlersDico.FirstOrDefault(x => arg.Data.CustomId.Contains(x.Key)).Value.Invoke(arg);
        }

        private async Task ModalSubmitted(SocketModal arg)
        {
            await Properties.ModalHandlersDico.FirstOrDefault(x => arg.Data.CustomId.Contains(x.Key)).Value.Invoke(arg);
        }

        private async Task LatencyUpdated(int previousLatency, int newLatency)
        {
            await _externalInteractions.SetGameRoleAsync();
            
            await _localApiService.PostSpoopyStatusAsync(Utilities.GetSpoopyStatus());
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousVoiceState, SocketVoiceState newVoiceState)
        {
            await _externalInteractions.UwUAsync(user, newVoiceState);
        }

        private async Task UserJoined(SocketGuildUser user)
        {            
            await _externalInteractions.SetRoleOnGuildJoined(user);            
        }

        private async Task PresenceUpdated(SocketUser user, SocketPresence pastPresence, SocketPresence newPresence)
        {
            
        }


        private async void _activitiesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Properties.BotActivities.Count == 0)
            {
                Properties.SetupBotActivities();
                await _externalInteractions.GetNumberFact();
            }
            string activity = Properties.BotActivities.Dequeue();
            await _client.SetGameAsync(activity, type: ActivityType.Watching);            
        }

        private async Task CreateSlashCommands()
        {
            // Test
            var testCommand = new SlashCommandBuilder();
            testCommand.WithName("test");
            testCommand.WithDescription("A ne pas utiliser");
            testCommand.WithDefaultMemberPermissions(GuildPermission.Administrator);

            // Help
            var helpCommand = new SlashCommandBuilder();
            helpCommand.WithName("help");
            helpCommand.WithDescription("De l'aide pour les gens perdus !");

            // Status
            var statusCommand = new SlashCommandBuilder();
            statusCommand.WithName("status");
            statusCommand.WithDescription("Pour stalk les gens du serveur");
            statusCommand.AddOption("username", ApplicationCommandOptionType.User, "La personne stalkée (Vous même par défaut)", isRequired: false);

            // Avatar
            var avatarCommand = new SlashCommandBuilder();
            avatarCommand.WithName("avatar");
            avatarCommand.WithDescription("Récupère l'avatar d'un membre du serveur");
            avatarCommand.AddOption("username", ApplicationCommandOptionType.User, "La personne dont l'avatar sera prélevé (Vous même par défaut)", isRequired: false);
            avatarCommand.AddOption(name: "size",
               type: ApplicationCommandOptionType.Integer,
               description: "La taille de l'image en pixel (512x512 par défaut)",
               isRequired: false,
               choices: new ApplicationCommandOptionChoiceProperties[]
               {
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "2048x2048",
                       Value = 2048,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "1024x1024",
                       Value = 1024,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "512x512",
                       Value = 512,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "256x256",
                       Value = 256,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "128x128",
                       Value = 128,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "64x64",
                       Value = 64,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "32x32",
                       Value = 32,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "16x16",
                       Value = 16,
                   },
               });

            // RandomOrg
            var randomCommand = new SlashCommandBuilder();
            randomCommand.WithName("random");
            randomCommand.WithDescription("Selectionne un nombre random entre le nombre de départ et de fin");
            randomCommand.AddOption("départ", ApplicationCommandOptionType.Integer, "Le nombre de départ | 1 par défaut", isRequired: false, minValue: 1);
            randomCommand.AddOption("fin", ApplicationCommandOptionType.Integer, "Le nombre de fin", isRequired: true, minValue: 2);
            randomCommand.AddOption("visible", ApplicationCommandOptionType.Boolean, "Détermine si la réponse est visible par tout le monde", isRequired: false);

            // SpoopyStatus
            var spoopyStatus = new SlashCommandBuilder();
            spoopyStatus.WithName("spoopy");
            spoopyStatus.WithDescription("Obtenir le status du bot");

            // Shuffle
            var shuffleCommand = new SlashCommandBuilder();
            shuffleCommand.WithName("shuffle");
            shuffleCommand.WithDescription("Mélange les lettres d'une chaine de charactères");
            shuffleCommand.AddOption("entree", ApplicationCommandOptionType.String, "La chaîne de charactères qui sera mélanger", isRequired: true);
            shuffleCommand.AddOption("visible", ApplicationCommandOptionType.Boolean, "Détermine si la réponse est visible par tout le monde", isRequired: false);

            // Basic Poll
            var pollCommand = new SlashCommandBuilder();
            pollCommand.WithName("sondage");
            pollCommand.WithDescription("Création de sondage");
            pollCommand.AddOption("question", ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true);
            pollCommand.AddOption("everyone", ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué (False par défaut)", isRequired: false);
            pollCommand.AddOption("persistant", ApplicationCommandOptionType.Boolean, "Défini si un sondage est infini ou non (False par défaut)", isRequired: false);

            // Complex Poll
            var complexPollCommand = new SlashCommandBuilder();
            complexPollCommand.WithName("poll");
            complexPollCommand.WithDescription("Création de sondage à choix multiples");
            complexPollCommand.AddOption("question", ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true)
                .AddOption("everyone", ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué", isRequired: false)
                .AddOption("persistant", ApplicationCommandOptionType.Boolean, "Défini si un sondage est infini ou non", isRequired: false)
                .AddOption("proposition1", ApplicationCommandOptionType.String, "Proposition n°1", isRequired: true)
                .AddOption("proposition2", ApplicationCommandOptionType.String, "Proposition n°2", isRequired: true)
                .AddOption("proposition3", ApplicationCommandOptionType.String, "Proposition n°3", isRequired: false)
                .AddOption("proposition4", ApplicationCommandOptionType.String, "Proposition n°4", isRequired: false)
                .AddOption("proposition5", ApplicationCommandOptionType.String, "Proposition n°5", isRequired: false)
                .AddOption("proposition6", ApplicationCommandOptionType.String, "Proposition n°6", isRequired: false)
                .AddOption("proposition7", ApplicationCommandOptionType.String, "Proposition n°7", isRequired: false)
                .AddOption("proposition8", ApplicationCommandOptionType.String, "Proposition n°8", isRequired: false)
                .AddOption("proposition9", ApplicationCommandOptionType.String, "Proposition n°9", isRequired: false);

            // FakeBan
            var fakeBanCommand = new SlashCommandBuilder();
            fakeBanCommand.WithName("ban");
            fakeBanCommand.WithDescription("Ban une personne du serveur");
            fakeBanCommand.AddOption("username", ApplicationCommandOptionType.User, "La personne qui sera ban", isRequired: false);

            var gameRoleCommand = new SlashCommandBuilder();
            gameRoleCommand.WithName("game");
            gameRoleCommand.WithDescription("Obtiens le role du jeu qui correspondant à ce channel !");

            try
            {
                await _client.CreateGlobalApplicationCommandAsync(helpCommand.Build());
                await _client.CreateGlobalApplicationCommandAsync(statusCommand.Build());
                await _client.CreateGlobalApplicationCommandAsync(avatarCommand.Build());
                await _client.CreateGlobalApplicationCommandAsync(randomCommand.Build());
                await _client.CreateGlobalApplicationCommandAsync(spoopyStatus.Build());
                await _client.CreateGlobalApplicationCommandAsync(shuffleCommand.Build());

                await Properties.Banquise.CreateApplicationCommandAsync(pollCommand.Build());
                await Properties.Banquise.CreateApplicationCommandAsync(complexPollCommand.Build());
                await Properties.Banquise.CreateApplicationCommandAsync(fakeBanCommand.Build());

                await Properties.TeykhoServer.CreateApplicationCommandAsync(gameRoleCommand.Build());

                await Properties.TestServer.CreateApplicationCommandAsync(testCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }
        }       

    }
}
