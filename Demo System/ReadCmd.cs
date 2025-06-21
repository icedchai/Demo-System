using CommandSystem;
using DemoSystem.SnapshotHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_System
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ReadCmd : ICommand
    {
        public string[] Aliases => [];

        public string Description => "to read";

        public string Command => "readdemo";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SnapshotReader.Singleton = new SnapshotReader();
            response = "Got it";
            return true;
        }
    }
}
