using System.IO;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ICities;
using Transit.Framework;
using Transit.Framework.Modularity;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;
using ColossalFramework.Plugins;
using System.Linq;
using System;
using System.Collections.Generic;
using HarmonyLib;
using CitiesHarmony.API;

#if DEBUG
using Debug = Transit.Framework.Debug;
#endif

namespace Transit.Framework.Mod
{
    public abstract partial class TransitModBase : IUserMod
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string Version { get; }

        public virtual string DefaultFolderPath
        {
            get { return Name; }
        }

        public virtual string AssetPath => PluginInfo.modPath;
        private static PluginInfo PluginInfo
        {
            get
            {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        var instances = item.GetInstances<IUserMod>();
                        if (!(instances.FirstOrDefault() is TransitModBase))
                        {
                            continue;
                        }
                        return item;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("Failed to find NetworkExtensions assembly!");

            }
        }

        bool m_hasHarmonyConflict = false;

        protected bool HasCompatibleHarmony()
        {
            if (m_hasHarmonyConflict)
                return false;

            if (HarmonyHelper.IsHarmonyInstalled)
            {
                var harmonyAssembly = typeof(Harmony).Assembly;
                object key = null;
                var signedBy = ByteArrayToHexString(harmonyAssembly.GetName().GetPublicKeyToken());

                if (signedBy != "8625d3dcfab56202" && key != null)
                {
                    m_hasHarmonyConflict = true;
                }
            }
            return !m_hasHarmonyConflict;
        }

        static string ByteArrayToHexString(byte[] bytes)
        {
            return string.Join(string.Empty, Array.ConvertAll(bytes, b => b.ToString("x2")));
        }

    }
}
