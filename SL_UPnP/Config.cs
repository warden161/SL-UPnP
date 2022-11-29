using System.Diagnostics;
using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SL_UPnP
{
    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        [Description("All debug levels can be found at: https://github.com/warden161/SL_UPnP/blob/master/README.md#Debug")]
        public SourceLevels DebugLevel { get; set; } = SourceLevels.Warning;
        public string MappingName { get; set; } = "scpsl:%port%";
        public int Timeout { get; set; } = 5000;
    }
}