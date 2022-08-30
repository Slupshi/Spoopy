using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Spoopy.Modules;

namespace Spoopy
{
    public class Properties
    {
        #region Properties       

        public static SocketGuild TestServer;

        public static SocketGuild Banquise;
        public static SocketRole UwuRole;
        public static SocketRole TkiToiRole;
        public static IMessageChannel PollChannel;
        public static IMessageChannel BotLogChannel;
        public static IMessageChannel BotDMsChannel;
        public static IMessageChannel YoutubeVideoChannel;
        public static IMessageChannel HelloChannel;
        public static IMessageChannel TestChannel;

        public static bool HadEmmerderZozoToday = false;

        #endregion


        #region Constantes

        public readonly static CultureInfo Culture = new CultureInfo("fr-FR");

        public static Color WhiteColor { get => new Color(255, 255, 255); }
        public static Color GreenColor { get => new Color(19, 121, 16); }
        public static Color RedColor { get => new Color(219, 78, 78); }

        public const long SlupID = 429352632138858506;
        public const long UwuID = 964621092562034718;
        public const long BanquiseID = 611568951406624768;

        public static readonly List<Emoji> ThumbEmojis = new() { new Emoji("👍"), new Emoji("👎") };
        public static readonly Dictionary<byte, Emoji> NumberEmoji = new()
        {
            {1,  new Emoji("1️⃣") }, { 2, new Emoji("2️⃣") }, { 3, new Emoji("3️⃣") }, { 4, new Emoji("4️⃣") }, { 5, new Emoji("5️⃣") }, { 6, new Emoji("6️⃣") }, { 7, new Emoji("7️⃣") }, { 8, new Emoji("8️⃣") }, { 9, new Emoji("9️⃣") },
        };
        public static readonly List<char> Vowels = new()
        {
            'A', 'E', 'Y', 'U', 'I', 'O'
        };

        public const string YoutubeLogoURL = "https://assets.stickpng.com/thumbs/580b57fcd9996e24bc43c545.png";
        public const string SpotifyLogoURL = "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png";
        public const string TwitterLogoURL = "https://www.freepnglogos.com/uploads/twitter-logo-png/twitter-icon-circle-png-logo-8.png";
        public const string ControllerIconURL = "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png";
        public const string QuestionMarkURL = "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png";
        public const string InfoIconURL = "https://e7.pngegg.com/pngimages/359/338/png-clipart-logo-information-library-business-information-miscellaneous-blue-thumbnail.png";

        public const GatewayIntents GatewayPrivleges = GatewayIntents.Guilds |
                GatewayIntents.GuildBans |
                GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageReactions |
                GatewayIntents.GuildVoiceStates |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildIntegrations |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildPresences;

        #endregion

        public static void SetPropertiesAtStartup(DiscordSocketClient client)
        {
            TestServer = client.GetGuild(1007315790283952188);
            Banquise = client.GetGuild(BanquiseID);
            UwuRole = Banquise.GetRole(UwuID);
            TkiToiRole = Banquise.GetRole(611571904901414921);
            BotLogChannel = (IMessageChannel)client.GetChannel(969507287448301598);
            BotDMsChannel = (IMessageChannel)client.GetChannel(1002946943959433308);
            PollChannel = (IMessageChannel)client.GetChannel(997148190568611870);
            YoutubeVideoChannel = (IMessageChannel)client.GetChannel(694662332953133148);
            TestChannel = (IMessageChannel)client.GetChannel(930095425292238879);
            HelloChannel = (IMessageChannel)client.GetChannel(611619367469449227);
        }

        public static Dictionary<string, Func<SocketSlashCommand,Task>> SlashCommandsDico = new()
        {
            {"test", SlashCommands.TestAsync },
            {"help", SlashCommands.HelpAsync },
            {"status", SlashCommands.StatusAsync },
            {"avatar", SlashCommands.GetUserAvatarAsync },
            {"sondage", SlashCommands.CreatePoll },
            {"poll", SlashCommands.CreateComplexPoll },
            {"ban", SlashCommands.FakeBanAsync }

        };

        public static Dictionary<string, Func<SocketMessageComponent,Task>> ButtonHandlersDico = new()
        {
            {"ytCommentButton", ButtonHandler.OpenYTCommentsModal },
        };

        public static Dictionary<string, Func<SocketModal,Task>> ModalHandlersDico = new()
        {
            {"ytCommentModal", ModalHandler.YTCommentsModalSubmitted },
        };


    }    
}
