using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace AutoExit
{
    [ApiVersion(2, 1)]
    public class AutoStop : TerrariaPlugin
    {
        public override string Author => "Kevin Meath";

        public override string Description => "" +
            "A TShock plugin that automatically stops the server while it's empty to save power.";

        public override string Name => "Auto Stop";

        public override Version Version => new Version(1, 0, 0, 0);

        public AutoStop(Main game) : base(game) {}

        public override void Initialize()
        {
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
                exitTimerThread.Start();
            }
        }

        private readonly Thread exitTimerThread = new Thread(ExitTimer);

        private static void ExitTimer()
        {
            Thread.Sleep(600000);
            TShock.Utils.StopServer();
        }
    }
}