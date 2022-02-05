﻿using System.Collections.Generic;
using System.Linq;
using Transit.Framework;
using UnityEngine;
#if DEBUG
using Debug = Transit.Framework.Debug;
#endif
namespace Transit.Addon.RoadExtensions.Roads.PedestrianRoads.Common
{
    public static partial class ZonablePedestrianHelper
    {
        public static void AddWoodBollards(this NetInfo info, NetInfoVersion version)
        {
            var bollardName = "WoodBollard.NE3.WoodBollard"; 
            float? bollardOffset = null;
            float? pillarOffset = null;
            var bollardInfo = PrefabCollection<PropInfo>.FindLoaded($"{bollardName}_Data");
            if (bollardInfo == null)
                Debug.Log($"{info.name}: {bollardName} not found!");
            else
            {
                bollardOffset = 0;
            }
            BuildingInfo pillarInfo = null;

            if (version == NetInfoVersion.Elevated || version == NetInfoVersion.Bridge)
            {
                var pillarName = "Wood8mEPillar.Wood Pillar 8m";
                pillarInfo = PrefabCollection<BuildingInfo>.FindLoaded($"{pillarName}_Data");
                if (pillarInfo == null)
                {
                    Debug.Log($"{info.name}: {pillarName} not found!");
                }
            }
            info.AddBollards(version, bollardInfo, pillarInfo, pillarOffset, bollardOffset);
        }
        public static void AddRetractBollard(this NetInfo info, NetInfoVersion version)
        {
            var bollardName = "RetractBollard.NE3.RetractBollard";
            var bollardInfo = PrefabCollection<PropInfo>.FindLoaded($"{bollardName}_Data");
            if (bollardInfo == null)
            {
                Debug.Log($"{info.name}: {bollardName} not found!");
            }
            info.AddBollards(version, bollardInfo);

        }

        public static void AddStoneBollard(this NetInfo info, NetInfoVersion version)
        {
            var bollardName = "StoneBollard.NE3.StoneBollard";
            var bollardInfo = PrefabCollection<PropInfo>.FindLoaded($"{bollardName}_Data");
            if (bollardInfo == null)
            {
                Debug.Log($"{info.name}: {bollardName} not found!");
            }
            info.AddBollards(version, bollardInfo);
        }

        private static void AddBollards(this NetInfo info, NetInfoVersion version, PropInfo bollardInfo = null, BuildingInfo pillarInfo = null, float? bridgePillarYOffset = null, float? bollardYOffset = null)
        {
            if (version == NetInfoVersion.Bridge || version == NetInfoVersion.Elevated)
            {
                if (pillarInfo == null)
                {
                    pillarInfo = Prefabs.Find<BuildingInfo>("Wood Pillar 8m", false);
                }

                var bridgeAI = info.GetComponent<RoadBridgeAI>();
                if (pillarInfo != null && bridgeAI != null)
                {
                    bridgeAI.m_doubleLength = false;
                    bridgeAI.m_bridgePillarInfo = pillarInfo;
                    bridgeAI.m_middlePillarInfo = null;
                    if (bridgePillarYOffset != null)
                        bridgeAI.m_bridgePillarOffset = (float)bridgePillarYOffset;
                    else
                        bridgeAI.m_bridgePillarOffset = 0;
                }
            }

            if (bollardInfo != null)
            {
                var bollardProp1 = new NetLaneProps.Prop()
                {
                    m_prop = bollardInfo,
                    m_finalProp = bollardInfo,
                    m_probability = 100,
                    m_segmentOffset = 1,
                    m_minLength = 10,
                    m_endFlagsRequired = NetNode.Flags.Transition
                };
                bollardProp1.m_position.x = -3.5f;
                if (bollardYOffset != null)
                    bollardProp1.m_position.y = (float)bollardYOffset;
                else
                    bollardProp1.m_position.y = -0.3f;

                var bollardProp2 = bollardProp1.ShallowClone();
                bollardProp2.m_segmentOffset = -1;
                bollardProp2.m_endFlagsRequired = NetNode.Flags.None;
                bollardProp2.m_startFlagsRequired = NetNode.Flags.Transition;

                var bollardProp3 = bollardProp1.ShallowClone();
                bollardProp3.m_position.x = 3.5f;

                var bollardProp4 = bollardProp2.ShallowClone();
                bollardProp4.m_position.x = 3.5f;

                var bollardProp5 = bollardProp1.ShallowClone();
                bollardProp5.m_position.x = 0;

                var bollardProp6 = bollardProp2.ShallowClone();
                bollardProp6.m_position.x = 0;

                var propPedLane = info
                    .m_lanes
                    .First(l => l.m_position == 0f && l.m_laneType == NetInfo.LaneType.Pedestrian);

                propPedLane.m_laneProps.m_props = propPedLane
                    .m_laneProps
                    .m_props
                    .Union(new[] { bollardProp1, bollardProp2, bollardProp3, bollardProp4, bollardProp5, bollardProp6 })
                    .ToArray();
            }
        }
    }
}
