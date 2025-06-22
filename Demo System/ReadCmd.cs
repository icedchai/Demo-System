using CommandSystem;
using DemoSystem.SnapshotHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ReadCmd : ICommand
    {
        public string[] Aliases => [];

        public string Description => "to read";

        public string Command => "readdemo";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string path = "recording-1";

            if (arguments.Count > 0)
            {
                path = arguments.At(0);
            }

            SnapshotReader.Singleton = new SnapshotReader(path);
            response = "Got it";
            return true;
        }
    }
}
