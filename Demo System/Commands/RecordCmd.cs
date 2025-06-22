using CommandSystem;
using DemoSystem.SnapshotHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class RecordCmd : ICommand
    {
        public string[] Aliases => [];

        public string Description => "to record";

        public string Command => "record";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SnapshotRecorder recorder = Plugin.Singleton.Recorder;

            if (recorder is null || recorder.Disposed)
            {
                Plugin.Singleton.Recorder = new SnapshotRecorder();
                recorder = Plugin.Singleton.Recorder;
            }

            if (recorder.IsRecording)
            {
                recorder.StopRecording();
            }
            else
            {
                recorder.StartRecording();
            }
                response = "Got it";
            return true;
        }
    }
}
