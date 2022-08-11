using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namotion.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Response.RUser;
using TwitterSharp.Request.Option;

namespace Spoopy.Modules
{
    /// <summary>
    /// Text Commands 
    /// </summary>
    public class Commands : ModuleBase<SocketCommandContext>
    {
        readonly Random Random = new Random();

        #region Help     	

        /// <summary>
        /// Si ceci s'affiche veuillez contacter Slupshi car c'est pas normal
        /// </summary>
        /// <returns></returns>
        [Command("help")]
        public Task HelpAsync()
        {
            try
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
                    var commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Le_Z.Modules.Commands").GetMethods().ToList().FindAll(i => i.Module.Name == "Le_Z.dll");
                    commandsList.RemoveAll(x => x.Name == "HelpAsync");
                    commandsList.RemoveAll(x => x.Name == "CreateTweetEmbedAsync");
                    commandsList.RemoveAll(x => x.Name == "UseTwitterClientAsync");

                    var embedHelp = new EmbedBuilder();
                    embedHelp.WithTitle(Format.Underline(message))
                             .WithColor(Color.DarkBlue)
                             .WithDescription("Chaque commande doit être éxécutée avec le préfix \"**z!**\"\n Les variables avec des '**{**' sont __obligatoires__, celles avec des '**[**' sont __optionnelles__")
                             .WithFooter("Généré automatiquement", iconUrl: "https://e7.pngegg.com/pngimages/359/338/png-clipart-logo-information-library-business-information-miscellaneous-blue-thumbnail.png");
                    foreach (var command in commandsList)
                    {
                        var commandsSummaryList = command.GetXmlDocsElement().ToXmlDocsContent().Split("\n").ToList();
                        commandsSummaryList.RemoveAll(i => string.IsNullOrWhiteSpace(i));
                        var commandName = command.CustomAttributes.Last().ConstructorArguments.First();
                        embedHelp.AddField(name: $"`{commandName} {string.Join(" ", commandsSummaryList.Skip(1).ToList())}`", value: commandsSummaryList.First());
                    }
                    return ReplyAsync(embed: embedHelp.Build());
                }
                else
                {
                    message = Format.Bold(message);
                    return ReplyAsync(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                return ReplyAsync("**Une erreur s'est produite avec l'éxécution de cette commande**");
            }
        }

        #endregion Help

        #region Say

