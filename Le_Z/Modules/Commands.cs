using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Le_Z.Modules
{
	// Keep in mind your module **must** be public and inherit ModuleBase.
	// If it isn't, it will not be discovered by AddModulesAsync!
	public class Commands : ModuleBase<SocketCommandContext>
	{
		//!help
		[Command("help")]
		public Task HelpAsync() 
			=> ReplyAsync("**Tu as vraiment cru que j'allais t'aider ?**");

		// !say hello world -> **hello world**
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
			Context.Message.DeleteAsync();
			var boldEcho = Format.Bold(echo);
			return ReplyAsync(boldEcho);
		}

		// !wakeup 	
		[Command("wakeup")]
		public Task WakeUpAsync()
		{
			var emoji = new Emoji("🖕🏼");
			Context.Message.AddReactionAsync(emoji);
			return ReplyAsync("**Ta gueule je dors**");
		}

		// !avatar @user [size]
		[Command("avatar")]
		public Task GetAndResizeAvatarAsync(SocketUser user,ushort size = 512)
        {
			if (!(size == 16 || size == 32 || size == 64 || size == 128 || size == 256 || size == 512))
			{
				var emoji = new Emoji("🖕🏼");
				Context.Message.AddReactionAsync(emoji);
				return ReplyAsync("Nique ta mère la taille est pas valide");
			}
			ulong userID = user.Id;
			string avatarID = user.AvatarId;
			return ReplyAsync(CDN.GetUserAvatarUrl(userID, avatarID, size, ImageFormat.Auto));
		}

		// !status @user
		[Command("status")]
		public Task StatusAsync(SocketGuildUser user = null)
        {
			if (user == null) return ReplyAsync("**Précise le nom de quelqu'un**");
			string userName = (user.Nickname == null) switch
			{
				false => user.Nickname,
				true => user.Username,
			};
			string userPresenceStatus = user.Status switch
			{
				UserStatus.Online => $"{userName} est en ligne",
				UserStatus.Offline => $"{userName} est hors ligne",
				UserStatus.Invisible => $"{userName} se cache comme une pute",
				UserStatus.DoNotDisturb => $"{userName} coule un bronze",
				UserStatus.Idle => $"{userName} est entrain de pisser",
				_ => "Nan franchement même moi je sais pas"
			};
			bool isPlaying = false;
			bool isOnSpotify = false;
			//bool isStreaming = false;
			string userPlayingStatus = " ";
			string userListeningStatus = " ";
			var userStatus = $"{userPresenceStatus}";
			if (user.Activities.Count > 0)
            {
				isPlaying = true;
				var userPlaying = user.Activities.First();
				userPlayingStatus = $"Joue à \"{userPlaying.Name}\"";
				if (userPlaying.Type == ActivityType.Listening)
                {
					isOnSpotify = true;
					userPlayingStatus = "Ecoute de la musique";
					userListeningStatus = $"{userPlaying}";
					string[] spotifyInfo = userListeningStatus.Split(' ');
					userListeningStatus = $"\n	Artiste : {spotifyInfo[0]}\n	Titre : {spotifyInfo[2]} ";
				}
				if(userPlaying.Type == ActivityType.Streaming)
                {
					//isStreaming = true;
					userPlayingStatus = "Est en Stream";

				}
			}
            if (isPlaying && !isOnSpotify)
            {
				userStatus = $"{userPresenceStatus}\n{userPlayingStatus}";
			}
			if (isOnSpotify)
            {
				userStatus = $"{userPresenceStatus}\n{userPlayingStatus} || {userListeningStatus}";
			}
				


			userStatus = Format.Bold(userStatus);
			return ReplyAsync($"{userStatus}");
		}
			
		// !clear [count]
		[Command("clear")]
		public async Task ClearAsync(int messageCount=1)
		{
			int count = 0;
			var msgToDelete = Context.Channel.GetMessagesAsync().TakeLast(messageCount);
            await foreach (var msg in msgToDelete)
            {
				foreach(var msgBis in msg)
                {
					if (count == messageCount) return;
					await msgBis.DeleteAsync();
					count++;
				}
			}
		} 	

		// !spam count message
        [Command("spam")]
        public Task SpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
			for (int i = 0; i < spamCount; i++) ReplyAsync(msgToSpam);
			return Task.CompletedTask;
        }

		// !bully @user message
        [Command("bully")]
        public Task TestAsync(SocketGuildUser user, [Remainder] string msg)
        {
			Context.Message.DeleteAsync();
			user.CreateDMChannelAsync();
			msg = Format.Bold(msg);
			string userName = (user.Nickname == null) switch
			{
				false => user.Nickname,
				true => user.Username,
			};
			ReplyAsync($"**{userName} est entrain de se faire bully**");
			return user.SendMessageAsync(msg);
        }


    }

	
}
