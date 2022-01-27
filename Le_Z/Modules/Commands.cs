using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namotion.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Response.RUser;

namespace Le_Z.Modules
{
    /// <summary>
    /// Text Commands 
    /// </summary>
    public class Commands : ModuleBase<SocketCommandContext>
    {
        readonly CultureInfo Culture = new CultureInfo("fr-FR");
        readonly Random Random = new Random();

        #region Help     	

        // z!help
        [Command("help")]
        public Task HelpAsync()
        {
            double rand = Random.NextDouble();
            string message = rand switch
            {
                < 0.25 => "Tu as vraiment cru que j'allais t'aider ?",
                < 0.5 => "Voici de l'aide jeune Padawan",
                < 0.75 => "C'est beau de rêver",
                _ => "Encore un pigeon tombé dans le panneau"
            };
#if DEBUG
            if (Context.User.Id == 429352632138858506)
                message = "Voici de l'aide jeune Padawan";
#endif
            if (message == "Voici de l'aide jeune Padawan")
            {
                var botUser = Context.Client.CurrentUser;
                var embedHelp = new EmbedBuilder();
                embedHelp.WithTitle(Format.Underline(message))
                         .WithColor(Color.DarkBlue)
                         .WithDescription("Chaque commande doit être éxécutée avec le préfix \"**z!**\"\n Les variables avec des '{' sont obligatoires, celles avec des '[' sont optionnelles")
                         .WithFooter("Surement pas à jour", iconUrl: "https://e7.pngegg.com/pngimages/359/338/png-clipart-logo-information-library-business-information-miscellaneous-blue-thumbnail.png")
                         .AddField(name: "`avatar {Pseudo discord ou @} [taille]`", "Renvoie l'avatar d'un user à la taille choisie", inline: false)
                         .AddField(name: "`bully {Pseudo discord ou @} {Ton Message}`", "Envoie un message en mp à un user", inline: false)
                         .AddField(name: "`clear [Nombre à delete]`", "Supprime des messages dans le channel où est exécutée la commande", inline: false)
                         .AddField(name: "`repos {code postal FR} [true/false]`", "Renvoie la liste des Jours férié de l'année en cours, true pour l'année suivante", inline: false)
                         .AddField(name: "`say {Ton message}`", "Fait parler Le_Z", inline: false)
                         .AddField(name: "`spam {Nombre à spam} {Ton Message}`", "Spam un message dans le channel où est exécutée la commande", inline: false)
                         .AddField(name: "`status [Pseudo discord ou @]`", "Renvoie le status de l'user", inline: false)
                         .AddField(name: "`tweet {Username Tweeter}`", "Affiche le dernier tweet du compte", inline: false)
                         .AddField(name: "`wakeup`", "Répond une insulte gratuite", inline: false);
                return ReplyAsync(embed: embedHelp.Build());
            }
            else
            {
                message = Format.Bold(message);
                return ReplyAsync(message);
            }
        }

        public enum Commandes
        {
            Help,
            Say,
            Wakeup,
            Avatar,
            Status,
            Clear,
            Spam,
            Bully,
            Tweet,
            Repos,
        }

        #endregion Help

        #region Say

        // z!say hello world -> **hello world**
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        {
            Context.Message.DeleteAsync();
            var boldEcho = Format.Bold(echo);
            return ReplyAsync(boldEcho);
        }

        #endregion Say

        #region Wakeup

        // z!wakeup 	
        [Command("wakeup")]
        public Task WakeUpAsync()
        {
            var commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Le_Z.Modules.Commands").GetMethods().ToList().FindAll(i => i.Module.Name == "Le_Z.dll");
            string summary = commandsList[3].GetXmlDocsSummary();

            var emoji = new Emoji("🖕🏼");
            Context.Message.AddReactionAsync(emoji);
            return ReplyAsync("**Ta gueule je dors**");
        }

        #endregion Wakeup

        #region Avatar

        /// <summary>
        /// Renvoie l'avatar d'un user à la taille choisie
        /// </summary>
        /// <param name="user">Pseudo discord ou @</param>
        /// <param name="size">Taille</param>
        /// <returns></returns>
        [Command("avatar")]
        public Task GetAndResizeAvatarAsync(SocketUser user, ushort size = 512)
        {
            List<ushort> imageSize = new List<ushort> { 16, 32, 64, 128, 256, 512, 1024, 2048 };
            if (imageSize?.FirstOrDefault(i => i == size) == 0)
            {
                MessageReference reference = new MessageReference(messageId: Context.Message.Id);
                string response = $"**Les tailles acceptées sont :** `{string.Join("` | `", imageSize)}`";
                return ReplyAsync(response, messageReference: reference);
            }
            ulong userID = user.Id;
            string avatarID = user.AvatarId;
            return ReplyAsync(CDN.GetUserAvatarUrl(userID, avatarID, size, ImageFormat.Auto));
        }

