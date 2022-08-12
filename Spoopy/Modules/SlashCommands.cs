using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Namotion.Reflection;
using TwitterSharp.Response.RUser;

namespace Spoopy.Modules
{
    public class SlashCommands
    {

        #region Help

        public static async Task HelpAsync(SocketSlashCommand scommand)
        {
            try
            {
                await scommand.DeferAsync(ephemeral: true);
                //bool isStandard = (scommand.Data.Options.FirstOrDefault(x => x.Name == "standard")?.Value) == null ? false : (bool)(scommand.Data.Options.FirstOrDefault(x => x.Name == "standard")?.Value);

                List<MethodInfo> commandsList;

                //if (isStandard)
                //{
                //    commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Spoopy.Modules.Commands").GetMethods().ToList().FindAll(i => i.Module.Name == "Spoopy.dll");
                //    commandsList.RemoveAll(x => x.Name == "HelpAsync");
                //    commandsList.RemoveAll(x => x.Name == "CreateTweetEmbedAsync");
                //    commandsList.RemoveAll(x => x.Name == "UseTwitterClientAsync");
                //}
                //else
                //{
                    commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Spoopy.Modules.SlashCommands").GetMethods().ToList().FindAll(i => i.Module.Name == "Spoopy.dll");
                    commandsList.RemoveAll(x => x.Name == "HelpAsync");
                    commandsList.RemoveAll(x => x.Name == "TestAsync");
                //}

                var embedHelp = new EmbedBuilder();
                embedHelp.WithTitle(Format.Underline("Voici de l'aide jeune Padawan"))
                            .WithColor(Color.DarkBlue)
                            .WithFooter("Généré automatiquement", iconUrl: Properties.InfoIconURL);
                //if (isStandard)
                //{
                //    embedHelp.WithDescription("Chaque commande doit être éxécutée avec le préfix \"**z!**\"\n Les variables avec des '**{**' sont __obligatoires__, celles avec des '**[**' sont __optionnelles__");
                //}

                foreach (var command in commandsList)
                {
                    var commandsSummaryList = command.GetXmlDocsElement().ToXmlDocsContent().Split("\n").ToList();
                    commandsSummaryList.RemoveAll(i => string.IsNullOrWhiteSpace(i));
                    var commandName = command.CustomAttributes.Last().ConstructorArguments.First().ToString();
                    //if (isStandard)
                    //{
                    //    embedHelp.AddField(name: $"`{commandName} {string.Join(" ", commandsSummaryList.Skip(1).ToList())}`", value: commandsSummaryList.First());
                    //}
                    //else
                    //{
                        commandName = commandName.Replace("\"", string.Empty);
                        embedHelp.AddField(name: $"/{commandName}", value: commandsSummaryList.First());
                    //}
                }
                await scommand.ModifyOriginalResponseAsync(func: delegate(MessageProperties msg)
                {
                    msg.Embed = embedHelp.Build();
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await scommand.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Format.Bold("Une erreur s'est produite avec l'éxécution de cette commande");
                });
                await Program.ZLog("Une erreur est survenue avec SlashCommand Help", isError: true);
            }
        }

        #endregion

        #region Polls

        // Y/N Poll
        /// <summary>
        /// Crée un sondage avec Oui ou Non comme réponses
        /// </summary>
        /// <returns></returns>
        [SlashCommand("sondage", "Créer un sondage avec Oui/Non comme réponses")]
        public static async Task CreatePoll(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);
                string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
                bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
                bool isPersistant = (command.Data.Options.FirstOrDefault(x => x.Name == "persistant")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "persistant")?.Value);
                SocketUser author = command.User;

