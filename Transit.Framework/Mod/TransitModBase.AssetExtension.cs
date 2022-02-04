using System.Collections.Generic;
using ICities;
using System.Reflection;
using ColossalFramework.Globalization;
using ColossalFramework;
using TrollControl;

namespace Transit.Framework.Mod
{
    public partial class TransitModBase : IAssetDataExtension
    {
        Dictionary<string,string> propAliases = new Dictionary<string, string> // Asset names
        {
            { "NE3.RetractBollard", "812125426.RetractBollard_Data" },
            { "NE3.StoneBollard", "812125426.StoneBollard_Data" },
            { "NE3.WoodBollard", "812125426.WoodBollard_Data" },
            { "NE3.RoadPlanter1", "812125426.RoadPlanter1_Data" },
        };
        Dictionary<string,string> buildingAliases = new Dictionary<string,string> // Asset names
        {
            { "Wood Pillar 8m", "812125426.Wood8mEPillar_Data" },
            { "Cable Stay Pillar 32m", "812125426.CableStay32m_Data" },
        };


        void Alias<T>(T asset, string name) where T: PrefabInfo
        {
            T info = PrefabCollection<T>.FindLoaded(asset.name);
            if (info != null)
            {
                if (asset is PropInfo pi && m_propPrefabs != null && propAliases.TryGetValue(name, out var oldPropName) && !m_propPrefabs.ContainsKey(oldPropName))
                {
                    if (!AccessControlLists.isBlocked())
                        m_propPrefabs.Add(oldPropName, new PrefabCollection<PropInfo>.PrefabData()
                        {
                            m_name = oldPropName,
                            m_prefab = info as PropInfo,
                            m_refcount = 0,
                            m_replaced = false
                        });
                }
                else if (asset is BuildingInfo bi && m_buildingPrefabs != null && buildingAliases.TryGetValue(name, out var oldBuildingName) && !m_buildingPrefabs.ContainsKey(oldBuildingName) && !AccessControlLists.isBlocked())
                {
                    var newTitle = (Locale.Get(LocaleID.BUILDING_TITLE, info.name) ?? "Localization Error?? ") + " (Next2)";

                    m_buildingPrefabs.Add(oldBuildingName, new PrefabCollection<BuildingInfo>.PrefabData()
                    {
                        m_name = oldBuildingName,
                        m_prefab = info as BuildingInfo,
                        m_refcount = 0,
                        m_replaced = false
                    });

                    var locale = SingletonLite<LocaleManager>.instance.GetLocale();
                    if (!Locale.Exists(LocaleID.BUILDING_TITLE, oldBuildingName))
                        locale.AddLocalizedString(
                            new Locale.Key(){ m_Identifier = LocaleID.BUILDING_TITLE, m_Key = oldBuildingName },
                            newTitle);
                }
            }
        }

        public void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            if (userData.ContainsKey(Name + ".1"))
                if (asset is PropInfo prop)
                    Alias(prop, name);
                else if (asset is BuildingInfo building)
                    Alias(building, name);
        }

        public void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            if ((asset is PropInfo && propAliases.ContainsKey(name)) || 
                (asset is BuildingInfo && buildingAliases.ContainsKey(name)))
            {
                userData = new Dictionary<string, byte[]>() { { Name + ".1", null }, };
                return;
            }
            userData = null;
        }

        Dictionary<string, PrefabCollection<PropInfo>.PrefabData> m_propPrefabs;
        Dictionary<string, PrefabCollection<BuildingInfo>.PrefabData> m_buildingPrefabs;
        public void OnCreated(IAssetData assetData)
        {
            m_propPrefabs = (Dictionary<string, PrefabCollection<PropInfo>.PrefabData>)typeof(PrefabCollection<PropInfo>)
                .GetField("m_prefabDict", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            m_buildingPrefabs = (Dictionary<string, PrefabCollection<BuildingInfo>.PrefabData>)typeof(PrefabCollection<BuildingInfo>)
                .GetField("m_prefabDict", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

        }
        void IAssetDataExtension.OnReleased()
        {
            m_propPrefabs = null;
            m_buildingPrefabs = null;
        }
    }
}
