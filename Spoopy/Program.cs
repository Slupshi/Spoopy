using Discord;
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
using System.Media;
using CliWrap;
using Spoopy.Jobs;
using Spoopy.Variables;

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
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug, GatewayIntents = Constantes.GatewayPrivileges});
            _client.Log += Log;

            _externalInteractions = _services.GetService<ExternalInteractions>();
            _apiService = _services.GetService<ApiService>();
            _localApiService = _services.GetService<LocalApiService>();
            _utilities = _services.GetService<Utilities>();
                
            _commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("/help", type: ActivityType.Watching);
            //await _client.SetStatusAsync(UserStatus.Online);

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
            await Startup.CreateSlashCommands(_client);
            await Startup.CreateContextMenuCommands(_client);

            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.UserCommandExecuted += HandleUserCommandExecuted;
            _client.MessageCommandExecuted += HandleMessageCommandExecuted;
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

            SoundPlayer soundPlayer= new SoundPlayer(@"D:\Video Edited\Effect\Ding.wav"); // Only on my PC
            soundPlayer.Play();

            //await _externalInteractions.ReorderVocalChannel();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }        

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += MessageReceive;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        #region EventHandlers

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
            try
            {
                await Properties.SlashCommandsDico.FirstOrDefault(x => x.Key == command.CommandName).Value.Invoke(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await command.RespondAsync(Format.Bold("Cette commande n'existe pas"), ephemeral: true);
            }
        }

        private async Task HandleUserCommandExecuted(SocketUserCommand arg)
        {
            try
            {
                await Properties.UserCommandsDico.FirstOrDefault(x => x.Key == arg.CommandName).Value.Invoke(arg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await arg.RespondAsync(Format.Bold("Cette commande n'existe pas"), ephemeral: true);
            }
        }

        private async Task HandleMessageCommandExecuted(SocketMessageCommand arg)
        {
            try
            {
                await Properties.MessageCommandsDico.FirstOrDefault(x => x.Key == arg.CommandName).Value.Invoke(arg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await arg.RespondAsync(Format.Bold("Cette commande n'existe pas"), ephemeral: true);
            }
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
            if (Properties.isLocalApiRunning)
            {
                await _externalInteractions.UpdateGameRoleAsync();

                await _localApiService.PostSpoopyStatusAsync();
            }
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
            await _externalInteractions.SetGameRoleAsync(user);
        }

        #endregion

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
        
    }
}
