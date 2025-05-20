using UnityEngine;

namespace AIStates
{
	public enum AIState
	{
		idle,
		stopped_turning_left,
		stopped_turning_right,
		stopped_turning_towards_target,
		searchTarget,
		paused_no_target,
		paused_looking_for_target,
		move_along_waypoint_path,
		//move_looking_for_target,
		chase_avoid_obstacle,
		move_forward,
		chasing_target,
		backing_up_looking_for_target,
		attackTarget
	}
}
