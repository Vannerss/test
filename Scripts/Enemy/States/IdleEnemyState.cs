using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Idle Enemy State")]
	public class IdleEnemyState : EnemyState
	{
		protected override void OnEnter(Enemy enemy)
		{
			Debug.Log("Idle");
		}

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();
			enemy.Friction();

			if (Vector3.Distance(enemy.transform.position, enemy.player.transform.position) < 100f)
			{
				enemy.states.Change<FollowEnemyState>();
			}
		}

		public override void OnContact(Enemy enemy, Collider other) { }
	}
}
