// project armada
using UnityEngine;
using System.Collections;

namespace Harris.GPC
{

    public static class MathHelper
    {
        public static Vector3 PerpendicularClockWise(Vector3 vec)
        {
            return new Vector3(vec.z,vec.y,-vec.x);
        }

        public static Vector3 PerpendicularCounterClockWise(Vector3 vec)
        {
            return new Vector3(-vec.z,vec.y,vec.x);
        }
    }

    public class InFrontTest
    {
        private Vector3 heading;
        private Vector3 fwd;

        public InFrontTest(Transform _self, Transform _target) : this(_self.forward, _target.position - _self.transform.position)
        {
        }

        public InFrontTest(Vector3 _fwd, Vector3 _heading)
        {
            fwd = _fwd;
            heading = _heading;
            
        }

        public bool targetIsInFront()
        {
            return Vector3.Dot(heading, fwd) > 0f;
        }
    }

    public class LeftRightTest {
        private Vector3 heading;

        private Vector3 fwd;

        private Vector3 up;

        private float dirNum;

        public LeftRightTest(Transform _self, Transform _target) : this(_self.forward,
                                                                    _target.position - _self.transform.position,
                                                                    _self.up)
        {
        }

        public LeftRightTest(Vector3 _fwd, Vector3 _heading, Vector3 _up)
        {
            fwd = _fwd;
            heading = _heading;
            up = _up;
        }

        public bool targetIsLeft()
        {
            return (dirNum = AngleDir(fwd, heading, up)) == -1;
        }
        
        private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);
            
            if (dir > 0f) {
                return 1f;
            } else if (dir < 0f) {
                return -1f;
            } else {
                return 0f;
            }
        }
    }
}