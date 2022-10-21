using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Spoopy.Modules
{
    public class ButtonHandler
    {
        public static async Task OpenYTCommentsModalAsync(SocketMessageComponent arg)
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
                modal.WithCustomId($"ytCommentModal|{arg.Data.CustomId.Split('|').Last()}")
                    .WithTitle("Commenter")
                    .AddTextInput(input);

                await arg.RespondWithModalAsync(modal.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", ex.Message);
                await arg.RespondAsync(Utilities.FormatToCode("Une erreur s'est produite, veuillez réessayer ultérieurement"), ephemeral: true);
                await Utilities.SpoopyLogAsync("Une erreur s'est produite avec la modal de commentaires YT");
            }
            
        }

        public static async Task OpenErrorContactModalAsync(SocketMessageComponent arg)
        {
            try
            {
                var input = new TextInputBuilder();
                input.WithCustomId("errorContactInput")
                    .WithRequired(true)
                    .WithStyle(TextInputStyle.Paragraph)
                    .WithLabel("Message")
                    .WithPlaceholder("Pouvez-vous préciser le détails de l'erreur ?");

                var modal = new ModalBuilder();
                modal.WithCustomId($"errorContactModal")
                    .WithTitle("Contacter le dev !")
                    .AddTextInput(input);

                await arg.RespondWithModalAsync(modal.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {0}", ex.Message);
                await arg.RespondAsync(Utilities.FormatToCode("Une erreur s'est produite, veuillez réessayer ultérieurement"), ephemeral: true);
                await Utilities.SpoopyLogAsync("Une erreur s'est produite avec la modal de contact d'erreur");
            }
        }
    }
}
