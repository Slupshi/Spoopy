using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Spoopy.Services;
using TwitterSharp.Response.RTweet;

namespace Spoopy.Modules
{
    public class ExternalInteractions
    {
        private TwitterService _twitterService;
        public ExternalInteractions(TwitterService tweetService)
        {
            _twitterService = tweetService;
        }

        public async Task UwUAsync(SocketUser user, SocketVoiceState newVoiceState)
        {
            if(newVoiceState.VoiceChannel == null)
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

        public async Task SetGameRoleAsync()
        {
            try
            {
                var rolesList = Properties.Banquise.Roles.ToList();
                var activeUsers = Properties.Banquise.Users.Where(u => u.Activities.Count > 0 && u.IsBot == false).ToList();
                foreach (var user in activeUsers)
                {
                    if (user.Id == 434662109595435008 && user.Activities.FirstOrDefault(a => a.Name == "VALORANT") != null) // 434662109595435008 : zozoID
                    {
                        if (!Properties.HadEmmerderZozoToday)
                        {
                            await EmmerderZozoAsync(user);
                            Properties.HadEmmerderZozoToday = true;
                        }
                    }
                    var game = user.Activities.FirstOrDefault(a => a.Type == ActivityType.Playing);
                    if (game != null)
                    {
                        SocketRole role = rolesList.FirstOrDefault(r => r.Name == game.Name);
                        if (role == null)
                        {
                            RestRole restRole = await Properties.Banquise.CreateRoleAsync(name: game.Name, isMentionable: true, color: Properties.WhiteColor);
                            await Program.ZLog($"Rôle {restRole.Name} créé avec succès");
                            Console.WriteLine($"Rôle {restRole.Name} créé avec succès");
                            await user.AddRoleAsync(restRole);
                            Console.WriteLine($"Rôle {restRole.Name} ajouté à {user.Username}");
                        }
                        else
                        {
                            if (user.Roles.FirstOrDefault(r => r.Name == role.Name) == null)
                            {
                                await user.AddRoleAsync(role);
                                Console.WriteLine($"Rôle {role.Name} ajouté à {user.Username}");
                            }
                        }
                    }
                }
                //await CheckRoleMembers();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public async Task SetRoleOnGuildJoined(SocketGuildUser user)
        {
            await user.AddRoleAsync(Properties.TkiToiRole);
        }

        public async Task EmmerderZozoAsync(SocketGuildUser user)
        {
            await user.CreateDMChannelAsync();
            await user.SendMessageAsync(Format.Bold("Ils sont bien les skins dans ton shop aujourd'hui ? =D"));
        }

        public async Task CheckRoleMembersAsync()
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

        public async Task CheckDMsAsync(SocketUserMessage message)
        {
            string response = Format.Bold($"{DateTime.Now.ToString("T")} | {message.Author.Username} sent :``` {message.CleanContent} ```");
            await Properties.BotDMsChannel.SendMessageAsync(response);
        }

        public async Task HandleNewYoutubeVideoAsync(SocketUserMessage message)
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
            button.WithCustomId("ytCommentButton")
                .WithLabel("Donnes ton avis !")
                .WithStyle(ButtonStyle.Danger)
                .WithEmote(new Emoji("✏"));

            var msgComponents = new ComponentBuilder().WithButton(button);

            await Properties.HelloChannel.SendMessageAsync(text: "@everyone",embed: embed.Build(), components: msgComponents.Build());
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
            var splitMessage = message.Content.Split('/');
            var splitID = splitMessage.Last().Split('?');
            var tweet = (Tweet)await _twitterService.UseTwitterClientAsync(method: TwitterService.TwitterClientMethod.GetTweet, id: splitID.First());
            var embedBuilder = await _twitterService.CreateTweetEmbedAsync(tweet, title: $"{message.Author.Username} partage un tweet");
            await message.DeleteAsync();
            var botResponse = await message.Channel.SendMessageAsync(embed: embedBuilder.Build());

            await botResponse.AddReactionsAsync(Properties.ThumbEmojis);
        }
    }
}
