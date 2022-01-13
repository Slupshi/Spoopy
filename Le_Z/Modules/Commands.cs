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

		//=============================================================================================================================================================//

		// !say hello world -> **hello world**
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
			Context.Message.DeleteAsync();
			var boldEcho = Format.Bold(echo);
			return ReplyAsync(boldEcho);
		}

		//=============================================================================================================================================================//

		// !wakeup 	
		[Command("wakeup")]
		public Task WakeUpAsync()
		{
			var emoji = new Emoji("🖕🏼");
			Context.Message.AddReactionAsync(emoji);
			return ReplyAsync("**Ta gueule je dors**");
		}

		//=============================================================================================================================================================//

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

		//=============================================================================================================================================================//

		// !status @user
		[Command("status")]
		public Task StatusAsync([Remainder] SocketGuildUser user = null)
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
            if (!user.IsBot)
            {
				if (user.ActiveClients.First().ToString() == "Mobile")
				{
					userPresenceStatus = $"{userPresenceStatus} sur son téléphone";
				}
				if (user.ActiveClients.First().ToString() == "Web")
				{
					userPresenceStatus = $"{userPresenceStatus} sur Web, car trop pauvre pour download l'appli";
				}
			}
			userPresenceStatus = Format.Bold(userPresenceStatus);	
			
								
			string userPlayingStatus = " ";			
			var userStatus = $"{userPresenceStatus}";

			if (user.Activities.Count > 0)
            {				
				var userPlaying = user.Activities.First();

                if(userPlaying.Type == ActivityType.CustomStatus && user.Activities.Count > 1)
                {
					userPlaying = user.Activities.ElementAt(1);
                }   
				
				if (userPlaying.Type == ActivityType.Playing)
                {
					var embedGame = new EmbedBuilder();

					if (userPlaying is Game gameInfo && !(userPlaying is RichGame game))
					{
						embedGame.WithTitle($"Joue à \"{gameInfo.Name}\"");		
						ReplyAsync(userPresenceStatus);
						return ReplyAsync(embed: embedGame.Build());
					}
					if (userPlaying is RichGame richGameInfo)
                    {
						var userPlayingTime = (TimeSpan)(DateTime.Now - richGameInfo.Timestamps.Start);
						embedGame.WithTitle($"Joue à {richGameInfo.Name}")
							.WithDescription($"Depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
						if (richGameInfo.Details != null && richGameInfo.State != null)
                        {
							if (richGameInfo.LargeAsset.Text != null)
                            {
								embedGame.AddField(richGameInfo.Details, richGameInfo.LargeAsset.Text)
									 .AddField(richGameInfo.State, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
							}
                            else
                            {
								embedGame.AddField(richGameInfo.Details, ".")
									 .AddField(richGameInfo.State, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
							}															
						}
						if (richGameInfo.SmallAsset != null)
							embedGame.WithThumbnailUrl($"{richGameInfo.SmallAsset.GetImageUrl()}");
						if (richGameInfo.LargeAsset != null) 
							embedGame.WithThumbnailUrl($"{richGameInfo.LargeAsset.GetImageUrl()}");
						ReplyAsync(userPresenceStatus);
						return ReplyAsync(embed: embedGame.Build());		
					}
				}		
				
				if (userPlaying.Type == ActivityType.Listening)
                {
					if(userPlaying is SpotifyGame spotifyInfo)
                    {												
						var spotifyInfoDuration = (TimeSpan)spotifyInfo.Duration;

						var embedSpotify = new EmbedBuilder();										
						embedSpotify.AddField("Titre :", spotifyInfo.TrackTitle)
							.AddField("Auteur :", spotifyInfo.Artists.First())
							.AddField("Album :", spotifyInfo.AlbumTitle)
							.AddField("Durée :", spotifyInfoDuration.ToString(@"mm\:ss"))							
							.WithColor(Color.DarkGreen)
							.WithTitle("Ecoute de la musique")
							.WithUrl(spotifyInfo.TrackUrl)
							.WithThumbnailUrl(spotifyInfo.AlbumArtUrl);
						
						ReplyAsync(userPresenceStatus);
						return ReplyAsync(embed: embedSpotify.Build());
					}					
				}

				if(userPlaying.Type == ActivityType.Streaming)
                {
					if (userPlaying is StreamingGame streamInfo)						
						userPlayingStatus = $"Et est en stream \"{streamInfo.Name}\"\nLien : {streamInfo.Url}";
				}
			}			
			return ReplyAsync($"{userStatus}");
		}

		//=============================================================================================================================================================//

		// !clear [count]
		[Command("clear")]
		public async Task ClearAsync(int messageCount=1)
		{
			int count = 0;
			var msgToDelete = Context.Channel.GetMessagesAsync().TakeLast(messageCount+1);
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

		//=============================================================================================================================================================//

		// !spam count message
		[Command("spam")]
        public Task SpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
			for (int i = 0; i < spamCount; i++) ReplyAsync(msgToSpam);
			return Task.CompletedTask;
        }

		//=============================================================================================================================================================//

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
			ReplyAsync("https://tenor.com/view/zemmour-1825-jvc-menace-rigole-gif-23505307");		
			return user.SendMessageAsync(msg);
        }
    }	
}
