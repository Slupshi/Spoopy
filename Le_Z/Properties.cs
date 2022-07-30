using System;
using System.Collections.Generic;
using System.Globalization;
using Discord;
using Discord.WebSocket;
using Le_Z.Modules;

namespace Le_Z
{
    public class Properties
    {
        #region Properties       

        public static SocketGuild Banquise;
        public static SocketRole UwuRole;
        public static IMessageChannel PollChannel;
        public static IMessageChannel BotLogChannel;
        public static IMessageChannel BotDMsChannel;

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

        #endregion

        public static void SetPropertiesAtStartup(DiscordSocketClient client)
        {
            Banquise = client.GetGuild(BanquiseID);
            UwuRole = Banquise.GetRole(UwuID);
            BotLogChannel = (IMessageChannel)client.GetChannel(969507287448301598);
            BotDMsChannel = (IMessageChannel)client.GetChannel(1002946943959433308);
            PollChannel = (IMessageChannel)client.GetChannel(997148190568611870);
        }

        public static Dictionary<string, Func<SocketSlashCommand, System.Threading.Tasks.Task>> SlashCommandsDico = new()
        {
            {"help", SlashCommands.HelpAsync },
            {"sondage", SlashCommands.CreatePoll },
            {"poll", SlashCommands.CreateComplexPoll },
            {"status", SlashCommands.StatusAsync },
            {"test", SlashCommands.TestAsync },

        };

    }    
}
