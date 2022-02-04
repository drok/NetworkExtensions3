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
                // GetOriginal() was introduced at 8c845dd between 2.0.4.0 and 2.1.0.0, and exists in 2.2.0.0
                var key = harmonyAssembly.GetType("HarmonyLib.HarmonySharedState").GetMethod("GetOriginal",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var signedBy = ByteArrayToHexString(harmonyAssembly.GetName().GetPublicKeyToken());

                /* Support only Harmony (redesigned) and Harmony 2.0.4-4 (May 5, 2021 version)
                 * Colossal Order's imminent Harmony 2.2 breaks compatibility in subtle ways,
                 * and in obvious ways with Harmony (redesigned), which is based on 2.0.4.0
                 * 
                 * Harmony 2.2 invites moders to start using 2.2 features in their mods,
                 * which will happen. These mods will start trickling into the workshop.
                 * 
                 * A mod which requires 2.2 features will not load on a 2.0.4.0 library,
                 * like Harmony (redesigned), so this means that as more 2.2 mods trickle
                 * in, there will be a growing number of TypeLoadExceptions for users
                 * who chose Harmony (redesigned).
                 * 
                 * Also, Colossal's 2.2 library uses fake version number to make it
                 * look like 2.0.4.0. This means mods that start using 2.2 features
                 * will be looking for a library with 2.0.4.0 in its version.
                 * 
                 * A legitimate 2.0.4.0 (not one with counterfit version) does not
                 * contain the 2.2 features that those mods want, and will of course
                 * cause the 2.2 mod to fail.
                 * 
                 * In effect, it will appear that a mod with 2.2 features runs fine
                 * with Colossal's Harmony 2.2, but fails to run with Harmony (redesigned).
                 * 
                 * This colossal order plan is an attack not only on Harmony (redesigned)
                 * because it makes it incompatible with the upcoming crop of mods,
                 * but also on the end users who will be left trying to figure out the mess.
                 * 
                 * It is also an attack on the modders who try using 2.2 features, because
                 * their mods will be asking for a 2.0.4.0 library, when in fact they need
                 * a 2.2 library.
                 * 
                 * Of course, old mods that reference 2.0.4.0 and expect 2.0.4.0 are
                 * indistinguishable from the new mods that also reference 2.0.4.0 but in fact
                 * crash when running on 2.0.4.0
                 * 
                 * Even if Harmony (redesigned) were somehow switched to 2.2, the version number
                 * mess where it's impossible to tell what version of library a mod actually
                 * requires means that the workshop is in for a long cancer.
                 * 
                 * I will have no part in this, and when/if my Harmony mod is updated to a newer
                 * Harmony library, it will use correct version numbers, and provide backwards
                 * compatibility and a clean way to indicate what features it actually supports.
                 * 
                 * This code is a version lock to cause NE3 (this mod) to fail unambiguously
                 * when run in the presence of Collossal's version mess. A clean, obvious
                 * failure is much easier to deal with than subtle errors involving
                 * multiple seemingly unrelated components. So I choose a clean break.
                 * 
                 * It will run correctly with any version of Harmony (redesigned), and
                 * with versions of Collossal's Harmony only up to and including 2.0.4.
                 * Some older versions were broken, so in reality only the non-broken
                 * Collossal versions will work with this mod.
                 * 
                 * I'm hoping that Colossal will re-think their Harmony 2.2 plans
                 * and not implement them for the time being.
                 * 
                 * What is being done here could be considered a mistake when done by
                 * a minecraft kid, but I am an engineer. I know this is wrong, and if
                 * I were to follow suit, I would not be making a mistake, I'd be
                 * criminally unethical, because I know better and I'm trained and
                 * expected to know better.
                 * 
                 * Long story short: If you chose to fork this software in order to
                 * remove the roadblock below, you knowingly committing something.
                 * I suggest consulting a lawyer before touching this code.
                 */
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
