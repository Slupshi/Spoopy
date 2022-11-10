using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Spoopy.Modules;
using Spoopy.Services;

namespace Spoopy.Variables
{
    public class Properties
    {
        #region Properties       

        public static SocketGuild TestServer;

        public static SocketGuild TeykhoServer;
        public static SocketRole ViewerRole;
        public static SocketRole MHWRole;
        public static IMessageChannel StreamChannel;

        public static SocketGuild Banquise;
        public static SocketRole UwuRole;
        public static SocketRole TkiToiRole;
        public static IMessageChannel PollChannel;
        public static IMessageChannel BotLogChannel;
        public static IMessageChannel BotDMsChannel;
        public static IMessageChannel YoutubeVideoChannel;
        public static IMessageChannel HelloChannel;
        public static IMessageChannel TestChannel;

        public static SocketUser Slupshi;

        public static bool isLocalApiRunning;

        #endregion      

        #region Dictionaries

        public readonly static Dictionary<string, Func<SocketSlashCommand, Task>> SlashCommandsDico = new()
        {
            {CommandsConstantes.TestSlashCommandName, SlashCommands.TestAsync },
            {CommandsConstantes.HelpSlashCommandName, SlashCommands.HelpAsync },
            {CommandsConstantes.StatusSlashCommandName, SlashCommands.StatusAsync },
            {CommandsConstantes.AvatarSlashCommandName, SlashCommands.GetUserAvatarAsync },
            {CommandsConstantes.RandomSlashCommandName, SlashCommands.RandomOrgAsync },
            {CommandsConstantes.SpoopyStatusSlashCommandName, SlashCommands.PrintSpoopyStatusAsync },
            {CommandsConstantes.ShuffleSlashCommandName, SlashCommands.ShuffleAsync },
            {CommandsConstantes.SondageSlashCommandName, SlashCommands.CreatePollAsync },
            {CommandsConstantes.PollSlashCommandName, SlashCommands.CreateComplexPollAsync },
            {CommandsConstantes.FakeBanSlashCommandName, SlashCommands.FakeBanAsync },
            {CommandsConstantes.GameSlashCommandName, SlashCommands.GetGameRoleAsync },
            {CommandsConstantes.StreamSlashCommandName, SlashCommands.StartStreamAsync },
        };

        public readonly static Dictionary<string, Func<SocketUserCommand, Task>> UserCommandsDico = new()
        {
            {"User Avatar", UserCommands.GetUserAvatarAsync },
        };

        public readonly static Dictionary<string, Func<SocketMessageCommand, Task>> MessageCommandsDico = new()
        {
        };

        public readonly static Dictionary<string, Func<SocketMessageComponent, Task>> ButtonHandlersDico = new()
        {
            {"ytCommentButton", ButtonHandler.OpenYTCommentsModalAsync },
            {"errorContactButton", ButtonHandler.OpenErrorContactModalAsync },
        };

        public readonly static Dictionary<string, Func<SocketModal, Task>> ModalHandlersDico = new()
        {
            {"ytCommentModal", ModalHandler.YTCommentsModalSubmittedAsync },
            {"errorContactModal", ModalHandler.ErrorContactModalSubmittedAsync },
        };

        public static Dictionary<string, SocketRole> TeykhoGameRoleDico = new()
        {
            {"monster-hunter-world", MHWRole }
        };

        public static Dictionary<long, string> TeykhoStreamers = new()
        {
            {Constantes.SlupID, "slupshi" },
            {Constantes.ZozoID, "teykhoo" },
            {Constantes.KeyuFuuID, "keyufuu" },
        };

        public static Queue<string> BotActivities = new Queue<string>();

        #endregion


        public static void SetPropertiesAtStartup(DiscordSocketClient client)
        {
            TestServer = client.GetGuild(1007315790283952188);

            Banquise = client.GetGuild(Constantes.BanquiseID);
            UwuRole = Banquise.GetRole(Constantes.UwuID);
            TkiToiRole = Banquise.GetRole(611571904901414921);
            BotLogChannel = (IMessageChannel)client.GetChannel(969507287448301598);
            BotDMsChannel = (IMessageChannel)client.GetChannel(1002946943959433308);
            PollChannel = (IMessageChannel)client.GetChannel(997148190568611870);
            YoutubeVideoChannel = (IMessageChannel)client.GetChannel(694662332953133148);
            TestChannel = (IMessageChannel)client.GetChannel(930095425292238879);
            HelloChannel = (IMessageChannel)client.GetChannel(611619367469449227);

            TeykhoServer = client.GetGuild(Constantes.TeykhoID);
            ViewerRole = TeykhoServer.GetRole(1020035350187487252);
            MHWRole = TeykhoServer.GetRole(1026167859036037163);
            TeykhoGameRoleDico["monster-hunter-world"] = MHWRole;
            StreamChannel = (IMessageChannel)TeykhoServer.GetChannel(1022093143094607873);

            Slupshi = client.GetUser(Constantes.SlupID);

            isLocalApiRunning = LocalApiService.Ping();
        }

        public static void SetupBotActivities()
        {
            BotActivities.Enqueue("/help");
        }

    }
}
