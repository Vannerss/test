using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(SplineContainer))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Spline Path")]
	public class SplinePath : MonoBehaviour
	{
		public struct SplineState
		{
			public bool backwards;
		}

		[SerializeField]
		protected Collider m_collider;

		[SerializeField]
		protected SplineContainer m_splineContainer;

		public int splineResolution = 128;

		protected Player m_tempPlayer;
		protected Enemy m_tempEnemy; // Added for Enemy
		
		// Made public for FollowEnemyState to access
		public Dictionary<Player, SplineState> m_splineStates = new();
		public Dictionary<Enemy, SplineState> m_enemySplineStates = new(); // Added for Enemy

		protected virtual void InitializeCollider()
		{
			if (m_collider == null)
				m_collider = GetComponent<Collider>();

			m_collider.isTrigger = true;
		}

		protected virtual void InitializeSplineContainer()
		{
			if (m_splineContainer == null)
				m_splineContainer = GetComponent<SplineContainer>();
		}

		public virtual void AssignPlayer(Player player)
		{
			if (m_splineStates.ContainsKey(player) || !player.isAlive)
				return;

			var forward = GetSplineTangentFrom(player.position, out _, out _); // Updated call
			var dot = Vector3.Dot(forward, player.pathForward);

			m_splineStates.Add(player, new SplineState() { backwards = dot < 0f });
			player.splinePath = this;
		}
		
		// --- NEW ENEMY METHOD ---
		public virtual void AssignEnemy(Enemy enemy)
		{
			if (m_enemySplineStates.ContainsKey(enemy) || !enemy.isAlive)
				return;

			var forward = GetSplineTangentFrom(enemy.position, out _, out _);
			var dot = Vector3.Dot(forward, enemy.pathForward);

			m_enemySplineStates.Add(enemy, new SplineState() { backwards = dot < 0f });
			enemy.splinePath = this;
		}
		// --- END NEW ENEMY METHOD ---

		public virtual void RemovePlayer(Player player)
		{
			if (!m_splineStates.ContainsKey(player))
				return;

			player.pathForward = GetPathForward(player, out _);
			m_splineStates.Remove(player);
			player.splinePath = null;
		}
		
		// --- NEW ENEMY METHOD ---
		public virtual void RemoveEnemy(Enemy enemy)
		{
			if (!m_enemySplineStates.ContainsKey(enemy))
				return;

			enemy.pathForward = GetPathForward(enemy, out _);
			m_enemySplineStates.Remove(enemy);
			enemy.splinePath = null;
		}
		// --- END NEW ENEMY METHOD ---

		public virtual Vector3 GetPathForward(Player player, out Vector3 closestPoint)
		{
			var tangent = GetSplineTangentFrom(player.position, out closestPoint, out _); // Updated call
			var sign = m_splineStates[player].backwards ? -1f : 1f;
			return tangent * sign;
		}
		
		// --- NEW ENEMY OVERLOAD ---
		public virtual Vector3 GetPathForward(Enemy enemy, out Vector3 closestPoint)
		{
			var tangent = GetSplineTangentFrom(enemy.position, out closestPoint, out _);
			var sign = m_enemySplineStates[enemy].backwards ? -1f : 1f;
			return tangent * sign;
		}
		// --- END NEW ENEMY OVERLOAD ---

		// --- MODIFIED to return 't' ---
		public virtual Vector3 GetSplineTangentFrom(Vector3 point, out Vector3 closestPoint, out float t)
		{
			SplineUtility.GetNearestPoint(
				m_splineContainer.Spline,
				transform.InverseTransformPoint(point),
				out var nearest,
				out t,
				splineResolution
			);
			closestPoint = transform.TransformPoint(nearest);
			return Vector3.Normalize(m_splineContainer.EvaluateTangent(t));
		}
		// --- END MODIFICATION ---

		protected virtual Vector3 GetSplineTangentFrom(Vector3 point, out Vector3 closestPoint)
		{
			SplineUtility.GetNearestPoint(
				m_splineContainer.Spline,
				transform.InverseTransformPoint(point),
				out var nearest,
				out var t,
				splineResolution
			);
			closestPoint = transform.TransformPoint(nearest);
			return Vector3.Normalize(m_splineContainer.EvaluateTangent(t));
		}
		
		
		protected virtual void Awake()
		{
			InitializeCollider();
			InitializeSplineContainer();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			// Updated to check for both Player and Enemy
			if (GameTags.IsPlayer(other) && other.TryGetComponent(out m_tempPlayer))
				AssignPlayer(m_tempPlayer);
			else if (GameTags.IsEnemy(other) && other.TryGetComponent(out m_tempEnemy))
				AssignEnemy(m_tempEnemy);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			// Updated to check for both Player and Enemy
			if (GameTags.IsPlayer(other) && other.TryGetComponent(out m_tempPlayer))
				RemovePlayer(m_tempPlayer);
			else if (GameTags.IsEnemy(other) && other.TryGetComponent(out m_tempEnemy))
				RemoveEnemy(m_tempEnemy);
		}
	}
}