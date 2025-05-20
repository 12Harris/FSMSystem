using UnityEngine;
using AIStates;
using System.Collections.Specialized;
//using Harris.Util;
using System;


namespace Harris.GPC
{
	[AddComponentMenu("CSharpBookCode/Base/AI Controller")]

	public class BaseAIController : ExtendedCustomMonoBehaviour
	{
		public bool AIControlled;

		[Header("AI States")]
		public AIState currentAIState;
		public AIState targetAIState;


		[Header("Enemy AI movement values")]

		public float horz;
		public float vert;
		public float modelRotateSpeed = 15f;
		public int _followTargetMaxTurnAngle = 120;
		public float minChaseDistance = 1f;
		public float maxChaseDistance = 10f;
		public float visionHeightOffset = 1f;

		[Header("Waypoints")]
		// waypoint following related variables
		public WaypointsController current_waypointsController;

		public Transform _currentWaypointTransform;
		public Transform _followTarget;
		public Transform _rotateTransform;

		[System.NonSerialized]
		public bool reachedLastWaypoint;
		public float waypointDistance = .2f;
		public float pathSmoothing = 2f;
		public bool shouldReversePathFollowing;
		public bool loopPath;
		public bool destroyAtEndOfWaypoints;
		public bool startAtFirstWaypoint;

		[Header("Layer masks and raycasting")]
		public LayerMask obstacleAvoidLayers;
		public LayerMask waypointLayer;
		public string potentialTargetTag = "Player";
		public LayerMask targetLayer;
		public int obstacleFinderResult;
		
		private int totalWaypoints;
		public int currentWaypointNum;
		private Vector3 relativeTarget;
		private float targetAngle;
		private int obstacleHitType;
		public int lookAheadWaypoints = 0;

		//public RaycastHit obstacleCollisionPoint;
		public bool canMoveForwardSafely;
		public float distanceToObstacle;

		public event Action _onReachedNextWaypoint;

		public event Action _onReachedLastWaypoint;

		private EnvironmentScanner environmentScanner;

		//private bool groundDetected = false;
		//public bool GroundDetected => groundDetected;

		public virtual void Start()
		{
			Init();
		}

		public virtual void Init()
		{
			// cache ref to gameObject
			_GO = gameObject;

			// cache ref to transform
			_TR = transform;

			// cache a ref to our rigidbody
			_RB = _TR.GetComponent<Rigidbody>();

			if (_rotateTransform == null)
				_rotateTransform = _TR;

			environmentScanner = GetComponent<EnvironmentScanner>();

			didInit = true;
		}

		public virtual void TurnLeft()
		{
			horz = -1;
		}

		public virtual void TurnRight()
		{
			horz = 1;
		}

		public virtual void MoveForward()
		{
			vert = 1;
		}

		public virtual void MoveBack()
		{
			vert = -1;
		}

		public virtual void NoMove()
		{
			vert = 0;
		}

		public virtual void LookAroundFor()
		{
			if (_followTarget != null)
			{
				float theDist = Vector3.Distance(_TR.position, _followTarget.position);
				bool canSee = CanSee(_followTarget);

				if (theDist < maxChaseDistance)
				{
					if (canSee == true)
					{
						Debug.Log("can see!");
						SetAIState(AIState.chasing_target);
					}
				}
			}
			else
			{
				GameObject potential = GameObject.FindGameObjectWithTag(potentialTargetTag);
				_followTarget = potential.transform;
			}
		}



		public void SetAIControl(bool state)
		{
			AIControlled = state;
		}

		// set up AI parameters --------------------
		public virtual void SetAIState(AIState newState)
		{
			// update AI state
			targetAIState = newState;
			UpdateTargetState();
		}

		//In 3d units, how close the bot will be allowed to get to a waypoint before it advances to the next waypoint.
		public void SetWaypointDistance(float aNum)
		{
			waypointDistance = aNum;
		}


		//In 3d units, minChaseDistance determines how close the bot should get to its target, when chasing a target, before it stops moving toward it. This is to prevent the bot from getting too close and pushing the player around.
		public void SetMinChaseDistance(float aNum)
		{
			minChaseDistance = aNum;
		}

		//In 3d units, how far away from its target the bot can acknowledge a target.
		public void SetMaxChaseDistance(float aNum)
		{
			maxChaseDistance = aNum;
		}

		// -----------------------------------------

