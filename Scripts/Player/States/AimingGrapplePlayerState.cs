// Scripts/Player/States/AimingHookshotPlayerState.cs
using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Aiming Hookshot Player State")]
	public class AimingGrapplePlayerState : PlayerState
	{
		// Configuration (Consider moving to PlayerStats or a dedicated Hookshot component)
		public LayerMask grappleableLayer; // Assign in Inspector on Player or Stats
		public LayerMask obstacleLayer;	// Layers that block the hookshot (usually everything except Player, Grappleable, triggers)
		public float hookshotRange = 150f;
		public Transform hookshotOrigin;	// Assign in Inspector on Player

		private GrappleTarget m_potentialTarget;
		private Camera m_mainCamera;

		protected override void OnEnter(Player player)
		{
			grappleableLayer = LayerMask.GetMask("GrappleTarget");
			hookshotOrigin = player.transform;
			
			Debug.Log("Entering Aiming Hookshot State");
			m_potentialTarget = null;
			m_mainCamera = Camera.main; // Cache the main camera
			

			Game.LockCursor(false);

			// Optional: Slow down time slightly while aiming?
			// Time.timeScale = 0.8f;
		}

		protected override void OnExit(Player player)
		{
			Debug.Log("Exiting Aiming Hookshot State");
			// Ensure previous target is unhighlighted
			if (m_potentialTarget != null)
			{
				m_potentialTarget.SetHighlight(false);
				m_potentialTarget = null;
			}

			Game.LockCursor(true);

			// Optional: Restore time scale if changed
			// Time.timeScale = 1.0f;
		}

		protected override void OnStep(Player player)
		{
			// --- Aiming Raycast ---
			Ray ray;
			Vector2 aimPosition; // To store mouse or screen center

			// Check if a mouse device is currently connected and being used
			// Mouse.current will be null if no mouse is connected
			Mouse mouse = Mouse.current;

			if (mouse != null && mouse.deviceId != InputDevice.InvalidDeviceId) // Check if mouse is valid
			{
				// Read the current mouse position from the new Input System
				aimPosition = mouse.position.ReadValue();
				ray = m_mainCamera.ScreenPointToRay(aimPosition);
				// Debug.Log("Using Mouse Position: " + aimPosition); // Optional debug
			}
			else
			{
				// No mouse detected or active, default to center screen raycast
				// This is useful for controllers or mobile where there's no mouse cursor
				aimPosition = new Vector2(Screen.width / 2f, Screen.height / 2f); // Calculate center
				ray = m_mainCamera.ScreenPointToRay(aimPosition);
				// Debug.Log("Using Center Screen"); // Optional debug
			}

			GrappleTarget hitTarget = null; // Target hit by the raycast this frame

			if (Physics.Raycast(ray, out RaycastHit hit, hookshotRange, grappleableLayer | obstacleLayer))
			{
				// Check if we hit a grappleable target directly
				if (((1 << hit.collider.gameObject.layer) & grappleableLayer) != 0)
				{
					// Verify line of sight from hookshot origin (not just camera)
					if (!Physics.Linecast(hookshotOrigin.position, hit.point, obstacleLayer))
					{
						hitTarget = hit.collider.GetComponent<GrappleTarget>();
					}
				}
				// If we hit an obstacle first, there's no valid target in line of sight via this ray
			}

			// --- Highlighting Logic ---
			// Unhighlight previous target if it's no longer the potential target
			if (m_potentialTarget != null && m_potentialTarget != hitTarget)
			{
				m_potentialTarget.SetHighlight(false);
			}

			// Highlight the new potential target
			m_potentialTarget = hitTarget;
			if (m_potentialTarget != null)
			{
				m_potentialTarget.SetHighlight(true);
			}

			// --- Firing Logic ---
			bool fireInput = player.inputs.actions["Grapple"].WasPressedThisFrame(); // Or your fire button/mouse click

			if (fireInput && m_potentialTarget != null)
			{
				// --- MODIFICATION START ---
				// Set the target on the Player script
				player.currentGrappleTarget = m_potentialTarget;
				// --- MODIFICATION END ---

				// Found a target and fire button pressed - transition to active hookshot state
				player.states.Change<GrapplePlayerState>();
				// m_potentialTarget will be unhighlighted in OnExit
				return; // Exit early
			}

			// --- Cancel Logic ---
			// Optional: Add input to cancel aiming (e.g., pressing crouch, different button)
			bool cancelInput = player.inputs.actions["Crouch"].WasPressedThisFrame(); // Example cancel input
			if (cancelInput)
			{
				Debug.Log("Aiming cancelled.");
				player.states.Change(player.states.last); // Go back to previous state
				return; // Exit early
			}

			// Keep player somewhat controlled while aiming (optional)
			// player.Gravity(); // Apply gravity
			// player.SnapToGround();
			// player.Friction(); // Apply friction to stop quickly
		}

		// No specific collision handling needed in aiming state
		public override void OnContact(Player player, Collider other) { }
	}
}