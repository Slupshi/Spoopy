using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;
using Spoopy.Variables;

namespace Spoopy.Jobs
{
    public class BasicPollsJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            long messageId = (long)context.Trigger.JobDataMap.FirstOrDefault(x => x.Key == "id").Value;

            IMessage message = await Properties.PollChannel.GetMessageAsync((ulong)messageId);
            if (message != null)
            {
                IEmbed pollEmbed = message.Embeds.FirstOrDefault();
                List<KeyValuePair<IEmote, ReactionMetadata>> reactions = message.Reactions.ToList();
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle($"Résultat du {pollEmbed.Title}");
                embed.WithDescription($"{Format.Bold("Question :")} {pollEmbed.Description}");
                embed.WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.QuestionMarkURL);
                embed.WithColor(new Color(27, 37, 70));
                embed.WithThumbnailUrl(pollEmbed.Thumbnail.Value.Url);

                double yesCount = reactions.FirstOrDefault(x => x.Key.Name.Contains("👍")).Value.ReactionCount - 1;
                double noCount = reactions.FirstOrDefault(x => x.Key.Name.Contains("👎")).Value.ReactionCount - 1;

                embed.AddField("Oui", $"{yesCount}", inline: true);
                embed.AddField("Non", $"{noCount}", inline: true);

                if (yesCount > 0 || noCount > 0) 
                {
                    double sum = yesCount + noCount;

                    double yesPourcentage = yesCount / sum * 100f;
                    double noPourcentage = noCount / sum * 100f;

                    string yesBar = new string('\u25A0', (int)(yesPourcentage / 10));
                    string noBar = new string('\u25A1', (int)(noPourcentage / 10));

                    embed.AddField("Resultat", $"{yesPourcentage}%   {yesBar}{noBar}   {noPourcentage}%");
                }
                else
                {
                    embed.AddField("Resultat", "Aucun votant");
                }

                IUserMessage resultMessage = await Properties.PollChannel.SendMessageAsync(embed: embed.Build());
                await message.DeleteAsync();

            }
        }
    }

    public class ComplexPollJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            long messageId = (long)context.Trigger.JobDataMap.FirstOrDefault(x => x.Key == "id").Value;

            IMessage message = await Properties.PollChannel.GetMessageAsync((ulong)messageId);
            if (message != null)
            {
                IEmbed pollEmbed = message.Embeds.FirstOrDefault();
                List<KeyValuePair<IEmote, ReactionMetadata>> reactions = message.Reactions.ToList();
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle($"Résultat du {pollEmbed.Title}");
                embed.WithDescription($"{Format.Bold("Question :")} {pollEmbed.Description}");
                embed.WithFooter(Utilities.GetCustomTimestamp(), iconUrl: Constantes.QuestionMarkURL);
                embed.WithColor(new Color(27, 37, 70));
                embed.WithThumbnailUrl(pollEmbed.Thumbnail.Value.Url);

                //TODO
            }
        }
    }
}