		public virtual void SetChaseTarget(Transform theTransform)
		{
			// set a target for this AI to chase, if required
			_followTarget = theTransform;
		}

		public virtual void Update()
		{
			// make sure we have initialized before doing anything
			if (!didInit)
				Init();

			if (!AIControlled)
				return;

			// do AI updates
			UpdateCurrentState();;
		}

		public bool GroundDetected(Vector3 checkLocation)
		{
			return environmentScanner.GroundDetected(checkLocation);
		}

		public virtual void UpdateCurrentState()
		{
			
		}

		public virtual void UpdateTargetState()
		{
			
		}

		//This code works but it can be made to work better => THIS IS MY RESPONSIBILITY
		public virtual int IsObstacleAhead()
		{
	
			distanceToObstacle = environmentScanner.distanceToObstacle;
			canMoveForwardSafely = environmentScanner.canMoveForwardSafely;
			//obstacleCollisionPoint = environmentScanner.obstacleCollisionPoint;
			return environmentScanner.obstacleHitType;
		}

		
		public void TurnTowardTarget(Transform aTarget)
		{
			if (aTarget == null)
				return;

			Vector3 relativeTarget = _rotateTransform.InverseTransformPoint(aTarget.position); // note we use _rotateTransform as a rotation object rather than _TR!

			// Calculate the target angle  
			targetAngle = Mathf.Atan2(relativeTarget.x, relativeTarget.z);

			// Atan returns the angle in radians, convert to degrees 
			targetAngle *= Mathf.Rad2Deg;

			targetAngle = Mathf.Clamp(targetAngle, -_followTargetMaxTurnAngle - targetAngle, _followTargetMaxTurnAngle);

			// turn towards the target at the rate of modelRotateSpeed
			_rotateTransform.Rotate(0, targetAngle * modelRotateSpeed * Time.deltaTime, 0);
		}


		//Diese Funktion ist falsch, ich muss sie korrigieren
		public virtual bool CanSee(Transform aTarget=null)
		{

			// first, let's get a vector to use for raycasting by subtracting the target position from our AI position
			Vector3 tempDirVec = Vector3.Normalize(aTarget.position - _TR.position);

			// lets have a debug line to check the distance between the two manually, in case you run into trouble!
			Debug.DrawLine(_TR.position + (visionHeightOffset * _TR.up), aTarget.position, Color.red);

			RaycastHit hit;

			if (Physics.Raycast(_TR.position + (visionHeightOffset * _TR.up), tempDirVec, out hit, Mathf.Infinity))
			{
				if (IsInLayerMask(hit.transform.gameObject.layer, targetLayer))
				{
					return true;
				}
			}

			// nothing found, so return false
			return false;
		}

		public void SetEnvironmentScannerTarget(Transform target)
		{
			environmentScanner.target = target;
		}

		//Find nearest waypoint to obstacle hit point
		public Transform findStartingWaypoint(out int wpIndex)
		{

			Transform result = null;
			float minDistance = Mathf.Infinity;
			Vector3 center = getObstacleHitPoint();
			Collider[] hitColliders = Physics.OverlapSphere(center, 7f, waypointLayer);
			foreach (var hitCollider in hitColliders)
			{
				Debug.Log("hit go(fsw) = " + hitCollider.gameObject);
				Transform wp = hitCollider.transform;
				if (Vector3.Distance(center, wp.position) < minDistance)
				{
					result = wp;
					minDistance = Vector3.Distance(center, wp.position);
				}
			}

			Debug.DrawRay(center, Vector3.up * 10, Color.green, 5);

			//find the index of the waypoint
			var wpController = result.parent.gameObject.GetComponent<WaypointsController>();
			wpIndex = wpController.GetWaypointIndex(result);

			return result;

		}

		public Vector3 getObstacleHitPoint()
		{
			return environmentScanner.getObstacleHitpoint();
		}

		//Here is the problem
		public void SetWayController(WaypointsController aControl, bool reset = true)
		{
			current_waypointsController = aControl;
			aControl = null;

			// grab total waypoints
			totalWaypoints = current_waypointsController.GetTotal();

			if (reset)
			{
				// make sure that if you use SetReversePath to set shouldReversePathFollowing that you
				// call SetReversePath for the first time BEFORE SetWayController, otherwise it won't set the first waypoint correctly
				if (shouldReversePathFollowing)
				{
					currentWaypointNum = totalWaypoints - 1;
				}
				else
				{
					currentWaypointNum = 0;
				}

				Init();

				// get the first waypoint from the waypoint controller
				_currentWaypointTransform = current_waypointsController.GetWaypoint(currentWaypointNum);
				if(_currentWaypointTransform == null)
					Debug.Log("Current wp + " + currentWaypointNum + " is null!");

				if (startAtFirstWaypoint)
				{
					// position at the _currentWaypointTransform position
					var v = _currentWaypointTransform.position;
					v.y = _TR.position.y;
					_TR.position = v;
				}
			}
		}

