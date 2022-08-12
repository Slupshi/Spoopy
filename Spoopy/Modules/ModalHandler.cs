using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Spoopy.Modules
{
    public class ModalHandler
    {
        public static async Task YTCommentsModalSubmitted(SocketModal modal)
        {
            try
            {
                var messageText = (modal.Data.Components as SocketMessageComponentData[])[0].Value;
                var messageID = modal.Data.CustomId.Split('|').Last();

                var message = await Properties.HelloChannel.GetMessageAsync(ulong.Parse(messageID));
                var videoTitle = message.Embeds.First().Fields.FirstOrDefault(x => x.Name.Contains("Titre")).Value;


                string response = Format.Bold($"Commentaire de {modal.User.Username} sur la vidéo {videoTitle} ```{DateTime.Now.ToString("T")} | {messageText} ```");
                await Properties.BotDMsChannel.SendMessageAsync(response);

                await modal.RespondAsync(text: Utilities.FormatToCode("Votre commentaires à bien été envoyé à Slupshi !"), ephemeral:true);
            }
            catch(Exception ex)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", ex.Message);
                await modal.RespondAsync(text: Utilities.FormatToCode("Une erreur s'est produite lors de l'envoi de votre commentaire"), ephemeral: true);
                await Program.ZLog("Une erreur s'est produite avec la modal de commentaires YT");
            }          
            
        }
    }
}
