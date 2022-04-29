using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Le_Z.Modules;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TwitterSharp.Response.RTweet;

namespace Le_Z
{
    public class Program
    {
        private byte _isStarting = 1;
        private SocketGuildChannel _botLogChannel;
        private DiscordSocketClient _client;
        private CommandService _commands;
        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug, GatewayIntents = GatewayIntents.All });
            _client.Log += Log;

            _commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("UwU");

            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);

            //Starting the bot
            await _client.StartAsync();
            await Task.Delay(-1);


        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.UserVoiceStateUpdated += UwU;
            _client.LatencyUpdated += LatencyUpdated;
            //_client.SlashCommandExecuted
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = (SocketUserMessage)msg;
            #region TriggerWords
            if (message == null) return;
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
            //
            #endregion TriggerWords
            if (message.Attachments.Count > 0 && message.Author.Id == 429352632138858506)
            {
                MessageReference messageRef = new MessageReference(messageId: message.Id);
                await message.Channel.SendMessageAsync("**Les sources de ce message ne sont surement pas valide**", messageReference: messageRef);
            }
            if (message.Content.Contains("https://twitter.com/") && message.Content.Contains("/status/"))
            {
                var splitMessage = message.Content.Split('/');
                var splitID = splitMessage.Last().Split('?');
                var tweet = (Tweet)await Commands.UseTwitterClientAsync(method: Commands.TwitterClientMethod.GetTweet, id: splitID.First());
                var embedBuilder = await Commands.CreateTweetEmbedAsync(tweet, title: $"{message.Author.Username} partage un tweet");
                await message.DeleteAsync();
                var botResponse = await message.Channel.SendMessageAsync(embed: embedBuilder.Build());
                Emoji[] emojis = new Emoji[2];
                emojis[0] = new Emoji("👍");
                emojis[1] = new Emoji("👎");
                await botResponse.AddReactionsAsync(emojis);
                return;
            }
            int argPos = 0;
            if (message.Content.Contains("http")) return;
            if (!message.HasStringPrefix("z!", ref argPos)) return;
            var context = new SocketCommandContext(_client, message);
            await context.Guild.DownloadUsersAsync();
            var result = await _commands.ExecuteAsync(context, argPos, null);
            if (!result.IsSuccess)
            {
                MessageReference messageRef = new MessageReference(messageId: message.Id);
                await context.Channel.SendMessageAsync("**Je ne reconnais pas cette commande**", messageReference: messageRef);
            }

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task UwU(SocketUser user, SocketVoiceState previousVoiceState, SocketVoiceState newVoiceState)
        {
            if (newVoiceState.VoiceChannel == null || newVoiceState.VoiceChannel.Guild.Id != 611568951406624768) return;
            SocketRole UwU = _client.GetGuild(611568951406624768).GetRole(964621092562034718);
            bool isUwU = newVoiceState.VoiceChannel.Id == 611881198700068864;

            if (isUwU)
            {
                if (!(user as IGuildUser).RoleIds.Any(r => r == 964621092562034718))
                {
                    await (user as IGuildUser).AddRoleAsync(UwU);
                }
            }
            else
            {
                if ((user as IGuildUser).RoleIds.Any(r => r == 964621092562034718))
                {
                    await (user as IGuildUser).RemoveRoleAsync(UwU);
                }
            }
        }

        private async Task LatencyUpdated(int previousLatency, int newLatency)
        {
            if (_isStarting == 1)
            {
                _isStarting = 2;
                return;
            }
            if (_isStarting == 2)
            {
                _botLogChannel = _client.GetGuild(611568951406624768).Channels.Where(c => c.Id == 969507287448301598).First();
                _isStarting = 0;
            }
            await SetGameRoleAsync();
        }

        private async Task SetGameRoleAsync()
        {
            try
            {
                SocketGuild guild = _client.GetGuild(611568951406624768);
                var rolesList = guild.Roles.ToList();
                var activeUsers = guild.Users.Where(u => u.Activities.Count > 0 && u.IsBot == false).ToList();
                foreach (var user in activeUsers)
                {
                    var game = user.Activities.FirstOrDefault(a => a.Type == ActivityType.Playing);
                    if (game != null)
                    {
                        SocketRole role = rolesList.FirstOrDefault(r => r.Name == game.Name);
                        if (role == null)
                        {
                            RestRole restRole = await guild.CreateRoleAsync(name: game.Name, isMentionable: true, color: new Color(255, 255, 255));
                            await (_botLogChannel as IMessageChannel).SendMessageAsync($"**```{DateTime.Now.ToString("T")} | Rôle {restRole.Name} créé avec succès```**");
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}