        #endregion Avatar

        #region Status

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
                UserStatus.Invisible => $"{userName} se cache",
                UserStatus.DoNotDisturb => $"{userName} n\'est pas dispo",
                UserStatus.Idle => $"{userName} n\'est pas dispo",
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

                if (userPlaying.Type == ActivityType.CustomStatus && user.Activities.Count > 1)
                {
                }

                if (userPlaying.Type == ActivityType.Playing)
                {
                    var embedGame = new EmbedBuilder();
                    bool showingTimespan = false;
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

                            .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Culture)} ", iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png");

                        if (richGameInfo.LargeAsset != null && richGameInfo.Details != null && richGameInfo.State != null)
                        {
                            if (richGameInfo.LargeAsset.Text != null)
                            {
                                embedGame.AddField(richGameInfo.Details, richGameInfo.LargeAsset.Text)
                                     .AddField(richGameInfo.State, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
                                showingTimespan = true;
                            }
                        }
                        else if (richGameInfo.Details != null && richGameInfo.State != null)
                        {
                            embedGame.AddField(richGameInfo.Details, ".")
                                     .AddField(richGameInfo.State, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
                            showingTimespan = true;
                        }
                        else if (richGameInfo.Details != null && richGameInfo.State == null)
                        {
                            embedGame.AddField(richGameInfo.Details, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
                            showingTimespan = true;
                        }
                        else if (richGameInfo.Details == null && richGameInfo.State != null)
                        {
                            embedGame.AddField(richGameInfo.State, $"depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");
                            showingTimespan = true;
                        }

                        if (!showingTimespan)
                            embedGame.WithDescription($"Depuis {userPlayingTime.ToString(@"hh\:mm\:ss")}");

                        if (richGameInfo.LargeAsset != null)
                            embedGame.WithThumbnailUrl($"{richGameInfo.LargeAsset.GetImageUrl()}");
                        ReplyAsync(userStatus);
                        Task.Delay(10);
                        return ReplyAsync(embed: embedGame.Build());
                    }
                }

                if (userPlaying.Type == ActivityType.Listening)
                {
                    if (userPlaying is SpotifyGame spotifyInfo)
                    {
                        var spotifyInfoDuration = (TimeSpan)spotifyInfo.Duration;
                        var spotifyInfoElapsed = (TimeSpan)spotifyInfo.Elapsed;
                        var embedSpotify = new EmbedBuilder();
                        embedSpotify.WithColor(Color.DarkGreen)
                            .WithTitle("Ecoute de la musique")
                            .WithUrl(spotifyInfo.TrackUrl)
                            .WithThumbnailUrl(spotifyInfo.AlbumArtUrl)
                            .AddField("Titre :", Format.Code(spotifyInfo.TrackTitle))
                            .AddField("Auteur :", Format.Code(string.Join("` **|** `", spotifyInfo.Artists)))
                            .AddField("Album :", Format.Code(spotifyInfo.AlbumTitle))
                            .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Culture)} ", iconUrl: "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png");
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

                if (userPlaying.Type == ActivityType.Streaming)
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

        #endregion Status

        #region Clear

        // z!clear [count]
        [Command("clear")]
        public async Task ClearAsync(int messageCount = 1)
        {
            int count = 0;
            var msgToDelete = Context.Channel.GetMessagesAsync().TakeLast(messageCount + 1);
            await foreach (var msg in msgToDelete)
            {
                foreach (var msgBis in msg)
                {
                    if (count == messageCount) return;
                    await msgBis.DeleteAsync();
                    count++;
                }
            }
        }

        #endregion Clear

        #region Spam

        // z!spam count message
        [Command("spam")]
        public Task SpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
            for (int i = 0; i < spamCount; i++) ReplyAsync(msgToSpam);
            return Task.CompletedTask;
        }

        #endregion Spam

        #region Bully

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

        #endregion Bully

        #region Tweet

        // z!tweet @TwitterUserName
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
            if (user == null)
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
            if (tweets.Length == 0)
            {
                await ReplyAsync("**L'utilisateur n'a aucun tweet disponible**");
                return;
            }
            Tweet tweet = tweets.First(tweetos => tweetos.ReferencedTweets == null || tweetos.ReferencedTweets.First().Type == ReferenceType.Quoted);

            var embedTweet = new EmbedBuilder();
            TimeSpan tweetosTime = tweet.CreatedAt.Value.TimeOfDay;
            int tweetosTimeHour = tweetosTime.Hours + await GetParisTimeZone();
            TimeSpan tweetTime = new TimeSpan(tweetosTimeHour, tweetosTime.Minutes, tweetosTime.Seconds);
            embedTweet.WithColor(Color.Blue)
                .WithAuthor($"{tweet.Author.Name} @{tweet.Author.Username}")
                .WithTitle($"Dernier Tweet de {tweet.Author.Name}")
                .WithUrl("https://twitter.com/" + tweet.Author.Username + $"/status/{tweet.Id}")
                .WithThumbnailUrl(tweet.Author.ProfileImageUrl)
                .WithDescription(tweet.Text)
                .WithFooter($"Publié à {tweetTime.ToString(@"hh\:mm")} le {tweet.CreatedAt.Value.Date.ToString("dd MMMM, yyyy", Culture)} ", iconUrl: "https://www.freepnglogos.com/uploads/twitter-logo-png/twitter-icon-circle-png-logo-8.png");
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
            if (tweet.ReferencedTweets != null)
            {
                if (tweet.ReferencedTweets.First().Type == ReferenceType.Quoted)
                {
                    ReferencedTweet referencedTweet = tweet.ReferencedTweets.First();
                    Tweet quoteTweet = await twitterClient.GetTweetAsync(referencedTweet.Id, tweetOptions: tweetOption, userOptions: userOption, mediaOptions: mediaOption);
                    embedTweet.AddField($"Cité de {quoteTweet.Author.Name} @{quoteTweet.Author.Username}", quoteTweet.Text);
                    if (quoteTweet.Attachments != null && tweet.Attachments == null)
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

        private class TimeZoneModel
        {
            public string UTC_offset { get; set; }
        }

        private static async Task<int> GetParisTimeZone()
        {
            using (HttpClient client = new HttpClient())
            {
                string path = "http://worldtimeapi.org/api/timezone/Europe/Paris";
                HttpResponseMessage response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                TimeZoneModel timeZone = JsonConvert.DeserializeObject<TimeZoneModel>(responseText);

                if (timeZone.UTC_offset == "+01:00") return 1;
                else return 2;

            }
        }

        #endregion Tweet

        #region Repos

        // z!repos codepostal [true]
        [Command("repos")]
        public async Task ReposMainAsync(string codePostal = null, bool twoYear = false)
        {
            if (codePostal == null)
            {
                await ReplyAsync("**Précise un code postal**");
                return;
            }

            int annee = DateTime.Now.Year;
            string zone = GetZone(codePostal);
            Embed[] embeds = await ReposAsync(annee, zone, codePostal);
            await ReplyAsync(embed: embeds[0]);
            if (twoYear)
                await ReplyAsync(embed: embeds[1]);
        }

        public class JoursFeriesResponseModel
        {
            public DateTime Date { get; set; }
            public string Nom { get; set; }
        }

        private string GetZone(string codePostal)
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

        private async Task<Embed[]> ReposAsync(int annee, string zone, string codePostal)
        {
            HttpClient clientAPI = new HttpClient();
            clientAPI.BaseAddress = new Uri("http://calendrier.api.gouv.fr/jours-feries/");

            var jours = await GetJoursFeriesAsync($"{zone}/{annee}.json", clientAPI);
            var joursn1 = await GetJoursFeriesAsync($"{zone}/{annee + 1}.json", clientAPI);

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
            embedJourFerie1.WithTitle($"Jours fériés de l'année {annee} pour le code postal {codePostal} :");
            var embedJourFerie2 = new EmbedBuilder();
            embedJourFerie2.WithTitle($"Jours fériés de l'année {annee + 1} pour le code postal {codePostal} :");

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

        private async Task<Dictionary<string, string>> GetJoursFeriesAsync(string path, HttpClient clientAPI)
        {
            HttpResponseMessage response = await clientAPI.GetAsync(path);
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();
            var jours = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

            return jours;
        }

        #endregion Repos











    }//Fin de la class
}//Fin du Namespace
