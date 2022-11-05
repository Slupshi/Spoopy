using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Spoopy.Models;
using Spoopy.Services;
using TwitterSharp.Response.RTweet;

namespace Spoopy.Modules
{
    public class ExternalInteractions
    {
        private Random _random = new Random();
        private TwitterService _twitterService;
        private LocalApiService _localApiService;
        public ExternalInteractions(TwitterService tweetService, LocalApiService localApiService)
        {
            _twitterService = tweetService;
            _localApiService = localApiService;
        }

        public async Task UwUAsync(SocketUser user, SocketVoiceState newVoiceState)
        {
            try
            {
                if (newVoiceState.VoiceChannel == null)
                {
                    if ((user as IGuildUser).RoleIds.Any(r => r == Properties.UwuID))
                    {
                        await (user as IGuildUser).RemoveRoleAsync(Properties.UwuRole);
                    }
                    return;
                }

                bool isUwU = newVoiceState.VoiceChannel.Id == 611881198700068864;

                if (isUwU)
                {
                    if (!(user as IGuildUser).RoleIds.Any(r => r == Properties.UwuID))
                    {
                        await (user as IGuildUser).AddRoleAsync(Properties.UwuRole);
                    }
                }
                else
                {
                    if ((user as IGuildUser).RoleIds.Any(r => r == Properties.UwuID))
                    {
                        await (user as IGuildUser).RemoveRoleAsync(Properties.UwuRole);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur UwU", isError:true);
            }
            
        }

        public async Task SetGameRoleAsync(SocketUser user)
        {
            try
            {
                SocketGuildUser banquiseUser = Properties.Banquise.Users.FirstOrDefault(x => x.Id == user.Id && !user.IsBot);
                if (banquiseUser == null) return;

                IActivity game = banquiseUser.Activities.FirstOrDefault(a => a.Type == ActivityType.Playing);
                if (game == null) return;

                if (Properties.Banquise.Roles.ToList().Any(role => role.Name.Contains(game.Name) || game.Name.Contains(role.Name)))
                {
                    SocketRole role = Properties.Banquise.Roles.FirstOrDefault(role => role.Name.Contains(game.Name) || game.Name.Contains(role.Name));
                    await banquiseUser.AddRoleAsync(role);
                }
                else
                {
                    if (!game.Name.ToLower().Contains("launcher"))
                    {
                        RestRole role = await Properties.Banquise.CreateRoleAsync(name: game.Name,
                                                                  color: Properties.WhiteColor,
                                                                  isMentionable: true);
                        await Utilities.SpoopyLogAsync($"Role Crée : {role.Name}");
                        await banquiseUser.AddRoleAsync(role);
                    }                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                await Utilities.SpoopyLogAsync("Erreur SetGameRole", isError: true);
            }    
        }

        public async Task UpdateGameRoleAsync()
        {
            try
            {
                Console.WriteLine("===== Updating DB Roles =====");
                List<SocketRole> banquiseRoles = Properties.Banquise.Roles.ToList();
                IEnumerable<SpoopyRole> roles = await _localApiService.GetBanquiseRolesAsync();
                if(roles != null)
                {
                    List<SpoopyRole> dbRoles = roles.ToList();
                    if(banquiseRoles.Count < dbRoles.Count)
                    {
                        foreach (SpoopyRole dbRole in dbRoles)
                        {
                            if (!banquiseRoles.Any(banquiseRole => banquiseRole.Name == dbRole.Name))
                            {
                                await _localApiService.DeleteBanquiseRoleAsync(dbRole);
                            }                            
                        }
                    }                    
                    foreach(SocketRole banquiseRole in banquiseRoles)
                    {
                        if(dbRoles.Any(dbRole => dbRole.Name == banquiseRole.Name))
                        {
                            SpoopyRole dbRole = dbRoles.FirstOrDefault(x => x.Name == banquiseRole.Name);
                            if(dbRole.MemberCount != banquiseRole.Members.Count())
                            {
                                dbRole.MemberCount = banquiseRole.Members.Count();
                                await _localApiService.PutBanquiseRoleAsync(dbRole);
                            }
                        }
                        else
                        {
                            SpoopyRole newDbRole = new SpoopyRole(
                                name: banquiseRole.Name, 
                                memberCount: banquiseRole.Members.Count(),
                                createdAt: banquiseRole.CreatedAt.Date
                                );
                            await _localApiService.PostBanquiseRoleAsync(newDbRole);
                        }
                    }
                }
                Console.WriteLine("===== DB Roles Updated =====");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Utilities.SpoopyLogAsync("Erreur UpdateGameRole", isError:true);
            }

        }

        public async Task SetRoleOnGuildJoined(SocketGuildUser user)
        {
            try
            {
                if (user.Guild == Properties.Banquise)
                {
                    await user.AddRoleAsync(Properties.TkiToiRole);
                }
                else if (user.Guild == Properties.TeykhoServer) 
                {
                    await user.AddRoleAsync(Properties.ViewerRole);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur SetRoleOnJoin", isError: true);
            }
        }

        public async Task EmmerderZozoAsync(SocketGuildUser user, string gameName)
        {
            try
            {
                string message;
                if (gameName == "VALORANT")
                {
                    message = "Ils sont bien les skins dans ton shop aujourd'hui ? =D";
                }
                else
                {
                    message = "Sale drogué";
                }
                await user.CreateDMChannelAsync();
                await user.SendMessageAsync(Format.Bold(message));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans EmmerderZozo", isError: true);
            }
            
        }

        public async Task CheckRoleMembersAsync()
        {
            try
            {
                foreach (SocketRole role in Properties.Banquise.Roles)
                {
                    if (role.Members.ToList().Count > 1 && role.Members.ToList().Count < 3 && role.Color == Properties.WhiteColor)
                    {
                        await role.ModifyAsync(r => r.Color = Properties.GreenColor);
                    }
                    else if (role.Members.ToList().Count > 3 && (role.Color == Properties.WhiteColor || role.Color == Properties.GreenColor))
                    {
                        await role.ModifyAsync(r => r.Color = Properties.RedColor);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur CheckRole",isError:true);
            }
           
        }

        public async Task CheckDMsAsync(SocketUserMessage message)
        {
            try
            {
                string response = Format.Bold($"DM de {message.Author.Username} : ```{DateTime.Now.ToString("T")} | {message} ```");
                await Properties.BotDMsChannel.SendMessageAsync(response);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur CheckDMs", isError: true);
            }
            
        }

        public async Task HandleNewYoutubeVideoAsync(SocketUserMessage message)
        {
            try
            {
                await message.PinAsync();
                var embed = new EmbedBuilder();
                embed.WithTitle("Nouvelle vidéo !")
                    .WithUrl(message.GetJumpUrl())
                    .WithThumbnailUrl(message.Author.GetAvatarUrl())
                    .WithColor(Color.Red)
                    .WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Properties.YoutubeLogoURL)
                    .WithDescription("N'hésites pas à mettre un petit like stp !\nTu peux aussi donner ton avis en utilisant un des bouton si dessous")
                    .AddField("Lien :", Format.Bold($"[Clique ici !]({message.CleanContent})"), inline: true)
                    .AddField("Titre :", Format.Bold(Format.Code($"{message.Embeds.First().Title}")), inline: true);

                var button = new ButtonBuilder();
                button.WithCustomId($"ytCommentButton|{message.Id}")
                    .WithLabel("Donnes ton avis !")
                    .WithStyle(ButtonStyle.Danger)
                    .WithEmote(new Emoji("✏"));

                var msgComponents = new ComponentBuilder().WithButton(button);

                await Properties.HelloChannel.SendMessageAsync(text: "@everyone", embed: embed.Build(), components: msgComponents.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le newVideoEmbed", isError: true);
            }
           
        }

        public async Task HandleTriggerWords(SocketUserMessage message)
        {
            #region TriggerWords
            if (message.Content.ToLower() == "ah" || message.Content.ToLower() == "ahh")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/ah-denis-brognart-tf1-koh-lanta-gif-7256068");
                return;
            }
            if (message.Content.ToLower() == "ils sont là" || message.Content.ToLower() == "ils sont la")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/marine-le-pen-le-pen-gif-8538154");
                return;
            }
            if (message.Content.ToLower() == "salut mon pote")
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/salut-mon-pote-hi-buddy-michel-drucker-gif-16070000");
                return;
            }
            if (message.Content.ToLower().Contains("zeub") || message.Content.ToLower().Contains("zob"))
            {
                var emoji = new Emoji("🍆");
                await message.AddReactionAsync(emoji);
                await message.Channel.SendMessageAsync("https://tenor.com/view/penis-standing-erect-erection-smile-gif-15812844");
            }
            if (message.Content.ToUpper().Contains("DEMARRER") || message.Content.ToUpper().Contains("DEMARRE"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/je-vais-le-d%C3%A9marrer-cet-gif-19154207");
            }
            if (message.Content.ToUpper().Contains("PHILIPPE"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/philippe-cobra-hitman-gif-13748655");
            }
            if (message.Content.ToLower().Contains("enorme") || message.Content.ToLower().Contains("énorme"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/%C3%A9norme-jamy-pas-sorcier-huge-gif-14277967");
            }
            if (message.Content.ToUpper().Contains("PARCOUR") || message.Content.ToUpper().Contains("PARCOURS") || message.Content.ToUpper().Contains("PARKOUR") || message.Content.ToUpper().Contains("PARKOURS"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/parkour-theoffice-freerunning-gif-5128248");
            }
            if (message.Content.ToUpper().Contains("EXPLOSION"))
            {
                await message.Channel.SendMessageAsync("https://tenor.com/view/explosion-megumin-konusoba-magic-destruction-gif-7559841");
            }
            #endregion TriggerWords

            if (message.Attachments.Count > 0 && message.Author.Id == Properties.SlupID)
            {
                MessageReference messageRef = new MessageReference(messageId: message.Id);
                await message.Channel.SendMessageAsync("**Les sources de ce message ne sont surement pas valide**", messageReference: messageRef);
                return;
            }
        }

        public async Task HandleNewTweetSent(SocketUserMessage message)
        {
            try
            {
                var splitMessage = message.Content.Split('/');
                var splitID = splitMessage.Last().Split('?');
                var tweet = (Tweet)await _twitterService.UseTwitterClientAsync(method: TwitterService.TwitterClientMethod.GetTweet, id: splitID.First());
                var embedBuilder = await _twitterService.CreateTweetEmbedAsync(tweet, title: $"{message.Author.Username} partage un tweet");
                await message.DeleteAsync();
                var botResponse = await message.Channel.SendMessageAsync(embed: embedBuilder.Build());

                await botResponse.AddReactionsAsync(Properties.ThumbEmojis);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le tweetEmbed", isError: true);
            }
            
        }

        public async Task ReorderVocalChannel()
        {
            try
            {
                var channels = Properties.Banquise.CategoryChannels.FirstOrDefault(x => x.Id == 611568952069586947).Channels.ToList();
                List<int> positions = new();
                channels.ForEach(x => positions.Add(x.Position));
                channels.ForEach(async x =>
                {
                    await x.ModifyAsync((chan) =>
                    {
                        int pos = _random.Next(positions.Count);
                        chan.Position = positions.ElementAt(pos);
                        positions.Remove(pos);
                    });
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Utilities.SpoopyLogAsync("Erreur dans le reorder des channels vocaux", isError: true);
            }
             

        }

        public async Task GetNumberFact()
        {
            string year = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/year");
            string trivia = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/trivia");
            string date = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/date");

            while(year.Count() > 128)
            {
                year = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/year");
            }
            while (trivia.Count() > 128)
            {
                trivia = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/trivia");
            }
            while (date.Count() > 128)
            {
                date = await Utilities.ScrapHtmlAsync("http://numbersapi.com/random/date");
            }

            Properties.BotActivities.Enqueue(year);
            Properties.BotActivities.Enqueue(trivia);
            Properties.BotActivities.Enqueue(date);

        }

    }

}
