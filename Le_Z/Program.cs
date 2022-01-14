using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Client;

namespace Le_Z
{
	public class Program
	{
        static HttpClient APIclient = new HttpClient();
        private DiscordSocketClient _client;
        private CommandService commands;
        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        

        public async Task RunBotAsync()
        {           
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel= LogSeverity.Debug, GatewayIntents = GatewayIntents.All });
            _client.Log += Log;

            commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("Te pète le fiak");

           
            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);
            



            //Starting the bot
            await _client.StartAsync();
            await Task.Delay(-1);

            
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = (SocketUserMessage)msg;
            #region TriggerWords
            if (message == null) return;
            if (message.Content.Contains("http")) return;            
            if (message.Content.ToLower() == "ah" || message.Content.ToLower() == "ahh")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/ah-denis-brognart-tf1-koh-lanta-gif-7256068");
                return;
            }
            if (message.Content.ToLower() == "ils sont là" || message.Content.ToLower() == "ils sont la")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/marine-le-pen-le-pen-gif-8538154");
                return;
            }
            if (message.Content.ToLower() == "salut mon pote")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/salut-mon-pote-hi-buddy-michel-drucker-gif-16070000");
                return;
            }
            if (message.Content.ToLower().Contains("zeub") || message.Content.ToLower().Contains("zob"))
            {
                var emoji = new Emoji("🍆");
                await message.AddReactionAsync(emoji);
                await message.Channel.SendMessageAsync("https://tenor.com/view/penis-standing-erect-erection-smile-gif-15812844");

            }
            if (message.Content.ToUpper().Contains("DEMARRER") || message.Content.ToUpper().Contains("DEMARRE"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/je-vais-le-d%C3%A9marrer-cet-gif-19154207");
                
            }
            if (message.Content.ToUpper().Contains("PHILIPPE"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/philippe-cobra-hitman-gif-13748655");
                
            }
            if (message.Content.ToUpper().Contains("ENORME"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/%C3%A9norme-jamy-pas-sorcier-huge-gif-14277967");

            }
            if (message.Content.ToUpper().Contains("PARCOUR")|| message.Content.ToUpper().Contains("PARCOURS") || message.Content.ToUpper().Contains("PARKOUR")|| message.Content.ToUpper().Contains("PARKOURS"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/parkour-theoffice-freerunning-gif-5128248");

            }
            #endregion TriggerWords
            int argPos = 0;
            if (!message.HasStringPrefix("z!", ref argPos)) return;
            var context = new SocketCommandContext(_client, message);
            await context.Guild.DownloadUsersAsync();
            var result = await commands.ExecuteAsync(context, argPos, null);
            if (!result.IsSuccess)
            {
                var emoji1 = new Emoji("🖕🏼");
                await message.AddReactionAsync(emoji1);
                await context.Channel.SendMessageAsync("Je connais pas ta commande batard");
            }

        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}


    }
}
