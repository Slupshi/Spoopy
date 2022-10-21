using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Spoopy.Modules
{
    public class ModalHandler
    {
        public static async Task YTCommentsModalSubmittedAsync(SocketModal modal)
        {
            try
            {
                var messageText = (modal.Data.Components as SocketMessageComponentData[])[0].Value;
                var messageID = modal.Data.CustomId.Split('|').Last();

                var message = await Properties.YoutubeVideoChannel.GetMessageAsync(ulong.Parse(messageID));
                var videoTitle = message.Embeds.First().Title;


                string response = Format.Bold($"Commentaire de {modal.User.Username} sur la vidéo `{videoTitle}` ```{DateTime.Now.ToString("T")} | {messageText} ```");
                await Properties.BotDMsChannel.SendMessageAsync(response);

                await modal.RespondAsync(text: Utilities.FormatToCode("Votre commentaires à bien été envoyé à Slupshi !"), ephemeral:true);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", ex.Message);
                await modal.RespondAsync(text: Utilities.FormatToCode("Une erreur s'est produite lors de l'envoi de votre commentaire"), ephemeral: true);
                await Utilities.SpoopyLogAsync("Une erreur s'est produite avec la modal de commentaires YT");
            }          
            
        }

        public static async Task ErrorContactModalSubmittedAsync(SocketModal modal)
        {
            try
            {
                var messageText = (modal.Data.Components as SocketMessageComponentData[])[0].Value;

                var user = Properties.Slupshi;

                await user.CreateDMChannelAsync();
                await user.SendMessageAsync(Utilities.FormatToCode(messageText));

                await modal.RespondAsync(text: Utilities.FormatToCode("Votre commentaires à bien été envoyé au dev !"), ephemeral: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", ex.Message);
                await modal.RespondAsync(text: Utilities.FormatToCode("Une erreur s'est produite lors de l'envoi de votre commentaire"), ephemeral: true);
                await Utilities.SpoopyLogAsync("Une erreur s'est produite avec la modal de contact d'erreur");
            }
        }
    }
}
