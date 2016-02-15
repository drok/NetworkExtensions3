﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework;
using Transit.Addon.ToolsV2.LaneRouting.Data;
using Transit.Framework.ExtensionPoints.PathFinding;
using UnityEngine;

namespace Transit.Addon.ToolsV2.LaneRouting
{
    public class LaneRoutingManager : Singleton<LaneRoutingManager>, ILaneRoutingManager
    {
        private const ushort LANEROUTING_CONTROL_BIT = 2048;

        private static IDictionary<uint, NodeRoutingData> _routingData;

        public static void SetLoadedData(IEnumerable<NodeRoutingData> loadedData)
        {
            _routingData = new Dictionary<uint, NodeRoutingData>();
            foreach (var data in loadedData.Where(d => d.IsRelevant()))
            {
                _routingData[data.NodeId] = data;
                UpdateArrows(data);
            }
        }

        public static IEnumerable<NodeRoutingData> GetAllData()
        {
            if (_routingData == null)
            {
                throw new Exception("Routing has not been initialized/loaded yet");
            }

            return _routingData.Values;
        }

        public static NodeRoutingData GetOrCreateData(ushort nodeId)
        {
            if (_routingData == null)
            {
                throw new Exception("Routing has not been initialized/loaded yet");
            }

            if (!_routingData.ContainsKey(nodeId))
            {
                var newData = new NodeRoutingData {NodeId = nodeId};
                Monitor.Enter(_routingData);
                try
                {
                    _routingData[nodeId] = newData;
                }
                finally
                {
                    Monitor.Exit(_routingData);
                }
            }

            return _routingData[nodeId];
        }

        public static void AddRoute(NodeRoutingData nodeRouting, LaneRoutingData route)
        {
            if (!nodeRouting.Routes.Contains(route))
            {
                NetManager.instance.AddLaneFlag(LANEROUTING_CONTROL_BIT, route.OriginLaneId);
                Monitor.Enter(nodeRouting.Routes);
                try
                {
                    nodeRouting.Routes.Add(route);
                }
                finally
                {
                    Monitor.Exit(nodeRouting.Routes);
                }
            }

            UpdateArrows(nodeRouting);
        }

        public static void RemoveRoute(NodeRoutingData nodeRouting, LaneRoutingData route)
        {
            if (nodeRouting.Routes.Contains(route))
            {
                Monitor.Enter(nodeRouting.Routes);
                try
                {
                    nodeRouting.Routes.Remove(route);
                }
                finally
                {
                    Monitor.Exit(nodeRouting.Routes);
                }

                if (nodeRouting.Routes.Count == 0)
                {
                    NetManager.instance.RemoveLaneFlag(LANEROUTING_CONTROL_BIT, route.OriginLaneId);
                }
            }

            UpdateArrows(nodeRouting);
        }

        public static void VerifyRoutes(NodeRoutingData nodeRouting)
        {
            ICollection<LaneRoutingData> obsoleteRoutes = null;

            foreach (var route in nodeRouting.Routes)
            {
                var originLane = NetManager.instance.GetLane(LANEROUTING_CONTROL_BIT, route.OriginLaneId);
                if (originLane == null)
                {
                    if (obsoleteRoutes == null)
                    {
                        obsoleteRoutes = new HashSet<LaneRoutingData>();
                    }
                    obsoleteRoutes.Add(route);
                    continue;
                }

                var destinationLane = NetManager.instance.GetLane(LANEROUTING_CONTROL_BIT, route.DestinationLaneId);
                if (destinationLane == null)
                {
                    if (obsoleteRoutes == null)
                    {
                        obsoleteRoutes = new HashSet<LaneRoutingData>();
                    }
                    obsoleteRoutes.Add(route);
                    continue;
                }
            }

            if (obsoleteRoutes != null)
            {
                Monitor.Enter(nodeRouting.Routes);
                try
                {
                    foreach (var route in obsoleteRoutes)
                    {
                        nodeRouting.Routes.Remove(route);
                    }
                }
                finally
                {
                    Monitor.Exit(nodeRouting.Routes);
                }
            }
        }

        public static void UpdateArrows(NodeRoutingData nodeRouting)
        {
            var node = NetManager.instance.GetNode(nodeRouting.NodeId);
            if (node == null)
            {
                return;
            }

            SetDefaultArrows(nodeRouting);
            SetRoutedArrows(nodeRouting);
        }

