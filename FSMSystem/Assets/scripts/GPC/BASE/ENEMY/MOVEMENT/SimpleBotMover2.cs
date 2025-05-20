using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// use this script to make a bot move around
namespace Harris.GPC
{

	public class SimpleBotMover2 :MonoBehaviour, ICommandReceiver
	{
		public Rigidbody _RB;
		public float moveSpeed = 1f;

		public Vector3 centerOfGravity;
        
        [SerializeField]
        private float _followTargetMaxTurnAngle = 120;

        [SerializeField]
        private float _modelRotateSpeed = 4;

		private Transform _TR;

        public bool _moving = false;

        public bool Moving => _moving;

        private Vector3 _targetPosition = Vector3.zero;

        public Vector3 TargetPosition {get => _targetPosition; set => _targetPosition = value;}

        private bool _turning = false;
        public bool Turning => _turning;

        public event Action _onReachedGoalLocation;

        public event Action _onAvoidedUnit;

        private List<Vector3> _avoidPath;

        public List<Vector3> AvoidPath {get => _avoidPath; set => _avoidPath = value;}
        
        private void Awake()
        {
            ((ICommandReceiver)this).BindReceiver("WASDCmd");
        }

		void Start()
		{
			// cache a ref to our transform
			_TR = transform;

            _RB = GetComponent<Rigidbody>();
			// set center of gravity
			if (_RB != null)
			{
				_RB.centerOfMass = centerOfGravity;
			}
		}

        public void ReceiveCommand(Command command)
        {

            if(command.Name == "WASDCmd")
            {
                Debug.Log("wasd cmd");
            }
        }


        //MOVETO PROBLEM
        public void MoveTo(Vector3 targetPosition)
        {
            if(Vector3.Distance(transform.position,targetPosition) > 1.7f)
            {
                _targetPosition = targetPosition;
                _moving = true;
                _turning = true;
            }
        }

        public void Pause(bool stop)
        {
            Debug.Log("pausing");
            Debug.Log("turning = false");
            _RB.linearVelocity = Vector3.zero;
            _turning = false;
            
        }

        public void Continue()
        {
            _moving = true;
            _turning = true;
        }

        public void TurnTowardTarget()
        {
            TurnTowardTarget(_targetPosition);
        }

        public void Move()
        {
            if(Vector3.Distance(transform.position, _targetPosition) > 0.1f)
            {
                TurnTowardTarget(_targetPosition);
                _RB.linearVelocity = transform.forward*moveSpeed;
            }
            else
            {
                _moving = false;
                _RB.linearVelocity = Vector3.zero;

                if(Vector3.Distance(transform.position, _targetPosition) <= 0.1f)
                {
                    Debug.Log("reached target location!");
                    _turning = false;
                    _onReachedGoalLocation?.Invoke();
                }
            }
        }



        public void TurnTowardTarget(Transform aTarget)
		{
            TurnTowardTarget(aTarget.position);
        }

        public void FollowAvoidPath()
        {

            if(_turning)
            {
                Debug.Log("turning is true");
            }
            if(AvoidPath.Count > 0 && Vector3.Distance(transform.position, new Vector3(AvoidPath[0].x,transform.position.y,AvoidPath[0].z)) > 0.1f)
            {
                //_moving = true;
                _turning = true;
                TurnTowardTarget(new Vector3(AvoidPath[0].x,transform.position.y,AvoidPath[0].z));
                _RB.linearVelocity = transform.forward*moveSpeed;
            }
            else if(AvoidPath.Count > 0)
            {
                //_turning = false;
                AvoidPath.RemoveAt(0);
            }

            if(AvoidPath.Count == 0)
            {
                //_moving = false;
                Debug.Log("avoid path length = 0");
                _onAvoidedUnit?.Invoke();
            }
        }

        public void TurnTowardTarget(Vector3 aTarget)
		{
			if (aTarget == null)
				return;

			Vector3 relativeTarget = transform.InverseTransformPoint(aTarget); // note we use _rotateTransform as a rotation object rather than _TR!

			// Calculate the target angle  
			var targetAngle = Mathf.Atan2(relativeTarget.x, relativeTarget.z);

			// Atan returns the angle in radians, convert to degrees 
			targetAngle *= Mathf.Rad2Deg;

            Debug.Log("targetAngle= " + targetAngle);

            //if(targetAngle > 180)
                //targetAngle = -(360-targetAngle);

            //if(Mathf.Abs(targetAngle) < 0.5f)
                //_turning = false;

			// turn towards the target at the rate of modelRotateSpeed
            //if(_turning && Mathf.Abs(targetAngle) > .5f)
            //if(_turning)
			    transform.Rotate(0, targetAngle * _modelRotateSpeed * Time.deltaTime, 0);
		}

	}
}