		public void SetReversePath(bool shouldRev)
		{
			shouldReversePathFollowing = shouldRev;
		}

		public void SetRotateSpeed(float aRate)
		{
			modelRotateSpeed = aRate;
		}

		public void UpdateWaypoints()
		{
			// If we don't have a waypoint controller, we safely drop out
			if (current_waypointsController == null)
				return;

			if (reachedLastWaypoint && destroyAtEndOfWaypoints)
			{
				// destroy myself(!)
				Destroy(gameObject);
				return;
			}


			// because of the order that scripts run and are initialised, it is possible for this function
			// to be called before we have actually finished running the waypoints initialization, which
			// means we need to drop out to avoid doing anything silly or before it breaks the game.
			if (totalWaypoints == 0)
			{
				// grab total waypoints
				totalWaypoints = current_waypointsController.GetTotal();
				return;
			}
	
			if (_currentWaypointTransform == null)
			{
				Debug.Log("Curent wp is null?");
				// grab our transform reference from the waypoint controller
				_currentWaypointTransform = current_waypointsController.GetWaypoint(currentWaypointNum + lookAheadWaypoints);
			}

			// now we check to see if we are close enough to the current waypoint
			// to advance on to the next one

			Vector3 myPosition = _TR.position;
			myPosition.y = 0;

			// get waypoint position and 'flatten' it
			Debug.Log("current wp transform = " + _currentWaypointTransform);
			Vector3 nodePosition = _currentWaypointTransform.position;
			nodePosition.y = 0;

			// check distance from this to the waypoint

			float currentWayDist = Vector3.Distance(nodePosition, myPosition);

			if (currentWayDist < waypointDistance)
			{
				// we are close to the current node, so let's move on to the next one!

				_onReachedNextWaypoint?.Invoke();

				if (shouldReversePathFollowing)
				{
					currentWaypointNum--;
					// now check to see if we have been all the way around
					if (currentWaypointNum < 0)
					{

						_onReachedLastWaypoint?.Invoke();
						// just incase it gets referenced before we are destroyed, let's keep it to a safe index number
						currentWaypointNum = 0;
						// completed the route!
						reachedLastWaypoint = true;
						// if we are set to loop, reset the currentWaypointNum to 0
						if (loopPath)
						{
							//currentWaypointNum = totalWaypoints;
							//Debug.Log("current waypont num = totalwaypoints that is wrong!");

							currentWaypointNum = totalWaypoints - 1;

							//This needed to be added by me!!! what a shame by the coder!!!
							_currentWaypointTransform = current_waypointsController.GetWaypoint(currentWaypointNum);

							reachedLastWaypoint = false;
						}
						// drop out of this function before we grab another waypoint into _currentWaypointTransform, as
						// we don't need one and the index may be invalid
						return;
					}
				} else {

					if (currentWaypointNum >= totalWaypoints-1)
					{
						Debug.Log("current waypoint num = " + currentWaypointNum);
						_onReachedLastWaypoint?.Invoke();
						// completed the route!
						reachedLastWaypoint = true;
						// if we are set to loop, reset the currentWaypointNum to 0
						if (loopPath)
						{
							currentWaypointNum = 0;

							//This needed to be added by me!!! what a shame by the coder!!!
							_currentWaypointTransform = current_waypointsController.GetWaypoint(currentWaypointNum);

							// the route keeps going in a loop, so we don't want reachedLastWaypoint to ever become true
							reachedLastWaypoint = false;
						}

						// drop out of this function before we grab another waypoint into _currentWaypointTransform, as
						// we don't need one and the index may be invalid
						return;
					}
					currentWaypointNum++;
				}

				// grab our transform reference from the waypoint controller
				_currentWaypointTransform = current_waypointsController.GetWaypoint(currentWaypointNum);

			}
		}

		public float GetHorizontal()
		{
			return horz;
		}

		public float GetVertical()
		{
			return vert;
		}
	}
}