        public static void SetDefaultArrows(NodeRoutingData nodeRouting)
        {
            var node = NetManager.instance.GetNode(nodeRouting.NodeId);
            if (node == null)
            {
                return;
            }

            foreach (var segmentId in node.Value.GetSegments())
            {
                var segment = NetManager.instance.GetSegment(segmentId);
                if (segment == null)
                {
                    continue;
                }

                var segmentValue = segment.Value;
                var info = segmentValue.Info;
                info.m_netAI.UpdateLanes(segmentId, ref segmentValue, false);
            }
        }

        public static void SetRoutedArrows(NodeRoutingData nodeRouting)
        {
            var node = NetManager.instance.GetNode(nodeRouting.NodeId);
            if (node == null)
            {
                return;
            }

            foreach (var group in nodeRouting.Routes.GroupBy(r => r.GetOriginUniqueId()))
            {
                var originSegmentId = group.First().OriginSegmentId;
                var originSegment = NetManager.instance.GetSegment(originSegmentId);
                if (originSegment == null)
                {
                    continue;
                }

                var originLaneId = group.First().OriginLaneId;
                var originLane = NetManager.instance.GetLane(LANEROUTING_CONTROL_BIT, originLaneId);
                if (originLane == null)
                {
                    continue;
                }

                var originSegmentDirection = originSegment.Value.GetDirection(nodeRouting.NodeId);
                var originLaneArrowFlags = NetLane.Flags.None;
                
                foreach (var route in group)
                {
                    var destSegmentId = route.DestinationSegmentId;
                    var destSegment = NetManager.instance.GetSegment(destSegmentId);
                    if (destSegment == null)
                    {
                        continue;
                    }

                    var destSegmentDirection = destSegment.Value.GetDirection(nodeRouting.NodeId);
                    if (Vector3.Angle(originSegmentDirection, destSegmentDirection) > 150f)
                    {
                        originLaneArrowFlags |= NetLane.Flags.Forward;
                    }
                    else
                    {
                        if (Vector3.Dot(Vector3.Cross(originSegmentDirection, -destSegmentDirection), Vector3.up) > 0f)
                        {
                            originLaneArrowFlags |= NetLane.Flags.Right;
                        }
                        else
                        {
                            originLaneArrowFlags |= NetLane.Flags.Left;
                        }
                    }
                }

                if (originLaneArrowFlags != NetLane.Flags.None)
                {
                    NetManager.instance.m_lanes.m_buffer[originLaneId].m_flags = 
                        (ushort)(
                            ((NetLane.Flags)originLane.Value.m_flags &
                            ~NetLane.Flags.LeftForwardRight) |
                            originLaneArrowFlags);
                }
            }
        }

        public bool CanLanesConnect(ushort nodeId, uint laneId1, uint laneId2)
        {
            if (_routingData == null)
            {
                throw new Exception("Routing has not been initialized/loaded yet");
            }

            if (!_routingData.ContainsKey(nodeId))
            {
                return true;
            }


            NodeRoutingData nodeRouting;
            Monitor.Enter(_routingData);
            try
            {
                nodeRouting = _routingData[nodeId];
            }
            finally
            {
                Monitor.Exit(_routingData);
            }


            LaneRoutingData[] laneRoutes;
            Monitor.Enter(nodeRouting.Routes);
            try
            {
                laneRoutes = nodeRouting
                    .Routes
                    .Where(r => r.OriginLaneId == laneId1)
                    .ToArray();
            }
            finally
            {
                Monitor.Exit(nodeRouting.Routes);
            }


            if (!laneRoutes.Any())
            {
                return true;
            }
            else
            {
                return laneRoutes.Any(r => r.DestinationLaneId == laneId2);
            }
        }

        //bool static FindNode(NetSegment segment)
        //{
        //    uint laneId = segment.m_lanes;
        //    NetInfo info = segment.Info;
        //    int laneCount = info.m_lanes.Length;
        //    int laneIndex = 0;
        //    for (; laneIndex < laneCount && laneId != 0; laneIndex++)
        //    {
        //        if (laneId == m_laneId)
        //            break;
        //        laneId = NetManager.instance.m_lanes.m_buffer[laneId].m_nextLane;
        //    }

        //    if (laneIndex < laneCount)
        //    {
        //        NetInfo.Direction laneDir = ((segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? info.m_lanes[laneIndex].m_finalDirection : NetInfo.InvertDirection(info.m_lanes[laneIndex].m_finalDirection);

        //        if ((laneDir & (NetInfo.Direction.Forward | NetInfo.Direction.Avoid)) == NetInfo.Direction.Forward)
        //            m_nodeId = segment.m_endNode;
        //        else if ((laneDir & (NetInfo.Direction.Backward | NetInfo.Direction.Avoid)) == NetInfo.Direction.Backward)
        //            m_nodeId = segment.m_startNode;

        //        return true;
        //    }

        //    return false;
        //}
    }
}