using System;
using System.Threading;
using TShockAPI;
using TShockAPI.Configuration;
using Terraria;
using TerrariaApi.Server;

namespace AutoStop
{
    [ApiVersion(2, 1)]
    public class AutoStop : TerrariaPlugin
    {
        public override string Author => "Kevin Meath";

        public override string Description => "A TShock plugin that automatically stops the server while it's empty to save power.";

        public override string Name => "Auto Stop";

        public override Version Version => new Version(1, 0, 0, 0);

        private ConfigFile<AutoStopConfig> config;

        public AutoStop(Main game) : base(game) {}

        public override void Initialize()
        {
            // Read config
            config = new ConfigFile<AutoStopConfig>();
            config.Read(AutoStopConfig.FilePath, out bool readFail);

            // Load default if read failed
            if (readFail)
            {
                config.Write(AutoStopConfig.FilePath);
                config.Settings.Delay = 600000;
            }

            // Register hooks
            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks
                ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);

                // Abort timer thread to prevent sleeping after the server is manually stopped
                exitTimerThread.Abort();
            }
            base.Dispose(disposing);
        }

        private void OnPlayerJoin(JoinEventArgs joinEventArgs)
        {
            if (exitTimerThread.IsAlive)
            {
                exitTimerThread.Abort();
            }
        }

        private void OnPlayerLeave(LeaveEventArgs leaveEventArgs)
        {
            if (TShock.Utils.GetActivePlayerCount() == 1)
            {
                exitTimerThread.Start(config.Settings.Delay);
            }
        }

        private readonly Thread exitTimerThread = new Thread(ExitTimer);

        private static void ExitTimer(object time)
        {
            Thread.Sleep((int) time);
            TShock.Utils.StopServer();
        }
    }
}