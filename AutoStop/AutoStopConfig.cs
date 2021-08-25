using System.IO;
using TShockAPI;

namespace AutoStop
{
    public class AutoStopConfig
    {
        public static string FilePath = Path.Combine(TShock.SavePath, "AutoStop.json");

        // Time in milliseconds before server stops after the last player leaves
        public int Delay = 600000;

        // Stop server if no player joins within the delay time
        public bool StopBeforeFirstJoin = false;
    }
}
