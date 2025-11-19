using UnityEngine;
using UnityEngine.Splines; // Required for Spline logic

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Follow Enemy State")]
	public class FollowEnemyState : EnemyState
	{
		protected float m_returnStateTimer;

		protected override void OnEnter(Enemy enemy)
		{
			m_returnStateTimer = 0f;
			Debug.Log("Follow");
		}

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();

			// --- Handle target lost ---
			if (!enemy.player || !enemy.player.isAlive) // Added isAlive check
			{
				if (enemy.stats.current.returnToLastStateWhenLostTarget)
				{
					m_returnStateTimer += Time.deltaTime;
					//enemy.Decelerate(enemy.stats.current.deceleration);

					// if (m_returnStateTimer >= enemy.stats.current.returnToLastStateDelay)
					// 	enemy.states.Change(enemy.states.last);
				}
				else
				{
					// If not returning, just decelerate
					enemy.Decelerate(enemy.stats.current.deceleration);
				}
				return;
			}
			
			// --- MODIFIED FOLLOW LOGIC ---
			bool playerOnSpline = enemy.player.splinePath != null;
			bool enemyOnSpline = enemy.splinePath != null;

			// Check if both are on the *same* spline
			if (playerOnSpline && enemyOnSpline && enemy.player.splinePath == enemy.splinePath)
			{
				// --- ON-SPLINE FOLLOW LOGIC ---
				SplinePath spline = enemy.splinePath;
				
				// Get 't' (normalized position) for both
				Vector3 playerTangent = spline.GetSplineTangentFrom(enemy.player.position, out _, out float playerT);
				Vector3 enemyTangent = spline.GetSplineTangentFrom(enemy.position, out _, out float enemyT);

				Vector3 desiredDirection = Vector3.zero;
				float distance = Mathf.Abs(playerT - enemyT);
				Debug.Log(distance);
				// Don't move if close enough
				if (distance > 0.01f)
				{
					// Use the spline's "forward" tangent as the base direction
					// This logic correctly follows the player around the spline
					if (playerT > enemyT)
						desiredDirection = enemyTangent; // Move "forward" along spline
					else
						desiredDirection = -enemyTangent; // Move "backward" along spline
				}
				
				// Convert world-space tangent to enemy's local-space for acceleration
				var localDirection = Quaternion.FromToRotation(enemy.transform.up, Vector3.up) * desiredDirection;

				if (desiredDirection.sqrMagnitude > 0)
				{
					enemy.Accelerate(localDirection.normalized, enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed);
					enemy.FaceDirectionSmooth(localDirection.normalized);
				}
				else
				{
					// Close enough, stop moving
					enemy.Decelerate(enemy.stats.current.deceleration);
				}
			}
			else
			{
				// --- DEFAULT 3D FOLLOW LOGIC ---
				// This will also handle the enemy moving *towards* the player on the spline
				var head = enemy.player.position - enemy.position;
				var upOffset = Vector3.Dot(enemy.transform.up, head);
				var direction = head - enemy.transform.up * upOffset;
				var localDirection = Quaternion.FromToRotation(enemy.transform.up, Vector3.up) * direction;

				localDirection = localDirection.normalized;

				enemy.Accelerate(localDirection, enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed);
				enemy.FaceDirectionSmooth(localDirection);
			}
			// --- END MODIFIED LOGIC ---
		}

		public override void OnContact(Enemy enemy, Collider other) { }
	}
}