using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Idle Player State")]
	public class IdlePlayerState : PlayerState
	{
		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Jump();
			player.Fall();
			player.Spin();
			player.PickAndThrow();
			player.RegularSlopeFactor();
			player.Friction();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			if (inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
			{
				player.states.Change<WalkPlayerState>();
			}
			else if (player.inputs.GetCrouchAndCraw())
			{
				player.states.Change<CrouchPlayerState>();
			}
			
			
			// if (player.inputs.actions["Grapple"].WasPressedThisFrame())
			// {
			// 	Debug.Log(player.inputs.GetLookDirection());
			// 	Debug.Log(player.IsThereATargetFor2DGrapple());
			// 	if (player.inputs.GetLookDirection().magnitude > 0 && player.IsThereATargetFor2DGrapple())
			// 	{
			// 		player.SetGrappleTargetGrapple2D();
			// 		player.states.Change<GrapplePlayerState>();
			//
			// 	}
			// 	else
			// 	{
			// 		player.states.Change<AimingGrapplePlayerState>();
			// 		return; // Exit current state's step logic early
			// 	}
			// }
			
			if (player.inputs.GetGrappleTapDown())
			{
				// This is a TAP. Try to do the 2D auto-aim grapple.
				if (player.inputs.GetLookDirection().magnitude > 0 && player.IsThereATargetFor2DGrapple())
				{
					Debug.Log("Grapple TAP: 2D Target Fire!");
					player.SetGrappleTargetGrapple2D();
					player.states.Change<GrapplePlayerState>();
					return; // Exit early
				}
				else
				{
					Debug.Log("Grapple TAP: No 2D target found.");
					// Optional: Play a "fail" sound here
				}
			}
			// Check for HOLD: (Button was held past the threshold)
			else if (player.inputs.GetGrappleHoldStarted())
			{
				// This is a HOLD. Enter the 3D free-aim state.
				Debug.Log("Grapple HOLD: Entering 3D Aim State.");
				player.states.Change<AimingGrapplePlayerState>();
				return; // Exit early
			}


			player.Dash();
		}

		public override void OnContact(Player player, Collider other)
		{

		}
	}
}
