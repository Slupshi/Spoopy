using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        {
			Random random = new Random();
			double rand = random.NextDouble();
			string message = rand switch
			{
				< 0.25 => "Tu as vraiment cru que j'allais t'aider ?",
				< 0.5 => "Qu'espérais-tu ? De l'aide peut-être ?",
				< 0.75 => "C'est beau de rêver",
				_ => "Encore un pigeon tombé dans le panneau"
			};
			message = Format.Bold(message);
			return ReplyAsync(message);
		}
			

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
							.WithDescription($"Depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}")
							.WithFooter($"{DateTime.Now.ToString(@"hh\:mm")} UTC • {DateTime.Now.ToString("dd MMMM, yyyy")} ", iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png");
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
							.AddField("Album :", spotifyInfo.AlbumTitle)
							.WithFooter($"{DateTime.Now.ToString(@"hh\:mm")} UTC • {DateTime.Now.ToString("dd MMMM, yyyy")} ", iconUrl: "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png");						
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

			CultureInfo culture = new CultureInfo("fr-FR");
			var embedTweet = new EmbedBuilder();
			embedTweet.WithColor(Color.Blue)
				.WithAuthor($"{tweet.Author.Name} @{tweet.Author.Username}")
				.WithTitle($"Dernier Tweet de {tweet.Author.Name}")
				.WithUrl("https://twitter.com/" + tweet.Author.Username + $"/status/{tweet.Id}")
				.WithThumbnailUrl(tweet.Author.ProfileImageUrl)
				.WithDescription(tweet.Text)
				.WithFooter($"Publié à {tweet.CreatedAt.Value.TimeOfDay.ToString(@"hh\:mm")} (UTC) le {tweet.CreatedAt.Value.Date.ToString("dd MMMM, yyyy", culture)} ", iconUrl: "https://www.freepnglogos.com/uploads/twitter-logo-png/twitter-icon-circle-png-logo-8.png");
			if (upperName.StartsWith('A') || upperName.StartsWith('E') || upperName.StartsWith('Y') || upperName.StartsWith('U') || upperName.StartsWith('I') || upperName.StartsWith('O') || upperName.StartsWith('H'))
			{
				embedTweet.WithTitle($"Dernier Tweet d'{tweet.Author.Name}");
			}
			if (tweet.Attachments != null)
			{
				if (tweet.Attachments.Media[0].Url != null)
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].Url);
				if (tweet.Attachments.Media[0].PreviewImageUrl != null)                
					embedTweet.WithImageUrl(tweet.Attachments.Media[0].PreviewImageUrl);										
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
			var lastMessageBot = await ReplyAsync(embed: embedTweet.Build());			
		}

		//=============================================================================================================================================================//		

		[Command("repos")]		
		public async Task ReposMainAsync(string codePostal = null , bool twoYear = false)
		{			
			if(codePostal == null)
            {
				await ReplyAsync("**Précise un code postal**");
				return;
			}
				
			int annee = DateTime.Now.Year;
			string zone = GetZone(codePostal);
			JoursFeriesModel joursFeriesModel = new JoursFeriesModel { Année = annee, Zone = zone };
			Embed[] embeds = await ReposAsync(joursFeriesModel, codePostal);
			await ReplyAsync(embed: embeds[0]);
			if(twoYear)
				await ReplyAsync(embed: embeds[1]);
		}

		public class JoursFeriesModel
		{
			public int Année { get; set; }
			public string Zone { get; set; }
		}
		public class JoursFeriesResponseModel
		{
			public DateTime Date { get; set; }
			public string Nom { get; set; }
		}

		public static string GetZone(string codePostal)
		{
			char[] codePostalSplit = codePostal.ToCharArray();
			string codePostalStart = $"{codePostalSplit[0]}{codePostalSplit[1]}";
			string codePostalStartCOM = $"{codePostalSplit[0]}{codePostalSplit[1]}{codePostalSplit[2]}";


			string zone = null;
			if (codePostalStart == "57" || codePostalStart == "67" || codePostalStart == "68") zone = "alsace-moselle";
			else if (codePostalStart == "97")
			{
				if (codePostalStartCOM == "971") zone = "guadeloupe";
				else if (codePostalStartCOM == "972") zone = "martinique";
				else if (codePostalStartCOM == "973") zone = "guyane";
				else if (codePostalStartCOM == "974") zone = "la -reunion";
				else if (codePostalStartCOM == "975") zone = "saint -pierre-et-miquelon";
				else if (codePostalStartCOM == "976") zone = "mayotte";
				else if (codePostalStartCOM == "977") zone = "saint -barthelemy";
				else if (codePostalStartCOM == "978") zone = "saint -martin";
			}
			else if (codePostalStart == "98")
			{
				if (codePostalStartCOM == "986") zone = "wallis -et-futuna";
				else if (codePostalStartCOM == "987") zone = "polynesie -francaise";
				else if (codePostalStartCOM == "988") zone = "nouvelle -caledonie";
			}
			else zone = "metropole";

			return zone;
		}
		public static async Task<Embed[]> ReposAsync(JoursFeriesModel joursFeries, string codePostal)
		{
			HttpClient clientAPI = new HttpClient();
			clientAPI.BaseAddress = new Uri("http://calendrier.api.gouv.fr/jours-feries/");

			var jours = await GetJoursFeriesAsync($"{joursFeries.Zone}/{joursFeries.Année}.json", clientAPI);
			var joursn1 = await GetJoursFeriesAsync($"{joursFeries.Zone}/{joursFeries.Année + 1}.json", clientAPI);

			List<JoursFeriesResponseModel> joursFeriesCurrentYear = new List<JoursFeriesResponseModel>();
			List<JoursFeriesResponseModel> joursFeriesNextYear = new List<JoursFeriesResponseModel>();

			foreach (var jour in jours)
			{
				JoursFeriesResponseModel jourFerie = new JoursFeriesResponseModel { Date = DateTime.Parse(jour.Key), Nom = jour.Value };
				joursFeriesCurrentYear.Add(jourFerie);
			}
			foreach (var jour in joursn1)
			{
				JoursFeriesResponseModel jourFerie = new JoursFeriesResponseModel { Date = DateTime.Parse(jour.Key), Nom = jour.Value };
				joursFeriesNextYear.Add(jourFerie);
			}
			var embedJourFerie1 = new EmbedBuilder();
			embedJourFerie1.WithTitle($"Jours fériés de l'année {joursFeries.Année} pour le code postal {codePostal} :");
			var embedJourFerie2 = new EmbedBuilder();
			embedJourFerie2.WithTitle($"Jours fériés de l'année {joursFeries.Année+1} pour le code postal {codePostal} :");

            foreach (var jour in joursFeriesCurrentYear)
            {
				embedJourFerie1.AddField($"{jour.Date.ToString("dd MMMM yyyy")} :", $"{jour.Nom}");
			}
            
            foreach (var jour in joursFeriesNextYear)
            {
				embedJourFerie2.AddField($"{jour.Date.ToString("dd MMMM yyyy")} :", $"{jour.Nom}");
			}
			Embed[] embeds = new Embed[2];
			embeds[0] = embedJourFerie1.Build();
			embeds[1] = embedJourFerie2.Build();
			return embeds;			
		}
		public static async Task<Dictionary<string, string>> GetJoursFeriesAsync(string path, HttpClient clientAPI)
		{
			HttpResponseMessage response = await clientAPI.GetAsync(path);
			response.EnsureSuccessStatusCode();

			var responseText = await response.Content.ReadAsStringAsync();
			var jours = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

			return jours;
		}
























	}	
}
