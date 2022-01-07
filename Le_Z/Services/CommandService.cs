using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Le_Z.Services
{
    public class CommandService : IDisposable
    {
        public IEnumerable<CommandInfo> Commands { get; }
        public ILookup<Type, TypeReader> TypeReaders { get; }
        public IEnumerable<ModuleInfo> Modules { get; }

        public CommandService()
        {

        }
        public CommandService(CommandServiceConfig config)
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
