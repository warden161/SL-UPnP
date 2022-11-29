using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;
using Open.Nat;
using Exiled.API.Features;

namespace SL_UPnP
{
    public class Plugin : Plugin<Config>
    {
        public override string Author { get; } = "warden161";
        public override string Name { get; } = "SL UPnP Tool";
        public override string Prefix { get; } = "sl_upnp";
        public override Version Version { get; } = new Version(0, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(5, 3, 1);

        public override void OnEnabled()
        {
            NatDiscoverer.TraceSource.Switch.Level = Config.DebugLevel;
            NatDiscoverer.TraceSource.Listeners.Add(new Logger());
            Task.Run(Forward);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            NatDiscoverer.ReleaseAll();
            base.OnDisabled();
        }

        public async Task Forward()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cancelation = new CancellationTokenSource(Config.Timeout);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cancelation);

                var mapping = new Mapping(Protocol.Udp, Server.Port, Server.Port, int.MaxValue, Config.MappingName.Replace("%port%", Server.Port.ToString()));
                if (mapping == null)
                {
                    Log.Error("Couldn't port forward: Mapping is null.");
                    return;
                }

                await device.CreatePortMapAsync(mapping);
            } catch (NatDeviceNotFoundException)
            {
                Log.Error("Couldn't port forward: No UPnP-supported devices were found.");
                return;
            } catch (MappingException e)
            {
                switch (e.ErrorCode)
                {
                    case 718:
                        Log.Error("Couldn't port forward: The port is already being forwarded.");
                        return;
                    case 728:
                        Log.Error("Couldn't port forward: The mapping table for your router is already full.");
                        return;
                }
            }
        }

        class Logger : TraceListener
        {
            private void MessageHandler(string message)
            {
                if (message == null)
                    return;

                Log.Info(message);
            }

            public override void Write(string message)
                => MessageHandler(message);
            public override void WriteLine(string message)
                => MessageHandler(message);
        }
    }
}