        /// <summary>
        /// Fait parler Le_Z
        /// </summary>
        /// <param name="echo">{Ton message}</param>
        /// <returns></returns>
        [Command("say")]
        public Task SayAsync([Remainder] string echo)
        {
            try
            {
                Context.Message.DeleteAsync();
                var boldEcho = Format.Bold(echo);
                return ReplyAsync(boldEcho);
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                return ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        #endregion Say

        #region Wakeup

        /// <summary>
        /// Répond une insulte gratuite
        /// </summary>
        /// <returns></returns>
        [Command("wakeup")]
        public Task WakeUpAsync()
        {
            try
            {
                var emoji = new Emoji("🖕🏼");
                Context.Message.AddReactionAsync(emoji);
                return ReplyAsync("**Ta gueule je dors**");
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                return ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }
        #endregion Wakeup

        #region Avatar

        /// <summary>
        /// Renvoie l'avatar d'un user à la taille choisie
        /// </summary>
        /// <param name="user">{Pseudo discord ou @}</param>
        /// <param name="size">[Taille]</param>
        /// <returns></returns>
        [Command("avatar")]
        public Task GetAndResizeAvatarAsync(SocketUser user, ushort size = 512)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                return ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        #endregion Avatar

        #region Status

        /// <summary>
        /// Renvoie le status de l'user
        /// </summary>
        /// <param name="user">[Pseudo discord ou @]</param>
        /// <returns></returns>
        [Command("status")]
        public async Task StatusAsync([Remainder] SocketGuildUser user = null)
        {
            try
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
                if (user.Status != UserStatus.Offline)
                {
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
                }


                string userPlayingStatus = " ";
                var userStatus = $"{userPresenceStatus}";

                if (user.VoiceChannel != null)
                {
                    var userVoicePresenceStatus = $"Se trouve dans \"*{user.VoiceChannel}*\" dans \"*{user.VoiceChannel.Category}*\"";
                    userStatus = $"{userStatus}\n{userVoicePresenceStatus}";
                }

                userStatus = Format.Bold(userStatus);

                if (user.Activities.Count > 0)
                {
                    var userPlaying = user.Activities.FirstOrDefault(a => a.Type != ActivityType.CustomStatus);

                    if (userPlaying != null)
                    {
                        if (userPlaying.Type == ActivityType.Playing)
                        {
                            var embedGame = new EmbedBuilder();
                            bool showingTimespan = false;
                            if (userPlaying is Game gameInfo && !(userPlaying is RichGame game))
                            {
                                embedGame.WithTitle($"Joue à \"{gameInfo.Name}\"");
                                await ReplyAsync(message: userStatus, embed: embedGame.Build());
                                return;
                            }
                            if (userPlaying is RichGame richGameInfo)
                            {
                                var userPlayingTime = (TimeSpan)(DateTime.Now - richGameInfo.Timestamps.Start);
                                embedGame.WithTitle($"Joue à {richGameInfo.Name}")
                                         .WithColor(Color.LightGrey)
                                         .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ", iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png");

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
                                await ReplyAsync(message: userStatus, embed: embedGame.Build());
                                return;
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
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ", iconUrl: "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png");
                                var elapsed = spotifyInfoElapsed / spotifyInfoDuration;
                                int elalpsedBarLenght = (int)(30 * elapsed);
                                string elapsedBar = "";
                                for (int i = 0; i <= elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A0";
                                for (int i = 0; i <= 30 - elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A1";

                                embedSpotify.AddField("Durée :", $"{spotifyInfoElapsed.ToString(@"mm\:ss")} | {elapsedBar} | {spotifyInfoDuration.ToString(@"mm\:ss")}");
                                await ReplyAsync(message: userStatus, embed: embedSpotify.Build());
                                return;
                            }
                        }

                        if (userPlaying.Type == ActivityType.Streaming)
                        {
                            if (userPlaying is StreamingGame streamInfo)
                            {
                                userPlayingStatus = $"Et est en stream \"{streamInfo.Name}\"\nLien : {streamInfo.Url}";
                                userPlayingStatus = Format.Bold(userPlayingStatus);
                                userStatus = $"{userStatus}\n{userPlayingStatus}";
                                await ReplyAsync($"{userStatus}");
                                return;
                            }
                        }

                    }

                }
                await ReplyAsync($"{userStatus}");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
                return;
            }
        }

        #endregion Status

        #region Clear

        /// <summary>
        /// Supprime des messages dans le channel où est exécutée la commande
        /// </summary>
        /// <param name="messageCount">[Nombre à delete]</param>
        /// <returns></returns>
        [Command("clear")]
        public async Task ClearAsync(int messageCount = 1)
        {
            try
            {
                int count = 0;
                var msgToDelete = /*await*/ Context.Channel.GetMessagesAsync().TakeLast(messageCount + 1)/*.FlattenAsync()*/;
                //await ((ITextChannel)Context.Message.Channel).DeleteMessagesAsync(msgToDelete);
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
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }

        }

        #endregion Clear

        #region SoftSpam

        /// <summary>
        /// Spam un message dans le channel où est exécutée la commande (1 seul message)
        /// </summary>
        /// <param name="spamCount">{Nombre à spam}</param>
        /// <param name="msgToSpam">{Ton Message}</param>
        /// <returns></returns>
        [Command("softspam")]
        public Task SoftSpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
            string msgToReturn = null;
            try
            {
                for (int i = 0; i < spamCount; i++)
                {
                    msgToReturn += $"\n{msgToSpam}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                return ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
            return ReplyAsync(msgToReturn.Substring(0, 1950));
        }

        #endregion SoftSpam

        #region Spam

        /// <summary>
        /// Spam un message dans le channel où est exécutée la commande (Messages multiples)
        /// </summary>
        /// <param name="spamCount">{Nombre à spam}</param>
        /// <param name="msgToSpam">{Ton Message}</param>
        /// <returns></returns>
        [Command("spam")]
        public async Task SpamAsync(int spamCount, [Remainder] string msgToSpam)
        {
            try
            {
                for (int i = 0; i < spamCount; i++)
                {
                    await ReplyAsync(msgToSpam);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        #endregion Spam

        #region Bully

        /// <summary>
        /// Envoie un message en mp à un user
        /// </summary>
        /// <param name="user">{Pseudo discord ou @}</param>
        /// <param name="msg">{Ton Message}</param>
        /// <returns></returns>
        [Command("bully")]
        public Task BullyAsync(SocketGuildUser user, [Remainder] string msg)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                return ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        #endregion Bully

        #region Tweet

        /// <summary>
        /// Affiche le dernier tweet du compte
        /// </summary>
        /// <param name="username">{@Pseudo Twitter}</param>
        /// <returns></returns>
        [Command("tweet")]
        public async Task LastTweetAsync([Remainder] string username)
        {
            try
            {
                if (username.Contains(" "))
                {
                    await ReplyAsync("**Les Pseudos Twitter ne contiennent pas d'espaces**");
                    return;
                }
                User user = (User)await UseTwitterClientAsync(TwitterClientMethod.GetUser, username: username);
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

                Tweet tweet = (Tweet)await UseTwitterClientAsync(TwitterClientMethod.GetTweets, id: user.Id);
                if (tweet == null)
                {
                    await ReplyAsync("**L'utilisateur n'a aucun tweet de disponible**");
                    return;
                }

                var embedTweet = await CreateTweetEmbedAsync(tweet, title: "Dernier Tweet");
                await ReplyAsync(embed: embedTweet.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        

        #endregion Tweet

        #region Repos

        /// <summary>
        /// Renvoie la liste des Jours férié de l'année en cours, true pour l'année suivante
        /// </summary>
        /// <param name="codePostal">{code postal FR}</param>
        /// <param name="twoYear">[true]</param>
        /// <returns></returns>
        [Command("repos")]
        public async Task ReposMainAsync(string codePostal = null, bool twoYear = false)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await ReplyAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }

        }

        private class JoursFeriesResponseModel
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

        #region Sondage

        /// <summary>
        /// Crée un sondage, true pour un @everyone
        /// </summary>
        /// <param name="question">{La question}</param>
        /// <param name="isEveryone">[true]</param>
        /// <returns></returns>
        [Command("sondage")]
        public async Task SondageAsync([Remainder] string question)
        {
            bool isEveryone = Context.Message.Content.Split(" ").LastOrDefault() == "true";
            await Context.Message.DeleteAsync();
            if(Context.Guild.Id != Properties.BanquiseID)
            {
                return;
            }
            var author = Context.User;
            if (isEveryone)
            {
                var words = question.Split(' ').ToList();
                words.RemoveAt(words.Count - 1);
                question = string.Join(" ", words);
            }

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(new Color(27, 37, 70))
                                    .WithTitle($"Sondage de {author.Username}")
                                    .WithThumbnailUrl(author.GetAvatarUrl())
                                    .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ", iconUrl: "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png");
            var embed = await Properties.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
            await embed.AddReactionsAsync(Properties.ThumbEmojis);
            await Program.ZLog($"Sondage crée par {author.Username}");
        }

        #endregion

        #region Game (WIP)

        //[Command("game")]
        //public async Task GameAsync()
        //{
        //    using (HttpClient clientSteamAPI = new())
        //    {
        //        string path = SteamPath.GameList;
        //        HttpResponseMessage response = await clientSteamAPI.GetAsync(path);
        //        response.EnsureSuccessStatusCode();
        //        var responseText = await response.Content.ReadAsAsync<List<Toto>>();                

        //        await ReplyAsync("fuck");
        //    }
        //}

        //private class Toto
        //{
        //    [JsonProperty("appid")]
        //    public int AppID { get; set; }
        //    [JsonProperty("name")]
        //    public string Name { get; set; }
        //}

        //private class SteamPath
        //{
        //    public static string GameList { get; } = "https://api.steampowered.com/ISteamApps/GetAppList/v2/?";
        //}

        #endregion Game (WIP)

        //[Command("albert")]
        //public Task AlbertEmbed()
        //{
        //    var embedBuilder = new EmbedBuilder();
        //    embedBuilder.WithTitle("Playlist communautaire !")
        //        .WithColor(Color.DarkGreen)
        //        .WithDescription($"Voici une playlist Spotify communautaire où vous pouvez y mettre les musiques que vous souhaitez ! {Environment.NewLine} *Vous pouvez jouer cette playlist avec le bot Maki !* {Environment.NewLine} *Pour cela : **/play [Lien de la playlist]***")
        //        .WithThumbnailUrl("https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png");
        //    var componentBuilder = new ComponentBuilder();
        //    componentBuilder.WithButton(label: "Lien de la Playlist",style: ButtonStyle.Link, url: "");

        //    return Context.Channel.SendMessageAsync(text: "@here",embed: embedBuilder.Build(), components: componentBuilder.Build());
        //}





    }
}
