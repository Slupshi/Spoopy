using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Le_Z.Modules;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TwitterSharp.Response.RTweet;



namespace Le_Z
{
    public class Program
    {     
        private static DiscordSocketClient _client;
        private CommandService _commands;
        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug, GatewayIntents = GatewayIntents.All });
            _client.Log += Log;

            _commands = new CommandService();
            await InstallCommandsAsync();

            await _client.SetGameAsync("UwU | /help");
            _client.Ready += _client_Ready;

            var token = Environment.GetEnvironmentVariable("DiscordBot_LE_Z", EnvironmentVariableTarget.User);
            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();
            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
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
            if (message.Attachments.Count > 0 && message.Author.Id == Properties.SlupID)
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
                
                await botResponse.AddReactionsAsync(Properties.ThumbEmojis);
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

        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            await Properties.SlashCommandsDico.FirstOrDefault(x => x.Key == command.CommandName).Value.Invoke(command);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static async Task ZLog(string message, bool isError = false)
        {
            await Properties.BotLogChannel.SendMessageAsync($"**```{(isError ? "fix" : string.Empty)}{Environment.NewLine}{DateTime.Now.ToString("T")} | {message}```**");
        }        

        private async Task LatencyUpdated(int previousLatency, int newLatency)
        {
            await ExternalInteractions.SetGameRoleAsync();
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousVoiceState, SocketVoiceState newVoiceState)
        {
            await ExternalInteractions.UwUAsync(user, newVoiceState);
        }

        private Task _client_Ready()
        {
            Properties.SetPropertiesAtStartup(_client);

            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.LatencyUpdated += LatencyUpdated;
            
            CreateSlashCommands();

            _client.SlashCommandExecuted += HandleSlashCommandAsync;

            return Task.CompletedTask;
        }

        private Task CreateSlashCommands()
        {
            // Basic Poll
            var pollCommand = new SlashCommandBuilder();
            pollCommand.WithName("sondage");
            pollCommand.WithDescription("Création de sondage");
            pollCommand.AddOption("question", ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true);
            pollCommand.AddOption("everyone", ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué", isRequired: false);
            pollCommand.AddOption("persistant", ApplicationCommandOptionType.Boolean, "Défini si un sondage est infini ou non", isRequired: false);

            // Complex Poll
            var complexPollCommand = new SlashCommandBuilder();
            complexPollCommand.WithName("poll");
            complexPollCommand.WithDescription("Création de sondage à choix multiples");
            complexPollCommand.AddOption("question", ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true)
                .AddOption("everyone", ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué", isRequired: false)
                .AddOption("persistant", ApplicationCommandOptionType.Boolean, "Défini si un sondage est infini ou non", isRequired: false)
                .AddOption("proposition1", ApplicationCommandOptionType.String, "Proposition n°1", isRequired: true)
                .AddOption("proposition2", ApplicationCommandOptionType.String, "Proposition n°2", isRequired: true)
                .AddOption("proposition3", ApplicationCommandOptionType.String, "Proposition n°3", isRequired: false)
                .AddOption("proposition4", ApplicationCommandOptionType.String, "Proposition n°4", isRequired: false)
                .AddOption("proposition5", ApplicationCommandOptionType.String, "Proposition n°5", isRequired: false)
                .AddOption("proposition6", ApplicationCommandOptionType.String, "Proposition n°6", isRequired: false)
                .AddOption("proposition7", ApplicationCommandOptionType.String, "Proposition n°7", isRequired: false)
                .AddOption("proposition8", ApplicationCommandOptionType.String, "Proposition n°8", isRequired: false)
                .AddOption("proposition9", ApplicationCommandOptionType.String, "Proposition n°9", isRequired: false);

            // Help
            var helpCommand = new SlashCommandBuilder();
            helpCommand.WithName("help");
            helpCommand.WithDescription("De l'aide pour les gens perdus !");
            helpCommand.AddOption("standard", ApplicationCommandOptionType.Boolean, "Si True, cela affichera les anciennes commandes", isRequired: false);

            // Status
            var statusCommand = new SlashCommandBuilder();
            statusCommand.WithName("status");
            statusCommand.WithDescription("Pour stalk les gens du serveur");
            statusCommand.AddOption("username", ApplicationCommandOptionType.User, "La personne stalkée", isRequired: false);

            try
            {
                Properties.Banquise.CreateApplicationCommandAsync(pollCommand.Build());
                Properties.Banquise.CreateApplicationCommandAsync(complexPollCommand.Build());
                Properties.Banquise.CreateApplicationCommandAsync(helpCommand.Build());
                Properties.Banquise.CreateApplicationCommandAsync(statusCommand.Build());
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }

            return Task.CompletedTask;
        }       

    }
}
