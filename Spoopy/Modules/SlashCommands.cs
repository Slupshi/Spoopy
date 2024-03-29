﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp.Text;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Namotion.Reflection;
using Spoopy.Jobs;
using Spoopy.Models;
using Spoopy.Variables;

namespace Spoopy.Modules
{
    public class SlashCommands
    {
        //----------| GlobalCommand |----------//
        #region Help

        public static async Task HelpAsync(SocketSlashCommand scommand)
        {
            try
            {
                await scommand.DeferAsync(ephemeral: true);

                List<MethodInfo> commandsList = Assembly.GetExecutingAssembly().GetModules().First().GetType("Spoopy.Modules.SlashCommands").GetMethods().ToList().FindAll(i => i.Module.Name == "Spoopy.dll");
                commandsList.RemoveAll(x => x.Name == "HelpAsync");
                commandsList.RemoveAll(x => x.Name.Contains("Test"));
                
                var embedHelp = new EmbedBuilder();
                embedHelp.WithTitle(Format.Underline("Voici de l'aide jeune Padawan"))
                            .WithColor(Color.DarkBlue)
                            .WithFooter("Généré automatiquement", iconUrl: Constantes.InfoIconURL);

                bool guildField = false;

                foreach (var command in commandsList)
                {
                    var commandsSummaryList = command.GetXmlDocsElement().ToXmlDocsContent().Split("\n").ToList();
                    commandsSummaryList.RemoveAll(i => string.IsNullOrWhiteSpace(i));
                    var commandName = command.CustomAttributes.Last().ConstructorArguments.First().ToString();
                    commandName = commandName.Replace("\"", string.Empty);
                    if(commandsSummaryList.Count > 1)
                    {
                        var guild = typeof(Properties).GetField(commandsSummaryList.Skip(1).First()).GetValue(null);
                        if(guild is SocketGuild)
                        {
                            SocketGuild server = (SocketGuild)guild;
                            if(server.Id == scommand.GuildId)
                            {
                                if (!guildField)
                                {
                                    embedHelp.AddField(name: "============================", value: $"Les suivantes sont exclusives à : `{server.Name}`");
                                    guildField = true;
                                }
                                embedHelp.AddField(name: $"/{commandName}", value: commandsSummaryList.First());
                            }
                        }
                    }
                    else
                    {
                        embedHelp.AddField(name: $"/{commandName}", value: commandsSummaryList.First());
                    }
                            
                }
                await scommand.ModifyOriginalResponseAsync(func: delegate(MessageProperties msg)
                {
                    msg.Embed = embedHelp.Build();
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await scommand.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync("Une erreur est survenue avec SlashCommand Help", isError: true);
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
                SocketGuildUser user = (SocketGuildUser)command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterUsername)?.Value;
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
                                TimeSpan? userPlayingTime = null;
                                if (richGameInfo.Timestamps != null && richGameInfo.Timestamps.Start != null)
                                {
                                    userPlayingTime = (TimeSpan)(DateTime.Now - richGameInfo.Timestamps.Start);
                                }
                                embedGame.WithTitle($"Joue à {richGameInfo.Name}")
                                         .WithColor(Color.LightGrey)
                                         .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.ControllerIconURL);

                                if (richGameInfo.LargeAsset != null && richGameInfo.Details != null && richGameInfo.State != null)
                                {
                                    if (richGameInfo.LargeAsset.Text != null)
                                    {
                                        embedGame.AddField(richGameInfo.Details, richGameInfo.LargeAsset.Text)
                                             .AddField(richGameInfo.State, userPlayingTime == null ? "depuis une durée inconnue" : $"depuis {userPlayingTime?.ToString(@"hh\:mm\:ss")}");
                                        showingTimespan = true;
                                    }
                                }
                                else if (richGameInfo.Details != null && richGameInfo.State != null)
                                {
                                    embedGame.AddField(richGameInfo.Details, ".")
                                             .AddField(richGameInfo.State, userPlayingTime == null ? "depuis une durée inconnue" : $"depuis {userPlayingTime?.ToString(@"hh\:mm\:ss")}");
                                    showingTimespan = true;
                                }
                                else if (richGameInfo.Details != null && richGameInfo.State == null)
                                {
                                    embedGame.AddField(richGameInfo.Details, userPlayingTime == null ? "depuis une durée inconnue" : $"depuis {userPlayingTime?.ToString(@"hh\:mm\:ss")}");
                                    showingTimespan = true;
                                }
                                else if (richGameInfo.Details == null && richGameInfo.State != null)
                                {
                                    embedGame.AddField(richGameInfo.State, userPlayingTime == null ? "depuis une durée inconnue" : $"depuis {userPlayingTime?.ToString(@"hh\:mm\:ss")}");
                                    showingTimespan = true;
                                }

                                if (!showingTimespan)
                                    embedGame.WithDescription(userPlayingTime == null ? "Depuis une durée inconnue" : $"Depuis {userPlayingTime?.ToString(@"hh\:mm\:ss")}");

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
                                    .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.SpotifyLogoURL);
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
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync("Une erreur est survenue avec SlashCommand Statut", isError: true);
            }
        }

