// project armada

namespace FSMController
{
	using System.Collections.Generic;
	using UnityEngine;

	[AddComponentMenu("Pathfinding/Connected Waypoint")]
	public partial class ConnectedWaypoint : Waypoint
	{


		private List<ConnectedWaypoint> _connections = new List<ConnectedWaypoint>();
		public List<ConnectedWaypoint> Connections => _connections;

        public void TryAddConnection(ConnectedWaypoint other)
        {
            RaycastHit hit;
            //public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction); 
            var result = Physics.Raycast(transform.position+Vector3.up*0.5f,other.transform.position-transform.position, out hit);

            if(result == false)
            {
                _connections.Add(other);
                other.Connections.Add(this);
            }
        }

		private void Start()
		{
			

		}

		public void DrawConnections()
		{
			foreach(var wp in _connections)
			{
				Debug.DrawRay(transform.position + Vector3.up, (wp.transform.position - transform.position) * (wp.transform.position - transform.position).magnitude, Color.green,10f);
			}
		}

	}
}


#if UNITY_EDITOR
namespace FSMController
{
	using UnityEngine;

	public partial class ConnectedWaypoint
	{
		//private const float _GIZMO_RADIUS = _connectivityRadius;
		private static readonly Color _GIZMO_COLOR = Color.blue;

		private void OnDrawGizmos()
		{
			Gizmos.color = _GIZMO_COLOR;
			Gizmos.DrawWireSphere(transform.position,1);
		}
	}
}
#endif