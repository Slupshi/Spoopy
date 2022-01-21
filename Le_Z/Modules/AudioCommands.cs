using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Le_Z.Modules
{
    public class AudioCommands : ModuleBase<SocketCommandContext>
    {	

		// z!connect
		[Command("connect", RunMode = RunMode.Async)]
		public async Task ConnectAsync()
		{
			var user = (SocketGuildUser)Context.User;
			if (user.VoiceChannel == null)
			{
				await ReplyAsync("**Tu dois être dans un channel vocal pour m'invoquer**");
				return;
			}
			var audioClient = await user.VoiceChannel.ConnectAsync(selfDeaf: true);			
			await SendAsync(audioClient, "https://www.youtube.com/watch?v=Ri7GzCUTC5s&ab_channel=KhaledFreak");
		}

		private Process CreateStream(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
		private async Task SendAsync(IAudioClient client, string path)
		{
			// Create FFmpeg using the previous example
			using (var ffmpeg = CreateStream(path))
			using (var output = ffmpeg.StandardOutput.BaseStream)
			using (var discord = client.CreatePCMStream(AudioApplication.Music))
			{
				try
				{
					await output.CopyToAsync(discord);
					await client.SetSpeakingAsync(true);
				}
				finally { await discord.FlushAsync(); }
			}
		}
	}
}
