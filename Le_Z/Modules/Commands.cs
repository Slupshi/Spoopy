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
		// !wakeup -> **Ta gueule je dors**		
		[Command("wakeup")]
		public Task WakeUpAsync()
		{
			var emoji = new Emoji("🖕🏼");
			Context.Message.AddReactionAsync(emoji);
			return ReplyAsync("**Ta gueule je dors**");
		}
		// !avatar @user [size] -> l'url de l'avatar avec la size choisi
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

		[Command("status")]
		public Task StatusAsync(SocketUser user)
			=>ReplyAsync($"{user.Username} is {user.Status}");

		[Command("clear")]
		public Task ClearAsync(int messageCount)
		{
			throw new NotImplementedException();
		}

		//[Command("spam")]
		//public Task SpamAsync(int spamCount, string msgToSpam)
		//{
		//	for (int i = 0; i < spamCount; i++) ReplyAsync(msgToSpam);
		//	return Task.CompletedTask;
		//}

		//[Command("bully")]
		//public Task TestAsync(SocketUser user)
		//{
		//    return user.SendMessageAsync("Salut pd");
		//}


	}

	
}
