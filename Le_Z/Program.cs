using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
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
                     
            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);
            
            await _client.StartAsync();
            await Task.Delay(-1);


            _client.Log += Log;
                    
        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}


	}
}
