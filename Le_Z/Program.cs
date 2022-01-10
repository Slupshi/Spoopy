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
            
            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;
            var context = new SocketCommandContext(_client, message);
            var result = await commands.ExecuteAsync(context, argPos, null);
            if (!result.IsSuccess) await context.Channel.SendMessageAsync(result.ErrorReason);

        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}


	}
}