        #endregion

        #region Music



        #endregion

        #region Avatar
        /// <summary>
        /// Récupère l'avatar un utilisateur du serveur
        /// </summary>
        /// <returns></returns>
        [SlashCommand("avatar", "Récupère l'avatar un utilisateur du serveur")]
        public static async Task GetUserAvatarAsync(SocketSlashCommand command)
        {            
            try
            {
                await command.DeferAsync(ephemeral: true);

                SocketGuildUser user = (SocketGuildUser)command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterUsername)?.Value;
                var size = command.Data.Options.FirstOrDefault((x) => x.Name == CommandsConstantes.CommandParameterSize)?.Value;

                if(user == null)
                    user = (SocketGuildUser)command.User;
                if (size == null)
                    size = 512;

                ulong userID = user.Id;
                string avatarID = user.AvatarId;

                string avatarURL = CDN.GetUserAvatarUrl(userID, avatarID, ushort.Parse(size.ToString()), ImageFormat.Auto);

                var openButton = new ButtonBuilder();
                openButton.WithUrl(avatarURL)
                    .WithLabel("Ouvrir l'originale")
                    .WithStyle(ButtonStyle.Link);

                //var copyButton = new ButtonBuilder();
                //copyButton.WithCustomId("copyAvatarButton")
                //    .WithLabel("Copier le lien")
                //    .WithStyle(ButtonStyle.Primary);

                var msgComponents = new ComponentBuilder().WithButton(openButton)/*.WithButton(copyButton)*/;

                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = avatarURL;
                    msg.Components = msgComponents.Build();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync(message: "Une erreur est survenue avec SlashCommand Avatar", isError: true);
            }
        }

        #endregion

        #region RandomOrg

        /// <summary>
        /// Selectionne un nombre aléatoire
        /// </summary>
        /// <returns></returns>
        [SlashCommand("random", "Selectionne un nombre random")]
        public static async Task RandomOrgAsync(SocketSlashCommand command)
        {
            bool isVisible = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterVisible)?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterVisible)?.Value);
            try
            {
                await command.DeferAsync(ephemeral: !isVisible);

                int start = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterStart)?.Value) == null ? 1 : Convert.ToInt32((command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterStart)?.Value));
                start = start == 0 ? 1 : start;

                int end = Convert.ToInt32(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterEnd)?.Value);

                if (end <= start)
                {
                    await command.ModifyOriginalResponseAsync((MessageProperties msg) =>
                    {
                        msg.Content = Format.Bold("Le nombre de fin doit être supérieur au nombre de départ !");
                    });
                    return;
                }

                Random random = new Random();

                int response = random.Next(start, end + 1);

                await command.ModifyOriginalResponseAsync((MessageProperties msg) =>
                {
                    msg.Content = Format.Bold($"La réponse est : {response}");
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync(message: "Une erreur est survenue avec SlashCommand RandomOrg", isError: true);
            }
        }

        #endregion

        #region SpoopyStatus

        /// <summary>
        /// Obtenir le status du bot
        /// </summary>
        /// <returns></returns>
        [SlashCommand("spoopy", "Obtenir le status du bot")]
        public static async Task PrintSpoopyStatusAsync(SocketSlashCommand command)
        {            
            try
            {
                await command.DeferAsync(ephemeral: true);
                SpoopyStatus status = Utilities.GetSpoopyStatus();
                SpoopyInfos infos = await Utilities.GetSpoopyInfos();

                var spoopyEmbed = new EmbedBuilder();
                spoopyEmbed.WithTitle($"{infos.Name}'s Status")
                    .WithColor(Color.DarkOrange)
                    .WithThumbnailUrl(infos.AvatarURL)
                    .WithDescription($"Created at {Format.Code(infos.CreatedAt.ToString("d"))} by {Format.Code(infos.Owner.Username)}")
                    .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: infos.AvatarURL);

                spoopyEmbed.AddField("Connection", Format.Code(status.IsRunning ? "Connecté" : "Déconnecté"))
                    .AddField("Uptime", Format.Code($"{status.Uptime.Days} day{(status.Uptime.Days > 1 ? "s" : string.Empty)}, {status.Uptime.Hours} hour{(status.Uptime.Hours > 1 ? "s" : string.Empty)}, {status.Uptime.Minutes} minute{(status.Uptime.Minutes > 1 ? "s" : string.Empty)}"))
                    .AddField("Runtime", Format.Code($"{status.Runtime.Days} day{(status.Runtime.Days > 1 ? "s" : string.Empty)}, {status.Runtime.Hours} hour{(status.Runtime.Hours > 1 ? "s" : string.Empty)}, {status.Runtime.Minutes} minute{(status.Runtime.Minutes > 1 ? "s" : string.Empty)}"))
                    .AddField("Servers Count", Format.Code(status.ServersCount.ToString()));

                await command.ModifyOriginalResponseAsync((msg) =>
                {
                    msg.Embed = spoopyEmbed.Build();
                });

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le PrintSpoopyStatus", isError: true);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());

            }
        }


        #endregion

        #region Shuffle
        /// <summary>
        /// Mélange les lettres d'une chaine de charactères
        /// </summary>
        /// <returns></returns>
        [SlashCommand("shuffle", "Mélange les lettres d'une chaine de charactères")]
        public static async Task ShuffleAsync(SocketSlashCommand command)
        {
            bool isVisible = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterVisible)?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterVisible)?.Value);
            try
            {
                await command.DeferAsync(ephemeral: !isVisible);
                string entry = command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterStart)?.Value.ToString();
                entry.Trim();

                Random random = new Random();
                string output = "";

                var buffer = entry.ToLower().ToList();
                int count = buffer.Count;
                for(int i = 0; i < count; i++)
                {
                    int index = random.Next(buffer.Count);                    
                    output += buffer.ElementAt(index);
                    buffer.RemoveAt(index);
                }

                output = output.First().ToString().ToUpper() + output.Substring(1);

                await command.ModifyOriginalResponseAsync((msg) =>
                {
                    msg.Content = $"**Entrée : {Format.Code(entry)}** {Environment.NewLine}**Sortie : {Format.Code(output)}**";
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans la commande Shuffle", isError: true);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
            }
        }

        #endregion

        //----------| BanquiseCommand |----------//

        #region Polls

        // Y/N Poll
        /// <summary>
        /// Crée un sondage avec Oui ou Non comme réponses
        /// </summary>
        /// <param name="guild">Banquise</param>
        /// <returns></returns>
        [SlashCommand("sondage", "Créer un sondage avec Oui/Non comme réponses")]
        public static async Task CreatePollAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);
                string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterQuestion)?.Value);
                bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterEveryone)?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterEveryone)?.Value);
                int duration = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterDuration)?.Value) == null ? 24 : Convert.ToInt32(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterDuration)?.Value);
                SocketUser author = command.User;

                EmbedBuilder embedBuilder = new();
                embedBuilder.WithColor(new Color(27, 37, 70))
                                        .WithTitle($"Sondage de {author.Username}")
                                        .WithThumbnailUrl(author.GetAvatarUrl())
                                        .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                        .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.QuestionMarkURL);
                IUserMessage embed = await Properties.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
                await embed.AddReactionsAsync(Constantes.ThumbEmojis);
                await Utilities.SpoopyLogAsync($"Sondage crée par {author.Username}");
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Utilities.FormatToCode("Sondage crée dans le channel \"sondage\"");
                });

                await QuartzScheduler.StartBasicPollJob(DateTime.Now.Add(TimeSpan.FromHours(duration)), (long)embed.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync("Une erreur est survenue avec SlashCommand SimplePoll", isError: true);
            }
        }

        // 2-9 Choices Poll
        /// <summary>
        /// Crée un sondage avec jusqu'à 9 propositions possibles
        /// </summary> 
        /// <param name="guild">Banquise</param>
        /// <returns></returns>
        [SlashCommand("poll", "Créer un sondage avec jusqu'à 9 propositions possibles")]
        public static async Task CreateComplexPollAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);
                string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterQuestion)?.Value);
                bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterEveryone)?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterEveryone)?.Value);
                int duration = (command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterDuration)?.Value) == null ? 24 : Convert.ToInt32(command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterDuration)?.Value);
                SocketUser author = command.User;

                var options = command.Data.Options.Where(x => x.Name.Contains("proposition")).ToList();

                EmbedBuilder embedBuilder = new();
                embedBuilder.WithColor(new Color(27, 37, 70))
                                        .WithTitle($"Sondage de {author.Username}")
                                        .WithThumbnailUrl(author.GetAvatarUrl())
                                        .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                        .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.QuestionMarkURL);
                List<Emoji> emojis = new List<Emoji>();
                foreach (var option in options)
                {
                    string number = option.Name.Split('n').LastOrDefault();
                    embedBuilder.AddField($"{Constantes.NumberEmojis.FirstOrDefault(x => x.Key.ToString() == number).Value} Proposition n°{number}", option.Value);

                    emojis.Add(Constantes.NumberEmojis.FirstOrDefault(x => x.Key.ToString() == number).Value);
                }
                var embed = await Properties.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
                await embed.AddReactionsAsync(emojis);
                await Utilities.SpoopyLogAsync($"Sondage crée par {author.Username}");
                await command.ModifyOriginalResponseAsync(delegate (MessageProperties msg)
                {
                    msg.Content = Utilities.FormatToCode("Sondage crée dans le channel \"sondage\"");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", e.Message);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync("Une erreur est survenue avec SlashCommand ComplexPoll", isError: true);
            }

        }

        #endregion

        #region FakeBan

        /// <summary>
        /// Ban quelqu'un définitivement du serveur
        /// </summary>
        /// <param name="guild">Banquise</param>
        /// <returns></returns>
        [SlashCommand("ban", "Ban une personne du serveur")]
        public static async Task FakeBanAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral:true);

                bool userWasNull = false;

                SocketGuildUser user = (SocketGuildUser)command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.CommandParameterUsername)?.Value;
                SocketGuildUser author = (SocketGuildUser)command.User;

                if (user == null)
                {
                    user = author;
                    userWasNull = true;
                }

                await author.SetTimeOutAsync(TimeSpan.FromSeconds(60));
                await author.CreateDMChannelAsync();
                if (userWasNull) 
                {
                    await author.SendMessageAsync(Format.Bold("La prochaine fois précise un pseudo ... Sinon ça va encore te tomber dessus"));
                }
                else
                {
                    await author.SendMessageAsync(Format.Bold("Tu as vraiment cru que j'allais vraiment ban quelqu'un ??"));
                    await Properties.HelloChannel.SendMessageAsync(Format.Bold($"@everyone {author.Mention} a essayé de ban {user.Mention} du coup il est Timed Out ce con"));
                    await command.ModifyOriginalResponseAsync((msg) =>
                    {
                        msg.Content = Format.Bold("Tu viens de te TimedOut tous seul en faisant ça !");
                    });
                }   

            }
            catch(Exception e)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", e.Message);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync(message: "Une erreur est survenue avec SlashCommand FakeBan", isError: true);
            }
        }

        #endregion

        //----------| TeykhoCommand |----------//

        #region GameRole
        /// <summary>
        /// Obtiens le rôle associé à ce channel
        /// </summary>
        /// <param name="guild">TeykhoServer</param>
        /// <returns></returns>
        [SlashCommand("game", "Obtiens le rôle associé à ce channel")]
        public static async Task GetGameRoleAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);

                SocketGuildUser user = (SocketGuildUser)command.User;
                string role = command.Channel.Name;

                var roleToAdd = Properties.TeykhoGameRoleDico.FirstOrDefault(x => role.Contains(x.Key)).Value;
                if (roleToAdd == null)
                {
                    await command.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Content = Format.Bold("Ce rôle n'existe pas");
                    });
                    return;
                }
                else if (user.Roles.Contains(roleToAdd))
                {
                    await command.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Content = Format.Bold("Tu as déjà ce rôle");
                    });
                    return;
                }              

                await user.AddRoleAsync(roleToAdd);

                await command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = Format.Bold("Role ajouté avec succès");
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le GetGameRole", isError: true);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
            }
            
        }

        #endregion

        #region StreamStart
        /// <summary>
        /// Annonce un nouveau stream de Teykho
        /// </summary>
        /// <param name="guild">TeykhoServer</param>
        /// <returns></returns>
        [SlashCommand("stream", "Annonce un nouveau stream")]
        public static async Task StartStreamAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);

                string game = command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.StreamStartCommandParameterGame)?.Value.ToString();
                SocketGuildUser user = (SocketGuildUser)command.Data.Options.FirstOrDefault(x => x.Name == CommandsConstantes.StreamStartCommandParameterStreamer)?.Value;

                string link = Properties.TeykhoStreamers.FirstOrDefault(x => x.Key == (long)user.Id).Value;

                var openButton = new ButtonBuilder();
                openButton.WithUrl($"https://www.twitch.tv/{link}")
                    .WithLabel("Regarder le stream sur Twitch")
                    .WithStyle(ButtonStyle.Link);

                var msgComponents = new ComponentBuilder().WithButton(openButton);

                await Properties.StreamChannel.SendMessageAsync(Format.Bold($"{user.DisplayName} est en stream sur {Format.Code(game)} @everyone"), components: msgComponents.Build());                

                await command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = Format.Bold("Commande éxécutée avec succès !");
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le StartStream", isError: true);
                await command.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
            }
        }

        #endregion


        public static async Task TestAsync(SocketSlashCommand command)
        {
            try
            {
                
                //await command.RespondAsync(year + Environment.NewLine + trivia + Environment.NewLine + date);

                //var embed = new EmbedBuilder();
                //embed.WithTitle("Test")
                //    .WithDescription("❌❌❌\n⬛⬛⬛\n⭕⭕⭕");
                //await command.RespondAsync(embed: embed.Build());
                
            }
            catch (Exception e)
            {
                await command.RespondAsync(text: e.Message);
            }

        }
    }
}
