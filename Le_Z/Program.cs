using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Le_Z
{
	public class Program
	{
        private DiscordSocketClient _client;
        private CommandService commands;
        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel= LogSeverity.Debug });
            _client.Log += Log;

            commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("Te pète le fiak");

            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);
            
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
            if (message == null) return;
            if (message.Content.Contains("http")) return;
            if (message.Content.Contains("zeub"))
            {
                var emoji = new Emoji("🍆");
                await message.AddReactionAsync(emoji);
                await message.Channel.SendMessageAsync("https://tenor.com/view/penis-standing-erect-erection-smile-gif-15812844");
                return;
            }
            if (message.Content == "ah" || message.Content=="ahh")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/ah-denis-brognart-tf1-koh-lanta-gif-7256068");
                return;
            }
            //
            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;
            var context = new SocketCommandContext(_client, message);
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
