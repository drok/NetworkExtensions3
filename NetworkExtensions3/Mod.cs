using System;
using ColossalFramework;
using ColossalFramework.Plugins;
using Transit.Framework;
using Transit.Framework.Mod;
using Transit.Framework.Modularity;
using ColossalFramework.PlatformServices;

namespace NetworkExtensions
{
    public sealed partial class Mod : TransitModBase
    {
        public override string Name
        {
            get { return "Network Extensions 3 " + Version; }
        }

        public override string Description
        {
            get { return
                    HasCompatibleHarmony() ? "Rome wasn't built in a day. Neither was this mod. Enjoy." :
                    "The installed Harmony is not compatible. Please use 2.0.4 or Harmony (redesigned)"; }
        }

        public override string Version
        {
            get { return "1.0.2.1"; }
        }

    }
}
