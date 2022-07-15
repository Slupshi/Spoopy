using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Namotion.Reflection;
using TwitterSharp.Response.RUser;

namespace Le_Z.Modules
{
    public class SlashCommands
    {
        readonly static Random Random = new Random();

        #region Polls

        // Y/N Poll
        public static async Task CreatePoll(SocketSlashCommand command)
        {
            string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
            bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
            SocketUser author = command.User;

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(new Color(27, 37, 70))
                                    .WithTitle($"Sondage de {author.Username}")
                                    .WithThumbnailUrl(author.GetAvatarUrl())
                                    .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png");
            var embed = await Program.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
            await embed.AddReactionsAsync(Program.ThumbEmojis);
            await Program.ZLog($"Sondage crée par {author.Username}");
        }

        // 2-9 Choices Poll
        public static async Task CreateComplexPoll(SocketSlashCommand command)
        {
            string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
            bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
            SocketUser author = command.User;

            var options = command.Data.Options.Where(x => x.Name.Contains("proposition")).ToList();

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(new Color(27, 37, 70))
                                    .WithTitle($"Sondage de {author.Username}")
                                    .WithThumbnailUrl(author.GetAvatarUrl())
                                    .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png");
            List<Emoji> emojis = new List<Emoji>();
            foreach(var option in options)
            {
                string number = option.Name.Split('n').LastOrDefault();
                embedBuilder.AddField($"Proposition n°{number}", option.Value);

                emojis.Add(Program.NumberEmoji.FirstOrDefault(x => x.Key.ToString() == number).Value);
            }
            var embed = await Program.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
            await embed.AddReactionsAsync(emojis);
            await Program.ZLog($"Sondage crée par {author.Username}");

        }

        #endregion

        #region Help

        public static async Task HelpAsync(SocketSlashCommand scommand)
        {
            try
            {
                    var commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Le_Z.Modules.Commands").GetMethods().ToList().FindAll(i => i.Module.Name == "Le_Z.dll");
                    commandsList.RemoveAll(x => x.Name == "HelpAsync");
                    commandsList.RemoveAll(x => x.Name == "CreateTweetEmbedAsync");
                    commandsList.RemoveAll(x => x.Name == "UseTwitterClientAsync");

                    var embedHelp = new EmbedBuilder();
                    embedHelp.WithTitle(Format.Underline("Voici de l'aide jeune Padawan"))
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
                    await scommand.RespondAsync(embed: embedHelp.Build());
                
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await scommand.RespondAsync("**Une erreur s'est produite avec l'éxécution de cette commande**");
            }
        }

        #endregion

        #region Status

        public static async Task StatusAsync(SocketSlashCommand command)
        {
            try
            {
                SocketGuildUser user = (SocketGuildUser)command.Data.Options.FirstOrDefault(x => x.Name == "username")?.Value;
                if (user == null)
                    user = (SocketGuildUser)command.User;

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
                                await command.RespondAsync(text: userStatus, embed: embedGame.Build());
                            }
                            if (userPlaying is RichGame richGameInfo)
                            {
                                var userPlayingTime = (TimeSpan)(DateTime.Now - richGameInfo.Timestamps.Start);
                                embedGame.WithTitle($"Joue à {richGameInfo.Name}")
                                         .WithColor(Color.LightGrey)
                                         .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png");

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
                                await command.RespondAsync(text: userStatus, embed: embedGame.Build());
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
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png");
                                var elapsed = spotifyInfoElapsed / spotifyInfoDuration;
                                int elalpsedBarLenght = (int)(30 * elapsed);
                                string elapsedBar = "";
                                for (int i = 0; i <= elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A0";
                                for (int i = 0; i <= 30 - elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A1";

                                embedSpotify.AddField("Durée :", $"{spotifyInfoElapsed.ToString(@"mm\:ss")} | {elapsedBar} | {spotifyInfoDuration.ToString(@"mm\:ss")}");
                                await command.RespondAsync(text: userStatus, embed: embedSpotify.Build());
                            }
                        }

                        if (userPlaying.Type == ActivityType.Streaming)
                        {
                            if (userPlaying is StreamingGame streamInfo)
                            {
                                userPlayingStatus = $"Et est en stream \"{streamInfo.Name}\"\nLien : {streamInfo.Url}";
                                userPlayingStatus = Format.Bold(userPlayingStatus);
                                userStatus = $"{userStatus}\n{userPlayingStatus}";
                                await command.RespondAsync($"{userStatus}");
                            }
                        }

                    }

                }
                await command.RespondAsync($"{userStatus}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await command.RespondAsync("Une erreur s'est produite avec l'éxécution de cette commande");
            }
        }

        #endregion



    }
}
