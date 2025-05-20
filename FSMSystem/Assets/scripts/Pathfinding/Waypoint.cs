// project armada

namespace FSMController
{
	using UnityEngine;
	using System.Collections.Generic;

	public abstract partial class Waypoint : MonoBehaviour
	{

		//use this to get all connected waypoints in the scene
		public static T[] FindWaypoints<T>() where T : Waypoint
		{
			var r = new List<T>();
			foreach(var p in _points)
			{
				if(p is T) { r.Add(p as T); }
			}
			return r.ToArray();
		}

		// all points in scene
		private static List<Waypoint> _points = new List<Waypoint>();

		private void OnEnable()
		{
			_points.Add(this);
		}

		private void OnDisable()
		{
			_points.Remove(this);
		}
	}
}

#if UNITY_EDITOR
namespace FSMController
{
	using UnityEngine;

	public partial class Waypoint
	{
		private const float _GIZMO_RADIUS = 1f;
		private static readonly Color _GIZMO_COLOR = Color.blue;

		private void OnDrawGizmos()
		{
			Gizmos.color = _GIZMO_COLOR;
			Gizmos.DrawWireSphere(transform.position, _GIZMO_RADIUS);
		}
	}
}
#endif
