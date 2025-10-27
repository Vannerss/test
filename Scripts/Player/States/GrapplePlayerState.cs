// Scripts/Player/States/HookshotPlayerState.cs
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Hookshot Player State")]
	public class GrapplePlayerState : PlayerState
	{
		// Variables to store hookshot state
		// protected GrappleTarget m_currentTarget; // REMOVED - Read from Player
		protected Vector3 m_hookshotPoint;
		protected bool m_isActive = false;

		// Configuration
		public float pullSpeed = 20f;
		public Transform hookshotOrigin; // Assign in Inspector on Player

		// REMOVED SetTarget method

		protected override void OnEnter(Player player)
		{
			// --- MODIFICATION START ---
			// Read the target from the Player script
			GrappleTarget target = player.currentGrappleTarget;

			if (target == null)
			// --- MODIFICATION END ---
			{
				Debug.LogError("HookshotPlayerState entered without a target set on Player!");
				player.states.Change<IdlePlayerState>(); // Go back safely
				return;
			}

			Debug.Log($"Hookshot Active - Target: {target.name}");
			m_hookshotPoint = target.transform.position; // Use target's main position
			m_isActive = true;
			player.velocity = Vector3.zero; // Stop player movement

			// TODO: Start visual effect for connected hook
		}

		protected override void OnExit(Player player)
		{
			Debug.Log("Exiting Hookshot State (Active Pull)");
			// Clean up visuals

			// Ensure target is unhighlighted (belt-and-suspenders)
			GrappleTarget exitingTarget = player.currentGrappleTarget; // Read before clearing
			if(exitingTarget != null) {
				exitingTarget.SetHighlight(false);
			}

			// --- MODIFICATION START ---
			// Clear the target on the Player script
			player.currentGrappleTarget = null;
			// --- MODIFICATION END ---

			m_isActive = false;
		}

		protected override void OnStep(Player player)
		{
			// --- MODIFICATION START ---
			// Use the target stored on the Player script for checks
			GrappleTarget target = player.currentGrappleTarget;

			if (!m_isActive || target == null)
			// --- MODIFICATION END ---
			{
				player.states.Change<FallPlayerState>();
				return;
			}

			// --- Hookshot Movement Logic ---
			// Use m_hookshotPoint (set in OnEnter)
			Vector3 directionToTarget = (m_hookshotPoint - player.position).normalized;
			float distanceToTarget = Vector3.Distance(player.position, m_hookshotPoint);

			if (distanceToTarget > player.radius * 2f)
			{
				Vector3 movement = directionToTarget * (pullSpeed * Time.deltaTime);
				player.controller.Move(movement);
			}
			else
			{
				Debug.Log("Reached hookshot target.");
				player.velocity = Vector3.zero;
				player.states.Change<FallPlayerState>();
				return;
			}

			// --- Handle Release Input ---
			if (player.inputs.actions["Jump"].WasPressedThisFrame())
			{
				Debug.Log("Hookshot released by player.");
				player.velocity = directionToTarget * (pullSpeed * 0.5f);
				player.states.Change<FallPlayerState>();
				return;
			}
		}

		public override void OnContact(Player player, Collider other)
		{
			// Optional: Release if hitting obstacles
		}

		// REMINDER: You need to add GetState<T> to EntityStateManager<T>
		// public TState GetState<TState>() where TState : EntityState<T> { ... }
	}
}