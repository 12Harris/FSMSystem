// project armada

#pragma warning disable 0414

using UnityEngine;
using System;
namespace Harris.GPC
{
	public class ObstacleWaypointsController : WaypointsController
	{

		public BaseAIController aiController;

		private bool initialized = false;

		public override void Start()
		{
			base.Start();
			//aiController = GetComponent<BaseAIController>();
		}

		private void Update()
		{

			if (aiController.reachedLastWaypoint)
			{
				//_onReachedLastWaypoint?.Invoke();
			}
		}

	}
}