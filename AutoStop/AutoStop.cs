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

        public override string Name => "AutoStop";

        public override Version Version => new(1, 2, 0, 0);

        private ConfigFile<AutoStopConfig> config;

        private int delay;

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
                TShock.Log.ConsoleInfo("Delay must be greater than or equal to 0. Resetting to default value.");
                config.Settings.Delay = 600000;
            }

            delay = config.Settings.Delay;

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

				if (scheduledShutdown != null)
				{
					CancelScheduledShutdown();
				}
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
            // Cancel shutdown when a player joins after shutdown has been scheduled
            if (scheduledShutdown != null)
            {
                CancelScheduledShutdown();
				TShock.Log.ConsoleInfo("Scheduled server shutdown cancelled.");
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
            TShock.Log.ConsoleInfo($"Server is empty, shutting down after {delay} milliseconds.");
            scheduledShutdown = new ScheduledShutdown(delay);
        }

        private void CancelScheduledShutdown()
		{
			scheduledShutdown.Cancel();
			scheduledShutdown.Dispose();
        }
    }

    // Class for scheduled shutdowns with IDisposable to avoid wasting resources with canceled shutdowns
    public class ScheduledShutdown : IDisposable
    {
        // Cancellation token for cancelling the shutdown when a player joins
        private readonly CancellationTokenSource cancellationTokenSource = new();

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
			GC.SuppressFinalize(this);
		}
    }
}