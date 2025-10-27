using System;
using UnityEngine;
using System.Collections.Generic; // Needed for List

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Player))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Cone Detect")]
	public class ConeDetect : MonoBehaviour
	{
		[Header("Detection Settings")]
		[Tooltip("The radius of the initial detection sphere.")]
		public float detectionRadius = 15f;

		[Tooltip("The angle of the cone in degrees. Defines how wide the detection area is.")]
		[Range(0, 180)]
		public float coneAngle = 90f;

		[Tooltip("The layer mask to specify which layers to detect colliders on.")]
		public LayerMask detectionLayer = Physics.DefaultRaycastLayers;

		[Header("Gizmo Settings")]
		public Color sphereColor = new Color(0f, 1f, 0f, 0.2f); // Light green, semi-transparent
		public Color coneColor = new Color(1f, 0f, 0f, 0.5f);   // Red, semi-transparent
		public Color detectedColor = Color.yellow;               // Yellow for detected objects

		[SerializeField] private Player m_player;
		[SerializeField] private PlayerInputManager m_inputs;
		private Vector3 m_currentConeDirection = Vector3.forward; // Default direction
		private List<Collider> m_detectedColliders = new List<Collider>(); // To store detected colliders

		public Collider detectedTarget;
		
		protected virtual void Start()
		{
			// Get references to Player and InputManager components
			if(m_player == null)
				m_player = GetComponent<Player>();
			
			if(m_inputs == null)
				m_inputs = GetComponent<PlayerInputManager>();

			if (m_inputs == null)
			{
				Debug.LogError("ConeDetect requires a PlayerInputManager component on the same GameObject.", this);
				enabled = false; // Disable the script if input manager is missing
			}
		}

		protected virtual void Update()
		{
			if (m_player == null || m_inputs == null) return; // Ensure components are valid

			// 1. Get Input Direction (using GetMovementDirection for raw input)
			Vector3 inputDirection = m_inputs.GetLookDirection();

			// 2. Determine Cone Direction based on input (Sidescroller perspective)
			CalculateConeDirection(inputDirection);

			// 3. Perform Cone Detection
			DetectInCone();

			// Optional: Do something with m_detectedColliders (e.g., interact with objects)
			// foreach (Collider col in m_detectedColliders)
			// {
			//     Debug.Log($"Detected: {col.name}");
			//     // Add interaction logic here
			// }
		}

		/// <summary>
		/// Calculates the direction of the detection cone based on player input
		/// and the player's current pathForward.
		/// </summary>
		/// <param name="inputDirection">Raw input from PlayerInputManager.GetMovementDirection()</param>
		protected virtual void CalculateConeDirection(Vector3 inputDirection)
		{
			// Get the player's forward direction in the 2D plane
			Vector3 playerForward = m_player.pathForward;
			// Get the player's up direction
			Vector3 playerUp = m_player.transform.up;
			// Calculate the player's right direction in the 2D plane
			Vector3 playerRight = Vector3.Cross(playerUp, playerForward).normalized;

			// Initialize cone direction based on horizontal input first
			if (inputDirection.x > 0.1f) // Pressing Right
			{
				m_currentConeDirection = playerForward;
			}
			else if (inputDirection.x < -0.1f) // Pressing Left
			{
				m_currentConeDirection = -playerForward;
			}
			else // No significant horizontal input, default to forward or maintain last if needed
			{
				// If you want the cone to stay in the last horizontal direction when only up/down is pressed,
				// you might adjust this logic. For now, we prioritize vertical if horizontal is neutral.
				m_currentConeDirection = Vector3.zero; // Neutral horizontal
			}

			// Add vertical component based on input
			if (inputDirection.z > 0.1f) // Pressing Up
			{
				if (m_currentConeDirection == Vector3.zero) // Only Up
					m_currentConeDirection = playerUp;
				else // Diagonal Up
					m_currentConeDirection = (m_currentConeDirection + playerUp).normalized;
			}
			else if (inputDirection.z < -0.1f) // Pressing Down
			{
				if (m_currentConeDirection == Vector3.zero) // Only Down
					m_currentConeDirection = -playerUp;
				else // Diagonal Down
					m_currentConeDirection = (m_currentConeDirection - playerUp).normalized;
			}

			// If no input direction, default to player's current facing direction (optional)
			if (m_currentConeDirection == Vector3.zero)
			{
				m_currentConeDirection = playerForward; // Or player.transform.forward if not strictly sidescrolling
			}
		}


		/// <summary>
		/// Detects colliders within the defined sphere and filters them based on the cone angle and direction.
		/// </summary>
		protected virtual void DetectInCone()
		{
			m_detectedColliders.Clear(); // Clear previous detections
			
			Vector3 detectionCenter = transform.position + m_player.center; // Use player's center

			// Perform the initial sphere overlap check
			Collider[] hitColliders = Physics.OverlapSphere(detectionCenter, detectionRadius, detectionLayer, QueryTriggerInteraction.Ignore);
			
			//TODO: Change this to select the closest one.
			foreach (Collider hitCollider in hitColliders)
			{
				if (hitCollider.transform == transform) continue; // Skip self

				// Calculate direction from player center to collider's closest point or center
				Vector3 directionToCollider = (hitCollider.ClosestPoint(detectionCenter) - detectionCenter).normalized;

				// If the direction is zero (collider center is exactly at detection center), skip or handle as needed
				if (directionToCollider == Vector3.zero) continue;

				// Calculate the angle between the cone direction and the direction to the collider
				float angleToCollider = Vector3.Angle(m_currentConeDirection, directionToCollider);

				// Check if the collider is within the cone angle
				if (angleToCollider <= coneAngle / 2f)
				{
					m_detectedColliders.Add(hitCollider);
				}
			}
			
			if (m_detectedColliders.Count > 0)
			{
				detectedTarget = m_detectedColliders[0];
			}
			else
			{
				detectedTarget = null;
			}
			

		}

		/// <summary>
		/// Draws Gizmos in the Scene view for visualizing the detection area.
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (m_player == null)
			{
				// Try to get component in edit mode for gizmo preview
				m_player = GetComponent<Player>();
				if (m_player == null) return;
			}

            // Get input direction *even in edit mode* if possible (requires Input System setup)
            // For simplicity in edit mode, we'll draw based on m_currentConeDirection which updates in play mode.
            // If you need live gizmo updates based on input *in edit mode*, more setup is needed.
            #if UNITY_EDITOR
            // For runtime gizmo updates based on live input:
            if (Application.isPlaying && m_inputs != null)
            {
                 Vector3 inputDirectionGizmo = m_inputs.GetLookDirection();
                 CalculateConeDirection(inputDirectionGizmo); // Recalculate for gizmo
            }
            // Otherwise, it uses the m_currentConeDirection from the last Update() call.
            #endif


			Vector3 gizmoCenter = transform.position; // Use player's center

			// 1. Draw the Overlap Sphere
			Gizmos.color = sphereColor;
			Gizmos.DrawSphere(gizmoCenter, detectionRadius);

			// 2. Draw the Cone
			Gizmos.color = coneColor;
			Vector3 coneDirection = m_currentConeDirection; // Use the calculated direction
			if (coneDirection == Vector3.zero) coneDirection = m_player.pathForward; // Default if zero

			// Calculate cone boundary directions
			float halfAngleRad = (coneAngle / 2f) * Mathf.Deg2Rad;
			Quaternion coneRotation = Quaternion.LookRotation(coneDirection, m_player.transform.up); // Align cone 'up' with player 'up'

			Vector3 upBoundary = coneRotation * Quaternion.Euler(-coneAngle / 2f, 0, 0) * Vector3.forward;
            Vector3 downBoundary = coneRotation * Quaternion.Euler(coneAngle / 2f, 0, 0) * Vector3.forward;
			Vector3 rightBoundary = coneRotation * Quaternion.Euler(0, coneAngle / 2f, 0) * Vector3.forward;
			Vector3 leftBoundary = coneRotation * Quaternion.Euler(0, -coneAngle / 2f, 0) * Vector3.forward;

			// Draw cone lines
			Gizmos.DrawLine(gizmoCenter, gizmoCenter + upBoundary * detectionRadius);
			Gizmos.DrawLine(gizmoCenter, gizmoCenter + downBoundary * detectionRadius);
			Gizmos.DrawLine(gizmoCenter, gizmoCenter + rightBoundary * detectionRadius);
            Gizmos.DrawLine(gizmoCenter, gizmoCenter + leftBoundary * detectionRadius);


			// Draw arcs to outline the cone base
            // Note: Drawing perfect arcs on a sphere surface projected in 3D is complex.
            // This approximates by drawing arcs on planes aligned with the cone direction.
            // For a side-scroller, arcs on the plane defined by player Up and PathForward might be sufficient.

            // Example Arc (adjust axes as needed for side-scroller view)
            // Handles.color = coneColor; // Requires using UnityEditor namespace
            // Vector3 arcNormal = Vector3.Cross(upBoundary, rightBoundary).normalized; // Normal of one cone face plane
            // Handles.DrawWireArc(gizmoCenter, coneDirection, leftBoundary, coneAngle, detectionRadius);
            // Handles.DrawWireArc(gizmoCenter, m_player.transform.right, upBoundary, coneAngle, detectionRadius); // Example arc

			// Draw center line of the cone
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(gizmoCenter, gizmoCenter + coneDirection * detectionRadius * 1.1f); // Slightly longer blue line

			// 3. Highlight Detected Colliders
			Gizmos.color = detectedColor;
			if (Application.isPlaying) // Only draw detected in play mode
			{
				foreach (Collider col in m_detectedColliders)
				{
					if (col != null)
					{
						Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
					}
				}
			}
		}
	}
}