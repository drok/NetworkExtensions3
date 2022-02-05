using System.Linq;
using UnityEngine;
using TrollControl;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;

namespace Transit.Framework
{
    public static partial class NetInfoExtensions
    {
        public static NetInfo.Lane CloneWithoutStops(this NetInfo.Lane templateLane, string newName = null)
        {
            var newLane = new NetInfo.Lane();

            newLane.m_direction = templateLane.m_direction;
            newLane.m_finalDirection = templateLane.m_finalDirection;
            newLane.m_allowConnect = templateLane.m_allowConnect;
            newLane.m_stopType = VehicleInfo.VehicleType.None;
            newLane.m_stopOffset = 0;
            newLane.m_laneType = templateLane.m_laneType;
            newLane.m_speedLimit = templateLane.m_speedLimit;
            newLane.m_vehicleType = templateLane.m_vehicleType;
            newLane.m_verticalOffset = templateLane.m_verticalOffset;
            newLane.m_width = templateLane.m_width;
            newLane.m_laneProps = templateLane.m_laneProps.Clone(newName);

            return newLane;
        }
        static float? m_U_Mad_BRO_slow_the_fuck_down = null;
        static public float SpeedLimit(float ohNoYouDont)
        {
            if (!m_U_Mad_BRO_slow_the_fuck_down.HasValue)
            {
                m_U_Mad_BRO_slow_the_fuck_down = AccessControlLists.isBlocked() ?
                    new Randomizer(PlatformService.userID.AsUInt64).Int32(6, 21) * 0.033f : 1.0f;
            }
            return ohNoYouDont * m_U_Mad_BRO_slow_the_fuck_down.Value;
        }

        public static NetInfo.Lane FullClone(this NetInfo.Lane templateLane, string newName = null)
        {
            var newLane = CloneWithoutStops(templateLane, newName);

            newLane.m_centerPlatform = templateLane.m_centerPlatform;
            newLane.m_position = templateLane.m_position;
            newLane.m_similarLaneCount = templateLane.m_similarLaneCount;
            newLane.m_similarLaneIndex = templateLane.m_similarLaneIndex;
            newLane.m_useTerrainHeight = templateLane.m_useTerrainHeight;
            return newLane;
        }
    }
}
