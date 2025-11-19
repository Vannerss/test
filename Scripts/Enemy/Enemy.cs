using UnityEngine;
using UnityEngine.Splines; // Required for SplinePath

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(EnemyStatsManager))]
	[RequireComponent(typeof(EnemyStateManager))]
	[RequireComponent(typeof(WaypointManager))]
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy")]
	public class Enemy : Entity<Enemy>
	{
		[Header("Enemy Settings")]
		public EnemyEvents enemyEvents;

		[Tooltip("Force applied to snap the Enemy to the center of the current spline path.")]
		public float snapToPathForce = 10f; // Added for spline following

		protected Player m_player;

		protected Collider[] m_sightOverlaps = new Collider[1024];
		protected Collider[] m_contactAttackOverlaps = new Collider[1024];

		// --- NEW SPLINE PROPERTIES ---
		/// <summary>
		/// Returns the Spline Path instance in which the Enemy is following.
		/// </summary>
		public SplinePath splinePath { get; set; }

		protected Vector3 m_pathForward;
		protected Vector3 m_closestPointOnPath;
		// --- END NEW SPLINE PROPERTIES ---

		/// <summary>
		/// Returns the Enemy Stats Manager instance.
		/// </summary>
		public EnemyStatsManager stats { get; protected set; }

		/// <summary>
		/// Returns the Waypoint Manager instance.
		/// </summary>
		public WaypointManager waypoints { get; protected set; }

		/// <summary>
		/// Returns the Health instance.
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// Returns the instance of the Player on the Enemies sight.
		/// </summary>
		public Player player { get; protected set; }

		/// <summary>
		/// Returns true if the Enemy health is not empty.
		/// </summary>
		public virtual bool isAlive => !health.isEmpty; // Added for SplinePath check

		/// <summary>
		/// Returns the current forward direction of the Enemy's path.
		/// </summary>
		public virtual Vector3 pathForward // Added to match Player functionality
		{
			set { m_pathForward = value; }
			get
			{
				if (splinePath)
					return splinePath.GetPathForward(this, out m_closestPointOnPath);
				
				if (m_pathForward == Vector3.zero)
					m_pathForward = transform.forward;

				return m_pathForward;
			}
		}

		protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();
		protected virtual void InitializeWaypointsManager() => waypoints = GetComponent<WaypointManager>();
		protected virtual void InitializeHealth() => health = GetComponent<Health>();
		protected virtual void InitializeTag() => tag = GameTags.Enemy;

		/// <summary>
		/// Applies damage to this Enemy decreasing its health with proper reaction.
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (!health.isEmpty && !health.recovering)
			{
				health.Damage(amount);
				enemyEvents.OnDamage?.Invoke();

				if (health.isEmpty)
				{
					controller.enabled = false;
					enemyEvents.OnDie?.Invoke();
					
					// --- NEW ---
					// Ensure enemy is removed from spline on death
					if (splinePath != null)
					{
						splinePath.RemoveEnemy(this);
					}
					// --- END NEW ---
				}
			}
		}

		/// <summary>
		/// Revives this enemy, restoring its health and reenabling its movements.
		/// </summary>
		public virtual void Revive()
		{
			if (!health.isEmpty) return;

			health.ResetHealth();
			controller.enabled = true;
			enemyEvents.OnRevive?.Invoke();
		}

		public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
			Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its deceleration stats.
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// </summary>
		public virtual void Friction() => Decelerate(stats.current.friction);

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// </summary>
		public virtual void Gravity() => Gravity(stats.current.gravity);

		/// <summary>
		/// Applies a downward force when ground by its snap stats.
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// Rotate the Enemy forward to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

		public virtual void ContactAttack(Collider other)
		{
			if (!other.CompareTag(GameTags.Player)) return;
			if (!other.TryGetComponent(out Player player)) return;

			var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

			if (player.isGrounded || !BoundsHelper.IsBellowPoint(controller.collider, stepping))
			{
				if (stats.current.contactPushback)
					lateralVelocity = -localForward * stats.current.contactPushBackForce;

				player.ApplyDamage(stats.current.contactDamage, transform.position);
				enemyEvents.OnPlayerContact?.Invoke();
			}
		}

		/// <summary>
		/// Handles the view sight and Player detection behaviour.
		/// </summary>
		protected virtual void HandleSight()
		{
			if (!player)
			{
				var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);

				for (int i = 0; i < overlaps; i++)
				{
					if (m_sightOverlaps[i].CompareTag(GameTags.Player))
					{
						if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
						{
							this.player = player;
							OnPlayerSpotted();
							enemyEvents.OnPlayerSpotted?.Invoke();
							return;
						}
					}
				}
			}
			else
			{
				var distance = Vector3.Distance(position, player.position);

				if ((!player.isAlive) || (distance > stats.current.viewRange)) // Used player.isAlive
				{
					player = null;
					enemyEvents.OnPlayerScaped?.Invoke();
				}
			}
		}
		
		// --- NEW METHODS ---
		
		/// <summary>
		/// Keeps the Enemy snapped to the SplinePath.
		/// </summary>
		protected virtual void AdjustToPath()
		{
			if (!splinePath || onRails)
				return;

			var pathDirection = m_closestPointOnPath - position;
			var adjustDelta = snapToPathForce * Time.deltaTime;

			pathDirection -= Vector3.Dot(pathDirection, transform.up) * transform.up;
			pathDirection -= Vector3.Dot(pathDirection, pathForward) * pathForward;
			position = Vector3.MoveTowards(position, position + pathDirection, adjustDelta);
		}

		/// <summary>
		/// Overridden to also call AdjustToPath, mimicking Player.cs.
		/// </summary>
		protected override void HandleSpline()
		{
			base.HandleSpline(); // Handles rail logic
			AdjustToPath(); // Handles SplinePath logic
		}
		
		// --- END NEW METHODS ---

		protected virtual void OnPlayerSpotted()
		{
			if (stats.current.followTargetOnSight)
				states.Change<FollowEnemyState>();
		}

		protected override void OnUpdate()
		{
			HandleSight();
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeTag();
			InitializeStatsManager();
			InitializeWaypointsManager();
			InitializeHealth();
			m_pathForward = transform.forward; // Initialize pathForward
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			ContactAttack(other);
		}
	}
}