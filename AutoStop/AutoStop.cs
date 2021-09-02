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

        public override Version Version => new Version(1, 1, 0, 0);

        private ConfigFile<AutoStopConfig> config;

        private ScheduledShutdown scheduledShutdown;

        public AutoStop(Main game) : base(game) { }

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

            // Check for invalid delay
            if (config.Settings.Delay < 0)
            {
                Console.WriteLine("[AutoStop] Delay must be greater than or equal to 0. Resetting to default value.");
                config.Settings.Delay = 600000;
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

                scheduledShutdown.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnGameReady(EventArgs eventArgs)
        {
            // Check if the plugin is configured to stop the server before the first player joins
            if (config.Settings.StopBeforeFirstJoin)
            {
                ScheduleShutdown();
            }
        }

        private void OnPlayerJoin(JoinEventArgs joinEventArgs)
        {
            // Cancel shutdown when a player joins
            if (scheduledShutdown != null)
            {
                scheduledShutdown.Cancel();
                scheduledShutdown.Dispose();
            }
        }

        private void OnPlayerLeave(LeaveEventArgs leaveEventArgs)
        {
            // Schedule server shutdown if the last player leaves
            // Check for 1 player rather than 0 becuase this is called just before the player leaves
            if (TShock.Utils.GetActivePlayerCount() == 1)
            {
                ScheduleShutdown();
            }
        }

        private void ScheduleShutdown()
        {
            scheduledShutdown = new ScheduledShutdown(config.Settings.Delay);
        }
    }

    // Class for scheduled shutdowns with IDisposable to avoid wasting resources with canceled shutdowns
    public class ScheduledShutdown : IDisposable
    {
        // Cancellation token for cancelling the shutdown when a player joins
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Shutdown the server after the given delay (milliseconds)
        public ScheduledShutdown(int delay)
        {
            Task.Delay(delay, cancellationTokenSource.Token)
                .ContinueWith(t => TShock.Utils.StopServer(), cancellationTokenSource.Token);
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        
        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }
    }
}