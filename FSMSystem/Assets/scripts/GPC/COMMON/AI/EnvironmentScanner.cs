using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
//using Harris.Util;

namespace Harris.GPC
{
	public class EnvironmentScanner : MonoBehaviour
	{

		private float scanTimer = 0.1f;
		private Ray leftScannerRay;
		private Ray rightScannerRay;
		private Ray groundDetectionRay;

		public RaycastHit obstacleCollisionPoint;

		public int obstacleHitType;

		private int oldObstacleHitType;

		private float obstacleAvoidDistance;

		[SerializeField]
		private LayerMask obstacleAvoidLayers;

		[SerializeField]
		private LayerMask groundLayer;

		[SerializeField]
		public Transform target;

		public bool canMoveForwardSafely;

		public float distanceToObstacle;

		public Vector3 obstacleHitPoint = Vector3.zero;

		private Vector3 prevPosition = Vector3.zero;

		void Start()
		{
			//InvokeRepeating("ScanEnvironment", 0f, 0.1f);
			leftScannerRay = new Ray();
			rightScannerRay = new Ray();
			obstacleHitType = 0;
			distanceToObstacle = 0f;
		}

		// Update is called once per frame
		void Update()
		{
			
			var moveDir = (transform.position - prevPosition);
			moveDir.y = 0;
			//if moveDir.magnitude < 0.05f
				//moveDir = Vector3.zero;
			ScanEnvironment(moveDir);
			prevPosition = transform.position;
		}

		private void ScanEnvironment(Vector3 moveDir)
		{
			IsObstacleAhead();
			GroundDetected(moveDir);
		}

		public bool GroundDetected(Vector3 checkLocation)
		{
			RaycastHit hit;

			groundDetectionRay.origin = checkLocation;
			groundDetectionRay.direction = -Vector3.up;

			//public static void DrawRay(Vector3 start, Vector3 dir, Color color = Color.white, float duration = 0.0f, bool depthTest = true);
			Debug.DrawRay(groundDetectionRay.origin, groundDetectionRay.direction*5f, Color.green);

			return Physics.Raycast(groundDetectionRay, out hit, 5f, groundLayer);
		}

		public void IsObstacleAhead()
		{
			//scan for potential obstacles to avoid them
			var dirToTarget = target.position-transform.position;
			obstacleAvoidDistance = dirToTarget.magnitude;
	
			var dirToTargetNormalized = dirToTarget.normalized;
			dirToTargetNormalized.y = 0;
			var perpClockwise = MathHelper.PerpendicularClockWise(dirToTargetNormalized);
			var perpCClockwise = MathHelper.PerpendicularCounterClockWise(dirToTargetNormalized);

			leftScannerRay.origin = transform.position + Vector3.up + perpCClockwise * 0.4f;
			rightScannerRay.origin = transform.position +  Vector3.up + perpClockwise* 0.4f;

			Debug.DrawRay(leftScannerRay.origin, leftScannerRay.direction * obstacleAvoidDistance);
			Debug.DrawRay(rightScannerRay.origin, rightScannerRay.direction * obstacleAvoidDistance);

			oldObstacleHitType = obstacleHitType;
			obstacleHitType = 0;

			leftScannerRay.direction = rightScannerRay.direction = dirToTarget;

			RaycastHit hit;//QueryTriggerInteraction.Ignore
			if (Physics.Raycast(leftScannerRay, out hit, obstacleAvoidDistance, obstacleAvoidLayers, QueryTriggerInteraction.Ignore))
			{
				// obstacle
				// it's a left hit, so it's a type 1 (though it could change when we check on the other side)

				if(hit.collider.transform != target)
				{
					obstacleHitType = 1;
					obstacleHitPoint = hit.point;
				}

			}

			if (Physics.Raycast(rightScannerRay, out hit, obstacleAvoidDistance, obstacleAvoidLayers, QueryTriggerInteraction.Ignore))
			{
				if(hit.collider.transform != target)
				{
	
					// obstacle
					if (obstacleHitType == 0)
					{
						// if we haven't hit anything yet, this is a type 2
						obstacleHitType = 2;
					}
					else
					{
						// if we have hits on both left and right raycasts, it's a type 3
						obstacleHitType = 3;
					}
				}
				else
				{
					Debug.Log("hitt target!");
				}

				obstacleHitPoint = hit.point;
			}

			if (obstacleHitType != 0)
			{
				//if(hit.collider.transform != target) Debug.Log("hit collider: " + hit.collider.transform);
				distanceToObstacle = Vector3.Distance(transform.position, hit.point);
				canMoveForwardSafely = false;
			}
			else
			{
				distanceToObstacle = 0f;
			}

			//Partially redundant code
			//distanceToObstacle = obstacleHitType != 0 ? Vector3.Distance(transform.position, hit.point) : 0f;

			if (obstacleHitType == 0 && oldObstacleHitType != 0)
			{
				StartCoroutine(Wait(0.2f));
			}

			obstacleCollisionPoint = hit;
		}

		public Vector3 getObstacleHitpoint()
		{
			return obstacleHitPoint;
		}

		public bool CanMoveForwardSafely()
		{
			return canMoveForwardSafely;
		}

		private IEnumerator Wait(float duration)
		{
			yield return new WaitForSeconds(duration);
			canMoveForwardSafely = true;
		}
	}
}
