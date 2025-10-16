using System;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class ClimbWall : MonoBehaviour
    {
        public new BoxCollider collider {get; protected set; }
        public Vector3 center => collider.bounds.center;

        protected Bounds _localBounds;

        public Vector3 normal => transform.forward;
        public Vector3 surfaceRight => transform.right;
        public Vector3 surfaceUp => transform.up;

        protected virtual void InitializeTag() => tag = GameTags.ClimbableWall; //TODO define in unity.
        protected virtual void InitializeCollider() => collider = GetComponent<BoxCollider>();
        protected virtual void InitializeLocalBounds() => _localBounds = BoundsHelper.GetLocalBounds(collider);

        protected void Awake()
        {
            InitializeTag();
            InitializeCollider();
            InitializeLocalBounds();
        }

        /// Returns the direction (normalized) from a given Transform toward the wall face.
        /// Ignores lateral components so the vector points orthogonally to the wall.
        public Vector3 GetDirectionToWall(Transform other) => GetDirectionToWall(other, out _);

        /// Returns the direction (normalized) from a given Transform toward the wall face and the absolute distance along the wall normal.
        /// Assumes the climbable face’s normal is transform.forward.
        public Vector3 GetDirectionToWall(Transform other, out float distance)
        {
            // Vector from the other to the wall center, expressed in wall local space
            Vector3 toWallCenterWS = collider.bounds.center - other.position;
            Vector3 toWallCenterLS = transform.InverseTransformDirection(toWallCenterWS);

            // We only want motion along the wall normal (local Z). Zero X/Y so we don't "slide" along the wall.
            Vector3 towardPlaneLS = new Vector3(0f, 0f, toWallCenterLS.z);
            Vector3 towardPlaneWS = transform.TransformDirection(towardPlaneLS);

            distance = towardPlaneWS.magnitude;
            if (distance > 0.0001f)
                return towardPlaneWS / distance;

            // Already on the plane: face opposite to the normal (toward the wall)
            distance = 0f;
            return -normal;
        }

        // Returns the closest point on the wallplane.
        public Vector3 GetStickPointOnWallPlane(Vector3 worldPoint, float stickDistance = 0f)
        {
            Plane plane = new Plane(normal, center);
            float signed = plane.GetDistanceToPoint(worldPoint);

            return worldPoint - normal * signed + normal * stickDistance;
        }

        public Vector3 ClampPointToWallArea(Vector3 point, float offset, out Vector2 t)
        {
            return ClampPointToWallArea(point, new Vector2(offset, offset), out t);
        }
        
        public Vector3 ClampPointToWallArea(Vector3 point, Vector2 offset, out Vector2 t)
        {
            Vector3 local = transform.InverseTransformPoint(point);

            Vector3 e = _localBounds.extents;

            float minX = -e.x + offset.x;
            float maxX = e.x - offset.x;
            float minY = -e.y + offset.y;
            float maxY = e.y - offset.y;

            local.x = Mathf.Clamp(local.x, minX, maxX);
            local.y = Mathf.Clamp(local.y, minY, maxY);
            
            t = new Vector2(
                Mathf.InverseLerp(minX, maxX, local.x),
                Mathf.InverseLerp(minY, maxY, local.y)
            );

            return transform.TransformPoint(local);
        }

        public virtual void RotateToWall(Transform other)
        {
            Quaternion target = Quaternion.LookRotation(-normal, surfaceUp);
            other.rotation = target;
        }

        public Vector3 GetClampedStickPoint(Vector3 point, float stickDistance, Vector2 clampOffset, out Vector2 t)
        {
            Vector3 onPlane = GetStickPointOnWallPlane(point, stickDistance);
            return ClampPointToWallArea(onPlane, clampOffset, out t);
        }

    }
}