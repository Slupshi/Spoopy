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
		// ~!say hello world -> hello world
		[Command("say")]
		[Summary("Echoes a message.")]
		
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
			Context.Message.DeleteAsync();
			return ReplyAsync(echo);
		}
				
		

		[Command("wakeup")]
		public Task WakeUpAsync()
		{
			var emoji = new Emoji("🖕🏼");
			Context.Message.AddReactionAsync(emoji);
			return ReplyAsync("Ta gueule je dors");
		}
			

		[Command("avatar")]
		public Task GetLargerAvatarAsync(SocketUser user,ushort size = 512)
        {
			if (!(size == 16 || size == 32 || size == 64 || size == 128 || size == 256 || size == 512))
			{
				var emoji = new Emoji("🖕🏼");
				Context.Message.AddReactionAsync(emoji);
				ReplyAsync("Nique ta mère la taille est pas valide");
				return Task.CompletedTask;
			}
			ulong userID = user.Id;
			string avatarID = user.AvatarId;
			return ReplyAsync(CDN.GetUserAvatarUrl(userID, avatarID, size, ImageFormat.Auto));
		}

		[Command("zeub")]
		public Task ZeubAsync()
        {
			throw new NotImplementedException();
        }


	}

	
}
