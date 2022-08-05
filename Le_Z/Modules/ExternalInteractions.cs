using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Le_Z.Modules
{
    public class ExternalInteractions
    {
        public static async Task UwUAsync(SocketUser user, SocketVoiceState newVoiceState)
        {
            if (newVoiceState.VoiceChannel == null || newVoiceState.VoiceChannel.Guild.Id != Properties.BanquiseID) return;

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

        public static async Task SetGameRoleAsync()
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

        public static async Task EmmerderZozoAsync(SocketGuildUser user)
        {
            await user.CreateDMChannelAsync();
            await user.SendMessageAsync("**Ils sont bien les skins dans ton shop aujourd'hui ? =D**");
        }

        public static async Task CheckRoleMembersAsync()
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

        public static async Task CheckDMsAsync(SocketUserMessage message)
        {
            string response = $"**{DateTime.Now.ToString("T")} | {message.Author.Username} sent :``` {message.CleanContent} ```**";
            await Properties.BotDMsChannel.SendMessageAsync(response);
        }

        public static async Task HandleNewYoutubeVideoAsync(SocketUserMessage message)
        {
            await message.PinAsync();
            var embed = new EmbedBuilder();
            embed.WithTitle("Nouvelle vidéo !")
                .WithUrl(message.GetJumpUrl())
                .WithThumbnailUrl(message.Author.GetAvatarUrl())
                .WithColor(Color.Red)
                .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Properties.Culture)} ", iconUrl: "https://assets.stickpng.com/thumbs/580b57fcd9996e24bc43c545.png")
                .WithDescription("N'hésites pas à mettre un petit like stp !\nTu peux aussi donner ton avis en utilisant un des bouton si dessous")
                .AddField("Lien :", $"**[Clique ici !]({message.CleanContent})**", inline: true)
                .AddField("Titre :", $"**`{message.Embeds.First().Title}`**", inline: true);

            var button = new ButtonBuilder();
            button.WithCustomId("ytCommentButton")
                .WithLabel("Donnes ton avis !")
                .WithStyle(ButtonStyle.Danger)
                .WithEmote(new Emoji("✏"));

            var msgComponents = new ComponentBuilder().WithButton(button);

            await Properties.HelloChannel.SendMessageAsync(text: "@everyone",embed: embed.Build(), components: msgComponents.Build());
        }
    }
}
