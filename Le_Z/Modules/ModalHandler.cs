using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Le_Z.Modules
{
    public class ModalHandler
    {
        public static async Task YTCommentsModalSubmitted(SocketModal modal)
        {
            try
            {
                var message = (modal.Data.Components as SocketMessageComponentData[])[0].Value;

                string response = $"**{DateTime.Now.ToString("T")} | {modal.User.Username} sent :``` {message} ```**";
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
