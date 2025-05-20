using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Harris.GPC
{

	[AddComponentMenu("CSharpBookCode/Utility/Waypoints Controller/BezierPathController")]

	public class BezierPathController : WaypointsController
	{
		[SerializeField]
		private Transform[] controlPoints;

		[SerializeField]
		private GameObject waypointPrefab;

		[SerializeField]
		private bool initialized = false;

		[SerializeField]
		private int numWayPointsMinusOne;
		public override void GetTransforms()
		{
			// we store all of the waypoints transforms in a List
			_transforms = new List<Transform>();
			int i = 4;

			//for(float t = 0; t <= 1; t += 0.1f)
			for(float t = 0; t <= 1.01f; t += 1.0f/numWayPointsMinusOne)
			{
				var goPos = Mathf.Pow(1 - t, 3) * controlPoints[0].position + 3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position + 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position + Mathf.Pow(t, 3) * controlPoints[3].position;
				
				if(!initialized)
				{
					var go = Instantiate(waypointPrefab, goPos, Quaternion.identity, transform);
				}
				
				else
				{
					transform.GetChild(i).transform.position = goPos;
				}
				i++;

			}
			initialized = true;

			// now we go through any transforms 'under' this transform, so all of
			// the child objects that act as our waypoints get put into our list
			foreach (Transform t in transform)
			{
				// add the waypoint to the list

				if(t.gameObject.tag == "Waypoint")
					_transforms.Add(t);
			}

			totalTransforms = (int)_transforms.Count;

			Debug.Log("There are " + totalTransforms + " Waypoints in the scene!");
		}
	}
}