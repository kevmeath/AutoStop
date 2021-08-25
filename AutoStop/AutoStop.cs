using System;
using System.Threading;
using System.Threading.Tasks;
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
            }

            // Register hooks
            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGameReady);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks
                ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGameReady);

                cancellationTokenSource.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnGameReady(EventArgs eventArgs)
        {
            // Check if the plugin is configured to stop the server before the first player joins
            if (config.Settings.StopBeforeFirstJoin)
            {
                ScheduleServerShutdown();
            }
        }

        private void OnPlayerJoin(JoinEventArgs joinEventArgs)
        {
            // Cancel shutdown if it hasn't already been cancelled
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private void OnPlayerLeave(LeaveEventArgs leaveEventArgs)
        {
            // Schedule server shutdown if the last player leaves
            // Check for 1 player rather than 0 becuase this is called just before the player leaves
            if (TShock.Utils.GetActivePlayerCount() == 1)
            {
                ScheduleServerShutdown();
            }
        }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void ScheduleServerShutdown()
        {
            // Replace old cancelled token source
            cancellationTokenSource = new CancellationTokenSource();

            // Shutdown the server after the configured time
            Task.Delay(config.Settings.Delay, cancellationTokenSource.Token)
                .ContinueWith(t => TShock.Utils.StopServer(), cancellationTokenSource.Token);
        }
    }
}