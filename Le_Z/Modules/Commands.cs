using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Response.RUser;

namespace Le_Z.Modules
{
	// Keep in mind your module **must** be public and inherit ModuleBase.
	// If it isn't, it will not be discovered by AddModulesAsync!
	public class Commands : ModuleBase<SocketCommandContext>
	{
		// z!help
		[Command("help")]
		public Task HelpAsync() 
			=> ReplyAsync("**Tu as vraiment cru que j'allais t'aider ?**");

		//=============================================================================================================================================================//

		// z!say hello world -> **hello world**
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
			Context.Message.DeleteAsync();
			var boldEcho = Format.Bold(echo);
			return ReplyAsync(boldEcho);
		}
		
		//=============================================================================================================================================================//

		// z!wakeup 	
		[Command("wakeup")]
		public Task WakeUpAsync()
		{
			var emoji = new Emoji("🖕🏼");
			Context.Message.AddReactionAsync(emoji);
			return ReplyAsync("**Ta gueule je dors**");
		}

		//=============================================================================================================================================================//

		// z!avatar @user [size]
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

		// z!status @user
		[Command("status")]
		public Task StatusAsync([Remainder] SocketGuildUser user = null)
        {
			if (user == null)			
				user = (SocketGuildUser)Context.User;	
						
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
            				
			string userPlayingStatus = " ";			
			var userStatus = $"{userPresenceStatus}";

			if (user.VoiceChannel != null)
			{
				var userVoicePresenceStatus = $"Se trouve dans \"{user.VoiceChannel}\" dans \"{user.VoiceChannel.Category}\"";				
				userStatus = $"{userStatus}\n{userVoicePresenceStatus}";				
			}

			userStatus = Format.Bold(userStatus);

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
						ReplyAsync(userStatus);
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
						if (richGameInfo.LargeAsset != null) 
							embedGame.WithThumbnailUrl($"{richGameInfo.LargeAsset.GetImageUrl()}");
						ReplyAsync(userStatus);
						Task.Delay(10);
						return ReplyAsync(embed: embedGame.Build());		
					}
				}		
				
				if (userPlaying.Type == ActivityType.Listening)
                {
					if(userPlaying is SpotifyGame spotifyInfo)
                    {												
						var spotifyInfoDuration = (TimeSpan)spotifyInfo.Duration;
						var spotifyInfoElapsed = (TimeSpan)spotifyInfo.Elapsed;
						var embedSpotify = new EmbedBuilder();
						embedSpotify.WithColor(Color.DarkGreen)
							.WithTitle("Ecoute de la musique")
							.WithUrl(spotifyInfo.TrackUrl)
							.WithThumbnailUrl(spotifyInfo.AlbumArtUrl)
							.AddField("Titre :", spotifyInfo.TrackTitle)
							.AddField("Auteur :", spotifyInfo.Artists.First())
							.AddField("Album :", spotifyInfo.AlbumTitle);

                        var elapsed = spotifyInfoElapsed / spotifyInfoDuration;

                        int elalpsedBarLenght = (int)(30 * elapsed);
                        string elapsedBar = "";
                        for (int i = 0; i <= elalpsedBarLenght; i++)
                            elapsedBar += "\u25A0";
                        for (int i = 0; i <= 30 - elalpsedBarLenght; i++)
                            elapsedBar += "\u25A1";
                        
                        embedSpotify.AddField("Durée :", $"{spotifyInfoElapsed.ToString(@"mm\:ss")} | {elapsedBar} | {spotifyInfoDuration.ToString(@"mm\:ss")}");
						ReplyAsync(userStatus);
						Task.Delay(10);
						return ReplyAsync(embed: embedSpotify.Build());
					}					
				}

				if(userPlaying.Type == ActivityType.Streaming)
                {
					if (userPlaying is StreamingGame streamInfo)
                    {
						userPlayingStatus = $"Et est en stream \"{streamInfo.Name}\"\nLien : {streamInfo.Url}";
						userPlayingStatus = Format.Bold(userPlayingStatus);
						userStatus = $"{userStatus}\n{userPlayingStatus}";
						return ReplyAsync($"{userStatus}");
					}	
				}
			}
			return ReplyAsync($"{userStatus}");
		}

		//=============================================================================================================================================================//

		// z!clear [count]
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

		// z!spam count message
		[Command("spam")]
        public Task SpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
			for (int i = 0; i < spamCount; i++) ReplyAsync(msgToSpam);
			return Task.CompletedTask;
        }

		//=============================================================================================================================================================//

		// z!bully @user message
		[Command("bully")]
        public Task BullyAsync(SocketGuildUser user, [Remainder] string msg)
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

		//=============================================================================================================================================================//

		// z!connect
		[Command("connect")]
		public Task ConnectAsync()
		{			
			var user = (SocketGuildUser)Context.User;
			if (user.VoiceChannel == null) return ReplyAsync("**Tu dois être dans un channel vocal pour m'invoquer**");
			user.VoiceChannel.ConnectAsync(selfDeaf: true);
			
			return user.VoiceChannel.DisconnectAsync();			
		}

		//=============================================================================================================================================================//

		// z!test
		[Command("tweetbyid")]
		public async Task TweetByIDAsync(string id)
		{
			var bearerToken = Environment.GetEnvironmentVariable("Bearer_Token_Spoopy", EnvironmentVariableTarget.User);
			TwitterClient twitterClient = new TwitterClient(bearerToken);
			TweetOption[] everyTweetOptions = (TweetOption[])Enum.GetValues(typeof(TweetOption));
			TweetOption[] tweetOption = new TweetOption[2];
			UserOption[] everyUserOptions = (UserOption[])Enum.GetValues(typeof(UserOption));
			UserOption[] userOption = new UserOption[2];
			MediaOption[] everyMediaOptions = (MediaOption[])Enum.GetValues(typeof(MediaOption));
			MediaOption[] mediaOption = new MediaOption[2];			

			tweetOption[0] =everyTweetOptions[1];
			tweetOption[1] = everyTweetOptions[10];			

			userOption[0] = everyUserOptions[5];
			userOption[1] = everyUserOptions[8];

			mediaOption[0] = everyMediaOptions[4];
			mediaOption[1] = everyMediaOptions[3];


			Tweet tweet = await twitterClient.GetTweetAsync(id, tweetOptions: tweetOption, userOptions : userOption, mediaOptions : mediaOption);			
			
			string upperName = tweet.Author.Name.ToUpper();
			string twitterIconUrl = "https://i0.wp.com/fredericgschneider.com/wp-content/uploads/2017/11/twitter-bird-white-on-blue.png?w=405&ssl=1";

			var embedTweet = new EmbedBuilder();
			embedTweet.WithColor(Color.DarkBlue)
				.WithAuthor($"{tweet.Author.Name} @{tweet.Author.Username}", iconUrl: twitterIconUrl, url: "https://twitter.com/"+ tweet.Author.Username)
				.WithTitle($"Nouveau Tweet de {tweet.Author.Name}")
				.WithUrl("https://twitter.com/" + tweet.Author.Username + $"/status/{tweet.Id}")
				.WithThumbnailUrl(tweet.Author.ProfileImageUrl)				
				.WithDescription(tweet.Text)				
				.WithFooter($"{tweet.CreatedAt.Value.TimeOfDay.ToString(@"hh\:mm")} UTC • {tweet.CreatedAt.Value.Date.ToString("dd MMMM, yyyy")} ");
			if (upperName.StartsWith('A') || upperName.StartsWith('E') || upperName.StartsWith('Y') || upperName.StartsWith('U') || upperName.StartsWith('I') || upperName.StartsWith('O') || upperName.StartsWith('H'))
			{
				embedTweet.WithTitle($"Nouveau Tweet d'{tweet.Author.Name}");
			}
			if (tweet.Attachments != null)
            {
				if (tweet.Attachments.Media[0].Url != null)
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].Url);
				if (tweet.Attachments.Media[0].PreviewImageUrl != null)
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].PreviewImageUrl);
			}
			
			


			await ReplyAsync(embed: embedTweet.Build());
						
		}

		//=============================================================================================================================================================//

		[Command("tweet")]
		public async Task LastTweetAsync([Remainder] string username)
		{
			var bearerToken = Environment.GetEnvironmentVariable("Bearer_Token_Spoopy", EnvironmentVariableTarget.User);
			TwitterClient twitterClient = new TwitterClient(bearerToken);
			TweetOption[] everyTweetOptions = (TweetOption[])Enum.GetValues(typeof(TweetOption));
			TweetOption[] tweetOption = new TweetOption[4];
			UserOption[] everyUserOptions = (UserOption[])Enum.GetValues(typeof(UserOption));
			UserOption[] userOption = new UserOption[3];
			MediaOption[] everyMediaOptions = (MediaOption[])Enum.GetValues(typeof(MediaOption));
			MediaOption[] mediaOption = new MediaOption[2];	

			tweetOption[0] = everyTweetOptions[1];
			tweetOption[1] = everyTweetOptions[10];
			tweetOption[2] = everyTweetOptions[3];
			tweetOption[3] = everyTweetOptions[7];

			userOption[0] = everyUserOptions[5];
			userOption[1] = everyUserOptions[8];
			userOption[2] = everyUserOptions[6];

			mediaOption[0] = everyMediaOptions[4];
			mediaOption[1] = everyMediaOptions[3];			

			User user = await twitterClient.GetUserAsync(username);			
			if(user == null)
            {
				await ReplyAsync("**Utilisateur introuvable**");
				return;
			}
            if (user.IsProtected != null)
            {
				if ((bool)user.IsProtected)
				{
					await ReplyAsync("**L'utilisateur restreint l'accès à son compte**");
					return;
				}
			}            
			string upperName = user.Name.ToUpper();

			Tweet[] tweets = await twitterClient.GetTweetsFromUserIdAsync(user.Id, tweetOptions: tweetOption, userOptions: userOption, mediaOptions: mediaOption);
			if(tweets.Length == 0)
            {
				await ReplyAsync("**L'utilisateur n'a aucun tweet disponible**");
				return;
			}
			Tweet tweet = tweets.First(tweetos =>  tweetos.ReferencedTweets == null || tweetos.ReferencedTweets.First().Type == ReferenceType.Quoted);
			
			
			var embedTweet = new EmbedBuilder();
			embedTweet.WithColor(Color.Blue)
				.WithAuthor($"{tweet.Author.Name} @{tweet.Author.Username}")
				.WithTitle($"Dernier Tweet de {tweet.Author.Name}")
				.WithUrl("https://twitter.com/" + tweet.Author.Username + $"/status/{tweet.Id}")
				.WithThumbnailUrl(tweet.Author.ProfileImageUrl)
				.WithDescription(tweet.Text)
				.WithFooter($"{tweet.CreatedAt.Value.TimeOfDay.ToString(@"hh\:mm")} UTC • {tweet.CreatedAt.Value.Date.ToString("dd MMMM, yyyy")} ");
			if (upperName.StartsWith('A') || upperName.StartsWith('E') || upperName.StartsWith('Y') || upperName.StartsWith('U') || upperName.StartsWith('I') || upperName.StartsWith('O') || upperName.StartsWith('H'))
			{
				embedTweet.WithTitle($"Dernier Tweet d'{tweet.Author.Name}");
			}
			if (tweet.Attachments != null)
			{
				if (tweet.Attachments.Media[0].Url != null)
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].Url);
				if (tweet.Attachments.Media[0].PreviewImageUrl != null)
                {
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].PreviewImageUrl);
					if (tweet.Attachments.Media[0].Url!=null)
						embedTweet.AddField("Lien de la vidéo : ", tweet.Attachments.Media[0].Url, inline:true);					
				}
					
			}
			if(tweet.ReferencedTweets != null)
            {
				if(tweet.ReferencedTweets.First().Type == ReferenceType.Quoted)
                {
					ReferencedTweet referencedTweet = tweet.ReferencedTweets.First();
					Tweet quoteTweet = await twitterClient.GetTweetAsync(referencedTweet.Id, tweetOptions: tweetOption, userOptions: userOption, mediaOptions: mediaOption);
					embedTweet.AddField($"Cité de {quoteTweet.Author.Name} @{quoteTweet.Author.Username}", quoteTweet.Text);
					if(quoteTweet.Attachments !=null && tweet.Attachments == null)
                    {
						if (quoteTweet.Attachments.Media[0].Url != null)
							embedTweet.WithImageUrl(quoteTweet.Attachments.Media[0].Url);
						if (quoteTweet.Attachments.Media[0].PreviewImageUrl != null)
							embedTweet.WithImageUrl(quoteTweet.Attachments.Media[0].PreviewImageUrl);
					}
                }
            }
			await ReplyAsync(embed: embedTweet.Build());
		}

		//=============================================================================================================================================================//
		
		public class JoursFeries
        {
			public short Année { get; set; }
			public string Zone { get; set; }
        }
		public class JoursFeriesResponse
        {
			//public JoursFériés();
			public string Date { get; set; }
			public string Nom { get; set; }
		}

		public enum Zone
        {
			alsacemoselle, guadeloupe, guyane, lareunion, martinique, mayotte, metropole, nouvellecaledonie, polynesiefrancaise, saintbarthelemy, saintmartin, saintpierreetmiquelon, wallisetfutuna
        }

		[Command("testapi")]
		public async Task TestAPIAsync()
        {
			HttpClient clientAPI = new HttpClient();
			clientAPI.BaseAddress = new Uri("http://calendrier.api.gouv.fr/jours-feries/");
			clientAPI.DefaultRequestHeaders.Accept.Clear();
			clientAPI.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			var toto = await GetJoursAsync("metropole/2022.json", clientAPI);


			await ReplyAsync(toto.ToString());
			
        }

		static async Task<IEnumerable<JoursFeriesResponse>> GetJoursAsync(string path, HttpClient clientAPI)
		{		
			HttpResponseMessage response = await clientAPI.GetAsync(path);
			response.EnsureSuccessStatusCode();	
			
			var responseText = await response.Content.ReadAsStringAsync();
			var jours = JsonConvert.DeserializeObject<IEnumerable<JoursFeriesResponse>>(responseText);

			foreach(var jour in jours)
            {
				Console.WriteLine(jour);
            }
			//var responseObject = await JsonSerializer.DeserializeAsync<JoursFeriesResponse>(responseStream);

			;
		}






	}	
}
