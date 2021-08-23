using System.IO;
using TShockAPI;

namespace AutoStop
{
    public class AutoStopConfig
    {
        public static string FilePath = Path.Combine(TShock.SavePath, "AutoStop.json");

        // Time in milliseconds before server stops after the last player leaves
        public int Delay = 600000;
    }
}
