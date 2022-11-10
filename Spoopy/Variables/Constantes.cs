using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Spoopy.Variables
{
    public class Constantes
    {
        #region Constantes

        public readonly static CultureInfo Culture = new CultureInfo("fr-FR");

        #region Colors

        public static Color WhiteColor { get => new Color(255, 255, 255); }
        public static Color GreenColor { get => new Color(19, 121, 16); }
        public static Color RedColor { get => new Color(219, 78, 78); }

        #endregion

        #region IDs

        public const long UwuID = 964621092562034718;

        public const long BanquiseID = 611568951406624768;
        public const long TeykhoID = 1020030635294859397;

        public const long SlupID = 429352632138858506;
        public const long ZozoID = 434662109595435008;
        public const long KeyuFuuID = 451665479912914954;

        #endregion

        public static readonly List<Emoji> ThumbEmojis = new() { new Emoji("👍"), new Emoji("👎") };
        public static readonly Dictionary<byte, Emoji> NumberEmojis = new()
        {
            {1,  new Emoji("1️⃣") }, { 2, new Emoji("2️⃣") }, { 3, new Emoji("3️⃣") }, { 4, new Emoji("4️⃣") }, { 5, new Emoji("5️⃣") }, { 6, new Emoji("6️⃣") }, { 7, new Emoji("7️⃣") }, { 8, new Emoji("8️⃣") }, { 9, new Emoji("9️⃣") },
        };
        public static readonly List<char> Vowels = new()
        {
            'A', 'E', 'Y', 'U', 'I', 'O'
        };

        #region Logos

        public const string YoutubeLogoURL = "https://assets.stickpng.com/thumbs/580b57fcd9996e24bc43c545.png";
        public const string SpotifyLogoURL = "https://www.freepnglogos.com/uploads/spotify-logo-png/spotify-logo-vector-download-11.png";
        public const string TwitterLogoURL = "https://www.freepnglogos.com/uploads/twitter-logo-png/twitter-icon-circle-png-logo-8.png";
        public const string ControllerIconURL = "https://icons.iconarchive.com/icons/paomedia/small-n-flat/512/gamepad-icon.png";
        public const string QuestionMarkURL = "https://www.nicepng.com/png/full/181-1816226_blue-question-mark-clipart-question-mark-icon-blue.png";
        public const string InfoIconURL = "https://e7.pngegg.com/pngimages/359/338/png-clipart-logo-information-library-business-information-miscellaneous-blue-thumbnail.png";

        #endregion

        public const GatewayIntents GatewayPrivileges = GatewayIntents.Guilds |
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
    }
}