                EmbedBuilder embedBuilder = new();
                embedBuilder.WithColor(new Color(27, 37, 70))
                                        .WithTitle($"Sondage de {author.Username}")
                                        .WithThumbnailUrl(author.GetAvatarUrl())
                                        .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                        .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Properties.QuestionMarkURL);
                var embed = await Properties.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
                if (isPersistant)
                {
                    await embed.PinAsync();
                }
                await embed.AddReactionsAsync(Properties.ThumbEmojis);
                await Program.ZLog($"Sondage crée par {author.Username}");
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Utilities.FormatToCode("Sondage crée dans le channel \"sondage\"");
                });
                await CheckUselessPolls();
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Format.Bold("Une erreur s'est produite avec l'éxécution de cette commande");
                });
                await Program.ZLog("Une erreur est survenue avec SlashCommand SimplePoll", isError: true);
            }

        }

        // 2-9 Choices Poll
        /// <summary>
        /// Crée un sondage avec jusqu'à 9 propositions possibles
        /// </summary>
        /// <returns></returns>
        [SlashCommand("poll", "Créer un sondage avec jusqu'à 9 propositions possibles")]
        public static async Task CreateComplexPoll(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);
                string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
                bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
                bool isPersistant = (command.Data.Options.FirstOrDefault(x => x.Name == "persistant")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "persistant")?.Value);
                SocketUser author = command.User;

                var options = command.Data.Options.Where(x => x.Name.Contains("proposition")).ToList();

                EmbedBuilder embedBuilder = new();
                embedBuilder.WithColor(new Color(27, 37, 70))
                                        .WithTitle($"Sondage de {author.Username}")
                                        .WithThumbnailUrl(author.GetAvatarUrl())
                                        .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                        .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Properties.QuestionMarkURL);
                List<Emoji> emojis = new List<Emoji>();
                foreach (var option in options)
                {
                    string number = option.Name.Split('n').LastOrDefault();
                    embedBuilder.AddField($"{Properties.NumberEmoji.FirstOrDefault(x => x.Key.ToString() == number).Value} Proposition n°{number}", option.Value);

                    emojis.Add(Properties.NumberEmoji.FirstOrDefault(x => x.Key.ToString() == number).Value);
                }
                var embed = await Properties.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
                if (isPersistant)
                {
                    await embed.PinAsync();
                }
                await embed.AddReactionsAsync(emojis);
                await Program.ZLog($"Sondage crée par {author.Username}");
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Utilities.FormatToCode("Sondage crée dans le channel \"sondage\"");
                });
                await CheckUselessPolls();
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Format.Bold("Une erreur s'est produite avec l'éxécution de cette commande");
                });
                await Program.ZLog("Une erreur est survenue avec SlashCommand ComplexPoll", isError: true);
            }
            
        }

        private static async Task CheckUselessPolls()
        {
            var messages = await Properties.PollChannel.GetMessagesAsync().FirstAsync();
            var hours = TimeSpan.FromHours(24);
            foreach(var message in messages)
            {
                if (message.Content.Contains("pinned"))
                {
                    await message.DeleteAsync();
                }
                else
                {
                    var time = DateTime.Now - message.Timestamp;
                    if (time > hours && !message.IsPinned)
                    {
                        await message.DeleteAsync();
                    }
                }

            }
        }

        #endregion      

        #region Status
        /// <summary>
        /// Affiche le status de la personne
        /// </summary>
        /// <returns></returns>
        [SlashCommand("status", "Affiche le status de la personne")]
        public static async Task StatusAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);
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
                        // Gaming
                        if (userPlaying.Type == ActivityType.Playing)
                        {
                            var embedGame = new EmbedBuilder();
                            bool showingTimespan = false;
                            if (userPlaying is Game gameInfo && !(userPlaying is RichGame game))
                            {
                                embedGame.WithTitle($"Joue à \"{gameInfo.Name}\"");
                                await command.ModifyOriginalResponseAsync(func: delegate (MessageProperties msg)
                                {
                                    msg.Content = userStatus;
                                    msg.Embed = embedGame.Build();
                                });
                                return;
                            }
                            // Rich Game
                            if (userPlaying is RichGame richGameInfo)
                            {
                                TimeSpan userPlayingTime;
                                if (richGameInfo.Timestamps.Start != null)
                                {
                                    userPlayingTime = (TimeSpan)(DateTime.Now - richGameInfo.Timestamps.Start);
                                }
                                else
                                {
                                    userPlayingTime = (TimeSpan)(DateTime.Now - (DateTime.Now - TimeSpan.FromSeconds(1)));
                                }
                                embedGame.WithTitle($"Joue à {richGameInfo.Name}")
                                         .WithColor(Color.LightGrey)
                                         .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Properties.ControllerIconURL);

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
                                await command.ModifyOriginalResponseAsync(func: delegate (MessageProperties msg)
                                {
                                    msg.Content = userStatus;
                                    msg.Embed = embedGame.Build();
                                });
                                return;
                            }
                        }
                        // Spotify
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
                                    .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Properties.SpotifyLogoURL);
                                var elapsed = spotifyInfoElapsed / spotifyInfoDuration;
                                int elalpsedBarLenght = (int)(30 * elapsed);
                                string elapsedBar = "";
                                for (int i = 0; i <= elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A0";
                                for (int i = 0; i <= 30 - elalpsedBarLenght; i++)
                                    elapsedBar += "\u25A1";

                                embedSpotify.AddField("Durée :", $"{spotifyInfoElapsed.ToString(@"mm\:ss")} | {elapsedBar} | {spotifyInfoDuration.ToString(@"mm\:ss")}");
                                await command.ModifyOriginalResponseAsync(func: delegate (MessageProperties msg)
                                {
                                    msg.Content = userStatus;
                                    msg.Embed = embedSpotify.Build();
                                });
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
                                await command.ModifyOriginalResponseAsync(func: delegate (MessageProperties msg)
                                {
                                    msg.Content = userStatus;
                                });
                                return;
                            }
                        }

                    }

                }
                await command.ModifyOriginalResponseAsync(func: delegate (MessageProperties msg)
                {
                    msg.Content = userStatus;
                });
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Format.Bold("Une erreur s'est produite avec l'éxécution de cette commande");
                });
                await Program.ZLog("Une erreur est survenue avec SlashCommand Statut", isError: true);
            }
        }

        #endregion

        #region Music



        #endregion

        public static async Task TestAsync(SocketSlashCommand command)
        {
            try
            {

            }
            catch(Exception e)
            {
                await command.RespondAsync(text: e.Message);
            }
            
        }

    }
}
