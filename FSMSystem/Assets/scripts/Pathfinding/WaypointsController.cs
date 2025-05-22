using UnityEngine;
using Harris.GPC;
using System;
using System.Collections.Generic;

namespace FSMController
{
    [ExecuteAlways]
    public class WaypointsController : MonoBehaviour
    {
        [SerializeField]
        private List<ConnectedWaypoint> _waypoints;
        public List<ConnectedWaypoint> Waypoints => _waypoints;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {
            ConnectWaypoints();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ConnectWaypoints()
        {
            foreach (var wp in _waypoints)
            {
                foreach (var other in _waypoints)
                {
                    if (wp == other)
                        continue;
                    if (other.Connections.Contains(wp))
                        continue;
                    wp.TryAddConnection(other);
                }
            }
        }
    }
}