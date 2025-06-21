using Exiled.API.Interfaces;

namespace DemoSystem
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        public bool RecordVoiceChat { get; set; } = false;
    }
}
