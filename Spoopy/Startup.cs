using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;
using Discord;
using Newtonsoft.Json;
using Spoopy.Variables;
using Discord.WebSocket;

namespace Spoopy
{
    public class Startup
    {
        public static async Task CreateSlashCommands(DiscordSocketClient client)
        {
            // Test
            var testCommand = new SlashCommandBuilder();
            testCommand.WithName(CommandsConstantes.TestSlashCommandName);
            testCommand.WithDescription("A ne pas utiliser");
            testCommand.WithDefaultMemberPermissions(GuildPermission.Administrator);

            // Help
            var helpCommand = new SlashCommandBuilder();
            helpCommand.WithName(CommandsConstantes.HelpSlashCommandName);
            helpCommand.WithDescription("De l'aide pour les gens perdus !");

            // Status
            var statusCommand = new SlashCommandBuilder();
            statusCommand.WithName(CommandsConstantes.StatusSlashCommandName);
            statusCommand.WithDescription("Pour stalk les gens du serveur");
            statusCommand.AddOption(CommandsConstantes.CommandParameterUsername, ApplicationCommandOptionType.User, "La personne stalkée (Vous même par défaut)", isRequired: false);

            // Avatar
            var avatarCommand = new SlashCommandBuilder();
            avatarCommand.WithName(CommandsConstantes.AvatarSlashCommandName);
            avatarCommand.WithDescription("Récupère l'avatar d'un membre du serveur");
            avatarCommand.AddOption(CommandsConstantes.CommandParameterUsername, ApplicationCommandOptionType.User, "La personne dont l'avatar sera prélevé (Vous même par défaut)", isRequired: false);
            avatarCommand.AddOption(name: CommandsConstantes.CommandParameterSize,
               type: ApplicationCommandOptionType.Integer,
               description: "La taille de l'image en pixel (512x512 par défaut)",
               isRequired: false,
               choices: new ApplicationCommandOptionChoiceProperties[]
               {
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "2048x2048",
                       Value = 2048,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "1024x1024",
                       Value = 1024,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "512x512",
                       Value = 512,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "256x256",
                       Value = 256,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "128x128",
                       Value = 128,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "64x64",
                       Value = 64,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "32x32",
                       Value = 32,
                   },
                   new ApplicationCommandOptionChoiceProperties
                   {
                       Name = "16x16",
                       Value = 16,
                   },
               });

            // RandomOrg
            var randomCommand = new SlashCommandBuilder();
            randomCommand.WithName(CommandsConstantes.RandomSlashCommandName);
            randomCommand.WithDescription("Selectionne un nombre random entre le nombre de départ et de fin");
            randomCommand.AddOption(CommandsConstantes.CommandParameterStart, ApplicationCommandOptionType.Integer, "Le nombre de départ | 1 par défaut", isRequired: false, minValue: 1);
            randomCommand.AddOption(CommandsConstantes.CommandParameterEnd, ApplicationCommandOptionType.Integer, "Le nombre de fin", isRequired: true, minValue: 2);
            randomCommand.AddOption(CommandsConstantes.CommandParameterVisible, ApplicationCommandOptionType.Boolean, "Détermine si la réponse est visible par tout le monde", isRequired: false);

            // SpoopyStatus
            var spoopyStatus = new SlashCommandBuilder();
            spoopyStatus.WithName(CommandsConstantes.SpoopyStatusSlashCommandName);
            spoopyStatus.WithDescription("Obtenir le status du bot");

            // Shuffle
            var shuffleCommand = new SlashCommandBuilder();
            shuffleCommand.WithName(CommandsConstantes.ShuffleSlashCommandName);
            shuffleCommand.WithDescription("Mélange les lettres d'une chaine de charactères");
            shuffleCommand.AddOption(CommandsConstantes.CommandParameterStart, ApplicationCommandOptionType.String, "La chaîne de charactères qui sera mélanger", isRequired: true);
            shuffleCommand.AddOption(CommandsConstantes.CommandParameterVisible, ApplicationCommandOptionType.Boolean, "Détermine si la réponse est visible par tout le monde", isRequired: false);

