using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using Discord;
using Discord.WebSocket;

namespace Spoopy.Modules
{
    public class UserCommands
    {
        public static async Task GetUserAvatarAsync(SocketUserCommand arg)
        {
            try
            {
                //await arg.DeferAsync(ephemeral: true);

                SocketGuildUser user = (SocketGuildUser)arg.Data.Member;
                SocketGuildUser author = (SocketGuildUser)arg.User;

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor(author);
                embed.WithTitle($"Voici l'avatar de {user.DisplayName}");
                embed.WithImageUrl(user.GetAvatarUrl());
                embed.WithFooter(Utilities.GetCustomTimestamp());

                await arg.RespondAsync(embed: embed.Build(), ephemeral: true);
                //await arg.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", ex.Message);
                await arg.ModifyOriginalResponseAsync(Utilities.RespondToSlashCommandErrorAsync());
                await Utilities.SpoopyLogAsync(message: "Une erreur est survenue avec UserCommand Avatar", isError: true);
            }
            

            

        }
    }
}
