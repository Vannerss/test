using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Aiming Hookshot Player State")]
	public class AimingGrapplePlayerState : PlayerState
	{
		// ... (Configuration variables are unchanged) ...
		public LayerMask grappleableLayer;
		public LayerMask obstacleLayer;
		public float hookshotRange = 150f;
		public Transform hookshotOrigin;

		private GrappleTarget m_potentialTarget;
		private Camera m_mainCamera;
		private PlayerGrappleUI m_grappleUI; // Reference to the UI script

		protected override void OnEnter(Player player)
		{
			grappleableLayer = LayerMask.GetMask("GrappleTarget");
			hookshotOrigin = player.transform;
			
			Debug.Log("Entering Aiming Hookshot State");
			m_potentialTarget = null;
			m_mainCamera = Camera.main; // Cache the main camera
			
			// Get the UI component and show the crosshair
			m_grappleUI = player.GetComponent<PlayerGrappleUI>();
			if (m_grappleUI != null)
			{
				m_grappleUI.ShowCrosshair(true); // This now ALSO handles cursor state
				m_grappleUI.SetCrosshairLocked(false); 
			}
			
			// Player and camera should not move
			//player.SetVelocity(Vector3.zero);
		}

		protected override void OnExit(Player player)
		{
			Debug.Log("Exiting Aiming Hookshot State");
			
			// Hide the crosshair
			if (m_grappleUI != null)
			{
				m_grappleUI.ShowCrosshair(false); // This restores cursor state
			}
			m_grappleUI = null; 
			
			if (m_potentialTarget != null)
			{
				m_potentialTarget.SetHighlight(false);
				m_potentialTarget = null;
			}
		}

		protected override void OnStep(Player player)
		{
			// --- NEW SIMPLIFIED RAYCAST ---
			
			// If UI isn't ready, do nothing.
			if (m_grappleUI == null) return;

			// Get the aim position directly from the UI script
			Vector2 aimPosition = m_grappleUI.CurrentCrosshairScreenPosition;
			
			// Cast the ray from our dynamic aim position
			Ray ray = m_mainCamera.ScreenPointToRay(aimPosition);
			
			// --- END NEW RAYCAST ---

			GrappleTarget hitTarget = null; 

			if (Physics.Raycast(ray, out RaycastHit hit, hookshotRange, grappleableLayer | obstacleLayer))
			{
				// Check if we hit a grappleable target directly
				if (((1 << hit.collider.gameObject.layer) & grappleableLayer) != 0)
				{
					// Verify line of sight from hookshot origin
					if (!Physics.Linecast(hookshotOrigin.position, hit.point, obstacleLayer))
					{
						hitTarget = hit.collider.GetComponent<GrappleTarget>();
					}
				}
			}

			// --- Highlighting Logic (Both 3D object and 2D UI) ---
			if (m_potentialTarget != null && m_potentialTarget != hitTarget)
			{
				m_potentialTarget.SetHighlight(false);
			}
			
			m_potentialTarget = hitTarget;
			
			if (m_potentialTarget != null)
			{
				m_potentialTarget.SetHighlight(true); 
			}
			
			// Update the UI sprite (locked or default)
			if (m_grappleUI != null)
			{
				m_grappleUI.SetCrosshairLocked(m_potentialTarget != null);
			}
			
			// --- Firing Logic ---
			bool fireInput = player.inputs.actions["Grapple"].WasPressedThisFrame(); 

			if (fireInput && m_potentialTarget != null)
			{
				player.currentGrappleTarget = m_potentialTarget;
				player.states.Change<GrapplePlayerState>();
				return; 
			}

			// --- Cancel Logic ---
			bool cancelInput = player.inputs.actions["Crouch"].WasPressedThisFrame(); 
			if (cancelInput)
			{
				Debug.Log("Aiming cancelled.");
				player.states.Change(player.states.last); 
				return; 
			}
			
			// Ensure player doesn't move
			//player.SetVelocity(Vector3.zero);
		}
		
		public override void OnContact(Player player, Collider other) { }
	}
}