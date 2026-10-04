namespace SLGameLogger
{
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int SavePlayerPositionPerTicks { get; set; } = 1;
        public int FlushDataPerTicks { get; set; } = 320;
        public string ListenHost { get; set; } = "localhost";
        public int Port { get; set; } = 8080;
        public string Motd { get; set; } = "SCP Server";
        public int MaxConcurrentRequests { get; set; } = 8;
    }
}