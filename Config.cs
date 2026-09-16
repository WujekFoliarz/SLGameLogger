namespace SLGameLogger
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int SavePlayerPositionPerTicks { get; set; } = 1;
        public int FlushDataPerTicks { get; set; } = 320;
    }
}