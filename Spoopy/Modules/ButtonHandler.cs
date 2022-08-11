using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Spoopy.Modules
{
    public class ButtonHandler
    {
        public static async Task OpenYTCommentsModal(SocketMessageComponent arg)
        {
            try
            {
                var input = new TextInputBuilder();
                input.WithCustomId("ytCommentInput")
                    .WithRequired(true)
                    .WithStyle(TextInputStyle.Paragraph)
                    .WithLabel("Votre avis")
                    .WithPlaceholder("N'hésitez pas à donner des conseils/idées pour les futures vidéos !");

                var modal = new ModalBuilder();
                modal.WithCustomId("ytCommentModal")
                    .WithTitle("Commenter")
                    .AddTextInput(input);

                await arg.RespondWithModalAsync(modal.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine("**Une erreur s'est produite : {0}**", ex.Message);
                await arg.RespondAsync(Utilities.FormatToCode("Une erreur s'est produite, veuillez réessayer ultérieurement"), ephemeral: true);
                await Program.ZLog("Une erreur s'est produite avec la modal de commentaires YT");
            }
            
        }
    }
}
