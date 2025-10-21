using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(BoxCollider))]
	public class ClimbWall : MonoBehaviour
	{
		public new BoxCollider collider { get; protected set; }
		public Vector3 center => transform.TransformPoint(collider.center);

		protected Bounds m_localBounds;

		protected virtual void InitializeTag() => tag = GameTags.ClimbableWall;
		protected virtual void InitializeCollider() => collider = GetComponent<BoxCollider>();

		protected void Awake()
		{
			InitializeTag();
			InitializeCollider();
			m_localBounds = new Bounds(collider.center, collider.size);
		}

		public void GetClosestSurfaceInfo(Vector3 queryPoint, out Vector3 closestPoint, out Vector3 surfaceNormal)
		{
			Vector3 localQueryPoint = transform.InverseTransformPoint(queryPoint);
			Vector3 localClosestPoint = m_localBounds.ClosestPoint(localQueryPoint);
			closestPoint = transform.TransformPoint(localClosestPoint);

			Vector3 diff = localQueryPoint - localClosestPoint;
			float maxDist = -1;
			Vector3 localNormal = Vector3.zero;

			if (Mathf.Abs(diff.x) > maxDist) { maxDist = Mathf.Abs(diff.x); localNormal = new Vector3(Mathf.Sign(diff.x), 0, 0); }
			if (Mathf.Abs(diff.y) > maxDist) { maxDist = Mathf.Abs(diff.y); localNormal = new Vector3(0, Mathf.Sign(diff.y), 0); }
			if (Mathf.Abs(diff.z) > maxDist) { localNormal = new Vector3(0, 0, Mathf.Sign(diff.z)); }

			surfaceNormal = transform.TransformDirection(localNormal).normalized;
		}

		public Vector3 ClampPointToWallFace(Vector3 point, Vector3 normal, Vector2 padding, out Vector2 normalizedPos)
		{
			Plane plane = new Plane(-normal, point);
			point = plane.ClosestPointOnPlane(point);

			Vector3 localPoint = transform.InverseTransformPoint(point);
			Vector3 localNormal = transform.InverseTransformDirection(normal);

			Vector3 extents = m_localBounds.extents;
			Vector3 center = m_localBounds.center;

			float hMin, hMax, vMin, vMax;

			if (Mathf.Abs(localNormal.x) > 0.9f) // On an X face
			{
				vMin = center.y - extents.y + padding.y;
				vMax = center.y + extents.y - padding.y;
				hMin = center.z - extents.z + padding.x;
				hMax = center.z + extents.z - padding.x;

				localPoint.y = Mathf.Clamp(localPoint.y, vMin, vMax);
				localPoint.z = Mathf.Clamp(localPoint.z, hMin, hMax);
				normalizedPos = new Vector2(Mathf.InverseLerp(hMin, hMax, localPoint.z), Mathf.InverseLerp(vMin, vMax, localPoint.y));
			}
			else if (Mathf.Abs(localNormal.y) > 0.9f) // On a Y face
			{
				hMin = center.x - extents.x + padding.x;
				hMax = center.x + extents.x - padding.x;
				// BUG FIX HERE: Changed padding.x to padding.y for vertical clamping
				vMin = center.z - extents.z + padding.y;
				vMax = center.z + extents.z - padding.y;

				localPoint.x = Mathf.Clamp(localPoint.x, hMin, hMax);
				localPoint.z = Mathf.Clamp(localPoint.z, vMin, vMax);
				normalizedPos = new Vector2(Mathf.InverseLerp(hMin, hMax, localPoint.x), Mathf.InverseLerp(vMin, vMax, localPoint.z));
			}
			else // On a Z face
			{
				hMin = center.x - extents.x + padding.x;
				hMax = center.x + extents.x - padding.x;
				vMin = center.y - extents.y + padding.y;
				vMax = center.y + extents.y - padding.y;

				localPoint.x = Mathf.Clamp(localPoint.x, hMin, hMax);
				localPoint.y = Mathf.Clamp(localPoint.y, vMin, vMax);
				normalizedPos = new Vector2(Mathf.InverseLerp(hMin, hMax, localPoint.x), Mathf.InverseLerp(vMin, vMax, localPoint.y));
			}

			return transform.TransformPoint(localPoint);
		}
	}
}