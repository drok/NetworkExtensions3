using ICities;
using System.Diagnostics;
using Transit.Framework.Modularity;
using Transit.Framework.Prerequisites;

namespace Transit.Framework.Mod
{
    public abstract partial class TransitModBase : LoadingExtensionBase
    {
        public virtual void OnEnabled()
        {
            if (!HasCompatibleHarmony())
                return;
            ModPrerequisites.InstallForMod(this);
            LoadModulesIfNeeded();
            LoadSettings();

            foreach (IModule module in Modules)
                module.OnEnabled();
        }

        public virtual void OnDisabled()
        {
            if (!HasCompatibleHarmony())
                return;
            foreach (IModule module in Modules)
                module.OnDisabled();

            ModPrerequisites.UninstallForMod(this);
        }

        public override void OnLevelUnloading()
        {
            if (!HasCompatibleHarmony())
                return;
            foreach (IModule module in Modules)
                module.OnLevelUnloading();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (!HasCompatibleHarmony())
                return;
            foreach (IModule module in Modules)
                module.OnLevelLoaded(mode);
        }
    }
}
