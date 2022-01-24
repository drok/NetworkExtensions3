using System;
using ColossalFramework;
using ColossalFramework.Plugins;
using Transit.Framework;
using Transit.Framework.Mod;
using Transit.Framework.Modularity;

namespace NetworkExtensions
{
    public sealed partial class Mod : TransitModBase
    {
        public override string Name
        {
            get { return "Network Extensions 3"; }
        }

        public override string Description
        {
            get { return "All roads lead to Rome."; }
        }

        public override string Version
        {
            get { return "1.0.0"; }
        }

    }
}
