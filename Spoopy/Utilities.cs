using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spoopy.Services;
using Spoopy.Models;
using Discord.WebSocket;
using System.Diagnostics;
using Discord;

namespace Spoopy
{
    public class Utilities
    {
        private static ApiService _apiService;
        private static LocalApiService _localApiService;
        private static DiscordSocketClient _client;

        public Utilities(ApiService apiService, LocalApiService localApiService,DiscordSocketClient client)
        {
            _apiService = apiService;
            _localApiService = localApiService;
            _client = client;
        }

        public static async Task SpoopyLogAsync(string message, bool isError = false, bool isLogger = false)
        {
            await Properties.BotLogChannel.SendMessageAsync(Utilities.FormatToCode($"{(isError ? "fix" : string.Empty)}{Environment.NewLine}{DateTime.Now.ToString("T")} | {message}"));
            if (!isLogger)
            {
                await _localApiService.PostSpoopyLogsAsync(new SpoopyLogs(message, isError, DateTime.Now));
            }
        }

        public static Action<MessageProperties> RespondToSlashCommandErrorAsync()
        {
            var button = new ButtonBuilder();
            button.WithCustomId($"errorContactButton")
                .WithLabel("Contacter le dev !")
                .WithStyle(ButtonStyle.Primary)
                .WithEmote(new Emoji("✏"));

            var msgComponents = new ComponentBuilder().WithButton(button);

            return (MessageProperties msg) => 
            {
                msg.Content = Format.Bold("Une erreur s'est produite avec l'éxécution de cette commande");
                msg.Components = msgComponents.Build();
            };
        }

        public static DiscordSocketClient GetClient() => _client;

        public static string FormatToCode(string message) => $"```{message}```";

        public static string GetCustomTimestamp() => $"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ";

        public static string DeleteUrlFromText(string text)
        {
            List<string> words = text.Split("http").ToList();
            words.RemoveAll(i => i.Contains("://"));
            string textToReturn = string.Join(" ", words);
            return textToReturn;
        }

        public static async Task<int> GetParisTimeZoneAsync()
        {            
            var timeZone = await _apiService.HttpGetAsync<TimeZoneModel>("http://worldtimeapi.org/api/timezone/Europe/Paris") as TimeZoneModel;
            if(timeZone.UTC_offset == "+01:00") return 1;
            else return 2;
        }

        public static string DeFactory(string nextWord, bool isUpperCase = false)
        {
            string response = isUpperCase ? "De " : "de ";
            char firstLetter = char.ToUpperInvariant(nextWord.First());
            if(Properties.Vowels.Any(x=> x == firstLetter))
            {
                if(!(firstLetter == 'Y' && Properties.Vowels.Any(x => x == char.ToUpperInvariant(nextWord.ElementAt(1)))))
                {
                    response = isUpperCase ? "D'" : "d'";
                }
            }
            return response;
        }

        public static async Task<string> ScrapHtmlAsync(string url)
        {
            return await _apiService.ScrapHtmlPageAsync(url);
        }

        public static SpoopyStatus GetSpoopyStatus()
        {
            bool isConnected = _client.ConnectionState == ConnectionState.Connected;
            return  new SpoopyStatus(
                    uptime: isConnected ? Program.UptimeTimer.Elapsed : TimeSpan.Zero,
                    runtime: DateTime.Now - Process.GetCurrentProcess().StartTime,
                    isConnected: isConnected,
                    serverCount: _client.Guilds.Count
                );
        }

        public static async Task<SpoopyInfos> GetSpoopyInfos() 
        {
            var infos = await _client.GetApplicationInfoAsync();
            return new SpoopyInfos(name: infos.Name,
                                   avatarUrl: infos.IconUrl,
                                   createdAt: infos.CreatedAt.DateTime,
                                   owner: infos.Owner);         
        }
    }
}
