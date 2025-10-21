using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class WallClimbPlayerState : PlayerState
	{
		protected Vector3 m_wallNormal;
		
		protected const float k_wallStickForce = 5f;
		protected const float k_wallOffset = 0.01f;

		protected override void OnEnter(Player player)
		{
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
			player.velocity = Vector3.zero;

			player.climbWall.GetClosestSurfaceInfo(player.position, out var closestPoint, out m_wallNormal);
			player.transform.rotation = Quaternion.LookRotation(-m_wallNormal, player.climbWall.transform.up);
			
			Vector3 snapToPosition = closestPoint + m_wallNormal * (player.radius + k_wallOffset);
			player.controller.Move(snapToPosition - player.position);
		}

		protected override void OnExit(Player player)
		{
			ResetUpAlignment(player);
		}

		protected override void OnStep(Player player)
		{
			player.climbWall.GetClosestSurfaceInfo(player.position, out _, out m_wallNormal);
			player.transform.rotation = Quaternion.LookRotation(-m_wallNormal, player.climbWall.transform.up);

			var inputDir = player.inputs.GetMovementDirection();
			
			var verticalSpeed = inputDir.z * (inputDir.z > 0 ? player.stats.current.climbUpTopSpeed : player.stats.current.climbDownTopSpeed);
			var horizontalSpeed = inputDir.x * player.stats.current.climbRotationTopSpeed;

			Vector3 climbVelocity = (player.transform.up * verticalSpeed) + (player.transform.right * horizontalSpeed);
			Vector3 stickVelocity = -m_wallNormal * k_wallStickForce;

			player.velocity = climbVelocity + stickVelocity;
			
			// After the main update moves the player, snap them back to the wall face.
			Vector2 padding = new Vector2(player.radius, player.height * 0.5f);
			
			// You can now adjust padding.y here and it will work correctly!
			// For example: padding.y += 0.5f;
			padding.x -= 0.25f;
			padding.y -= 1.3f;

			Vector3 clampedPosition = player.climbWall.ClampPointToWallFace(player.position, m_wallNormal, padding, out var normalizedPos);
			player.controller.Move(clampedPosition - player.position);
			
			// NEW: Check for a ledge when moving up near the top
			if (inputDir.z > 0 && normalizedPos.y >= 0.98f)
			{
				if (player.TryLedgeGrabFromClimb())
				{
					return; // Exit early if we successfully grabbed a ledge
				}
			}
			
			// Handle Jump
			if (player.inputs.GetJumpDown())
			{
				player.DirectionalJump(
					m_wallNormal,
					player.stats.current.wallJumpHeight,
					player.stats.current.wallJumpDistance);
				player.states.Change<FallPlayerState>();
				return;
			}
			
			// Handle detaching from the bottom
			if (normalizedPos.y < 0.01f && player.isGrounded && inputDir.z < 0)
			{
				player.states.Change<IdlePlayerState>();
				return;
			}
		}

		public override void OnContact(Player player, Collider other) { }
		
		protected virtual void ResetUpAlignment(Player player)
		{
			if (player.gravityField)
				return;

			var target = Quaternion.FromToRotation(player.transform.up, Vector3.up);
			player.transform.rotation = target * player.transform.rotation;
		}
	}
}