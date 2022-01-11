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
	// Create a module with no prefix
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
			var userStatus = user.Status switch
			{
				UserStatus.Online => $"{user.Nickname} est en ligne",
				UserStatus.Offline => $"{user.Nickname} est hors ligne",
				UserStatus.Invisible => $"{user.Nickname} se cache comme une pute",
				UserStatus.DoNotDisturb => $"{user.Nickname} coule un bronze",
				UserStatus.Idle => $"{user.Nickname} est entrain de pisser",
				_ => "Nan franchement même moi je sais pas"
			};
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
			ReplyAsync($"**{user.Nickname} est entrain de se faire bully**");
			return user.SendMessageAsync(msg);
        }


    }

	
}
