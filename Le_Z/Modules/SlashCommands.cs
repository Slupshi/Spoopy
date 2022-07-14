using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Le_Z.Modules
{
    public class SlashCommands
    {
        public static async Task CreatePoll(SocketSlashCommand command)
        {
            string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
            bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
            SocketUser author = command.User;

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(new Color(27, 37, 70))
                                    .WithTitle($"Sondage de {author.Username}")
                                    .WithThumbnailUrl(author.GetAvatarUrl())
                                    .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png");
            var embed = await Program.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
            await embed.AddReactionsAsync(Program.ThumbEmojis);
            await Program.ZLog($"Sondage crée par {author.Username}");
        }

        public static async Task CreateComplexPoll(SocketSlashCommand command)
        {
            string question = (string)(command.Data.Options.FirstOrDefault(x => x.Name == "question")?.Value);
            bool isEveryone = (command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value) == null ? false : (bool)(command.Data.Options.FirstOrDefault(x => x.Name == "everyone")?.Value);
            SocketUser author = command.User;

            var options = command.Data.Options.Where(x => x.Name.Contains("proposition")).ToList();

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(new Color(27, 37, 70))
                                    .WithTitle($"Sondage de {author.Username}")
                                    .WithThumbnailUrl(author.GetAvatarUrl())
                                    .WithDescription($"{question} {(question.Contains("?") ? string.Empty : "?")}")
                                    .WithFooter($"{DateTime.Now.ToString(@"HH\:mm")} • {DateTime.Now.ToString("dd MMMM, yyyy", Program.Culture)} ", iconUrl: "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png");
            List<Emoji> emojis = new List<Emoji>();
            foreach(var option in options)
            {
                string number = option.Name.Split('n').LastOrDefault();
                embedBuilder.AddField($"Proposition n°{number}", option.Value);

                emojis.Add(Program.NumberEmoji.FirstOrDefault(x => x.Key.ToString() == number).Value);
            }
            var embed = await Program.PollChannel.SendMessageAsync(text: $"{(isEveryone ? "@everyone" : string.Empty)}", embed: embedBuilder.Build());
            await embed.AddReactionsAsync(emojis);
            await Program.ZLog($"Sondage crée par {author.Username}");

        }
    }
}