            // Basic Poll
            var pollCommand = new SlashCommandBuilder();
            pollCommand.WithName(CommandsConstantes.SondageSlashCommandName);
            pollCommand.WithDescription("Création de sondage");
            pollCommand.AddOption(CommandsConstantes.CommandParameterQuestion, ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true);
            pollCommand.AddOption(CommandsConstantes.CommandParameterEveryone, ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué (False par défaut)", isRequired: false);
            pollCommand.AddOption(CommandsConstantes.CommandParameterDuration, ApplicationCommandOptionType.Integer, "Défini la durée d'un sondage, en heures (24h par défaut)", isRequired: false
            //    ,choices: new ApplicationCommandOptionChoiceProperties[]
            //{
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "30sec",
            //        Value = TimeSpan.FromSeconds(30),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "15 minutes",
            //        Value = TimeSpan.FromMinutes(15),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "30 minutes",
            //        Value = TimeSpan.FromMinutes(30),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "1 heure",
            //        Value = TimeSpan.FromHours(1),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "2 heures",
            //        Value = TimeSpan.FromHours(2),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "6 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "12 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "24 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "1 semaine",
            //        Value = TimeSpan.FromDays(7),
            //    },
            //}
            );

            // Complex Poll
            var complexPollCommand = new SlashCommandBuilder();
            complexPollCommand.WithName(CommandsConstantes.PollSlashCommandName);
            complexPollCommand.WithDescription("Création de sondage à choix multiples");
            complexPollCommand.AddOption(CommandsConstantes.CommandParameterQuestion, ApplicationCommandOptionType.String, "La question qui sera posée", isRequired: true)
                .AddOption(CommandsConstantes.CommandParameterEveryone, ApplicationCommandOptionType.Boolean, "Défini si un @everyone est effectué", isRequired: false)
                .AddOption("proposition1", ApplicationCommandOptionType.String, "Proposition n°1", isRequired: true)
                .AddOption("proposition2", ApplicationCommandOptionType.String, "Proposition n°2", isRequired: true)
                .AddOption("proposition3", ApplicationCommandOptionType.String, "Proposition n°3", isRequired: false)
                .AddOption("proposition4", ApplicationCommandOptionType.String, "Proposition n°4", isRequired: false)
                .AddOption("proposition5", ApplicationCommandOptionType.String, "Proposition n°5", isRequired: false)
                .AddOption("proposition6", ApplicationCommandOptionType.String, "Proposition n°6", isRequired: false)
                .AddOption("proposition7", ApplicationCommandOptionType.String, "Proposition n°7", isRequired: false)
                .AddOption("proposition8", ApplicationCommandOptionType.String, "Proposition n°8", isRequired: false)
                .AddOption("proposition9", ApplicationCommandOptionType.String, "Proposition n°9", isRequired: false);
            complexPollCommand.AddOption(CommandsConstantes.CommandParameterDuration, ApplicationCommandOptionType.Integer, "Défini la durée d'un sondage, en heures (24h par défaut)", isRequired: false
            //    ,choices: new ApplicationCommandOptionChoiceProperties[]
            //{
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "30sec",
            //        Value = TimeSpan.FromSeconds(30),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "15 minutes",
            //        Value = TimeSpan.FromMinutes(15),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "30 minutes",
            //        Value = TimeSpan.FromMinutes(30),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "1 heure",
            //        Value = TimeSpan.FromHours(1),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "2 heures",
            //        Value = TimeSpan.FromHours(2),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "6 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "12 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "24 heures",
            //        Value = TimeSpan.FromHours(6),
            //    },
            //    new ApplicationCommandOptionChoiceProperties
            //    {
            //        Name = "1 semaine",
            //        Value = TimeSpan.FromDays(7),
            //    },
            //}
            );

            // FakeBan
            var fakeBanCommand = new SlashCommandBuilder();
            fakeBanCommand.WithName(CommandsConstantes.FakeBanSlashCommandName);
            fakeBanCommand.WithDescription("Ban une personne du serveur");
            fakeBanCommand.AddOption(CommandsConstantes.CommandParameterUsername, ApplicationCommandOptionType.User, "La personne qui sera ban", isRequired: false);

            // GameRole
            var gameRoleCommand = new SlashCommandBuilder();
            gameRoleCommand.WithName(CommandsConstantes.GameSlashCommandName);
            gameRoleCommand.WithDescription("Obtiens le role du jeu qui correspondant à ce channel !");

            // StartStream
            var streamCommand = new SlashCommandBuilder();
            streamCommand.WithName(CommandsConstantes.StreamSlashCommandName);
            streamCommand.WithDescription("Annonce un stream");
            streamCommand.WithDefaultMemberPermissions(GuildPermission.Administrator);
            streamCommand.AddOption(CommandsConstantes.StreamStartCommandParameterGame, ApplicationCommandOptionType.String, "Le jeu au quel le stream est dédié", isRequired: true);
            streamCommand.AddOption(CommandsConstantes.StreamStartCommandParameterStreamer, ApplicationCommandOptionType.User, "Le streameur", isRequired: true);

            try
            {
                var applicationCommands = await client.GetGlobalApplicationCommandsAsync();

                if (applicationCommands != null && applicationCommands.Any())
                {
                    foreach (var applicationCommand in applicationCommands)
                    {
                        await applicationCommand.DeleteAsync();
                    }
                }

                await client.CreateGlobalApplicationCommandAsync(helpCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(statusCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(avatarCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(randomCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(spoopyStatus.Build());
                await client.CreateGlobalApplicationCommandAsync(shuffleCommand.Build());

                await Properties.Banquise.CreateApplicationCommandAsync(pollCommand.Build());
                await Properties.Banquise.CreateApplicationCommandAsync(complexPollCommand.Build());
                await Properties.Banquise.CreateApplicationCommandAsync(fakeBanCommand.Build());

                await Properties.TeykhoServer.CreateApplicationCommandAsync(gameRoleCommand.Build());
                await Properties.TeykhoServer.CreateApplicationCommandAsync(streamCommand.Build());

                await Properties.TestServer.CreateApplicationCommandAsync(testCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }
        }

        public static async Task CreateContextMenuCommands(DiscordSocketClient client)
        {
            var avatarUserCommand = new UserCommandBuilder();
            //var avatarMessageCommand = new MessageCommandBuilder();

            avatarUserCommand.WithName(CommandsConstantes.UserAvatarUserCommandName);
            //avatarMessageCommand.WithName("avatar");

            await client.BulkOverwriteGlobalApplicationCommandsAsync(new ApplicationCommandProperties[]
            {
                //avatarMessageCommand.Build(),
                avatarUserCommand.Build(),
            });


        }
    }
}
