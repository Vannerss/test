using UnityEngine;
using Animancer; // Required for Animancer
using PLAYERTWO.PlatformerProject; // Required for Player, PlayerState, etc.
using System; // Required for Type comparison

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(AnimancerComponent))] // Require AnimancerComponent
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Animancer")]
public class PlayerAnimancer : MonoBehaviour
{
    [SerializeField] private AnimancerComponent _animancer;
    [SerializeField] private Player _player;

    [Header("Mixers")]
    [Tooltip("Blend: Idle, Walk, Run")]
    [SerializeField] private LinearMixerTransition _locomotionMixer;
    [Tooltip("Blend: HoldIdle, HoldWalk")]
    [SerializeField] private LinearMixerTransition _holdLocomotionMixer;
    [Tooltip("Blend: CrouchIdle, Crawl")]
    [SerializeField] private LinearMixerTransition _crouchMixer;
    [Tooltip("Blend: SwimIdle, SwimMove")]
    [SerializeField] private LinearMixerTransition _swimMixer;
    [Tooltip("Blend: PoleClimbIdle, PoleClimbMove")]
    [SerializeField] private LinearMixerTransition _poleClimbMixer;
    [Tooltip("Blend: LedgeHangIdle, LedgeHangMove")]
    [SerializeField] private LinearMixerTransition _ledgeHangMixer;
    
    [Header("Animation Clips")]

    #region Clips
    [SerializeField] private ClipTransition _idleAnimation;
    [SerializeField] private ClipTransition _walkAnimation;
    [SerializeField] private ClipTransition _runAnimation; 
    [SerializeField] private ClipTransition _brakeAnimation;
    [SerializeField] private ClipTransition _fallAnimation;
    [SerializeField] private ClipTransition _crouchIdleAnimation; 
    [SerializeField] private ClipTransition _crawlAnimation;
    
    [SerializeField] private ClipTransition _jumpAnimation;
    [SerializeField] private ClipTransition _spinAnimation;
    [SerializeField] private ClipTransition _airDiveAnimation;
    [SerializeField] private ClipTransition _stompStartAnimation; 
    [SerializeField] private ClipTransition _stompFallAnimation;
    [SerializeField] private ClipTransition _stompLandAnimation;
    [SerializeField] private ClipTransition _backflipAnimation;
    [SerializeField] private ClipTransition _dashAnimation;
    [SerializeField] private ClipTransition _glideAnimation;
    
    [SerializeField] private ClipTransition _ledgeHangIdleAnimation;
    [SerializeField] private ClipTransition _ledgeHangMoveAnimation; 
    [SerializeField] private ClipTransition _ledgeClimbAnimation;
    [SerializeField] private ClipTransition _wallDragAnimation;
    [SerializeField] private ClipTransition _wallClimbAnimation; 
    [SerializeField] private ClipTransition _poleClimbIdleAnimation;
    [SerializeField] private ClipTransition _poleClimbMoveAnimation; 
    [SerializeField] private ClipTransition _swimIdleAnimation;
    [SerializeField] private ClipTransition _swimMoveAnimation; 
    [SerializeField] private ClipTransition _railGrindAnimation;
    
    [SerializeField] private ClipTransition _hurtAnimation;
    [SerializeField] private ClipTransition _dieAnimation;
    [SerializeField] private ClipTransition _holdIdleAnimation;
    [SerializeField] private ClipTransition _holdWalkAnimation;
    #endregion

    [Header("Settings")]
    [Tooltip("Default fade duration for transitions.")]
    [SerializeField] private float _defaultFadeDuration = 0.25f;
    [Tooltip("Minimum speed multiplier for walk/run animations based on player speed.")]
    [SerializeField] private float _minLateralAnimSpeed = 0.5f;

    private AnimancerState _currentState;

    protected virtual void Start()
    {
        // Get references if not assigned in Inspector
        if (_player == null)
            _player = GetComponent<Player>();
        if (_animancer == null)
            _animancer = GetComponent<AnimancerComponent>();
        
        // Subscribe to state changes
        if (_player != null && _player.states != null && _player.states.events != null)
        {
            _player.states.events.onChange.AddListener(OnStateChanged);
        }
        else
        {
            Debug.LogError("Player, PlayerStateManager, or events not found!", this);
        }
        
        OnStateChanged();
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (_player != null && _player.states != null && _player.states.events != null)
        {
            _player.states.events.onChange.RemoveListener(OnStateChanged);
        }
    }

    protected virtual void LateUpdate()
    {
        // Update animation parameters like speed based on the *current* Animancer state
        // and player data. This is simpler than Mecanim parameters.
        UpdateAnimationSpeed();
        // Add other parameter updates if needed (e.g., blend values for mixers)
    }

    /// <summary>
    /// Called when the Player's state changes. Plays the corresponding animation.
    /// </summary>
    private void OnStateChanged()
    {
        if (_player == null || _player.states == null || _player.states.current == null) return;

        Type currentStateType = _player.states.current.GetType();
        ClipTransition targetAnimation = GetAnimationForState(currentStateType);

        if (targetAnimation != null && targetAnimation.Clip != null)
        {
            _currentState = _animancer.Play(targetAnimation, _defaultFadeDuration);
        }
        else
        {
            // Fallback or handle missing animations
            Debug.LogWarning($"No animation clip assigned for state: {currentStateType.Name}", this);
            _currentState = _animancer.Play(_idleAnimation, _defaultFadeDuration);
        }
    }
    
    /// <summary>
    /// Maps a PlayerState Type to its corresponding animation clip.
    /// </summary>
    private ClipTransition GetAnimationForState(Type stateType)
    {
        // --- Locomotion ---
        if (stateType == typeof(IdlePlayerState)) return _player.holding ? _holdIdleAnimation : _idleAnimation;
        if (stateType == typeof(WalkPlayerState)) return _player.holding ? _holdWalkAnimation : _walkAnimation;
        // Add RunPlayerState if you implement it
        if (stateType == typeof(BrakePlayerState)) return _brakeAnimation;
        if (stateType == typeof(FallPlayerState)) return _fallAnimation;
        if (stateType == typeof(CrouchPlayerState)) return _crouchIdleAnimation; // Assuming crouch starts idle
        if (stateType == typeof(CrawlingPlayerState)) return _crawlAnimation;

        // --- Actions ---
        if (stateType == typeof(SpinPlayerState)) return _spinAnimation;
        if (stateType == typeof(AirDivePlayerState)) return _airDiveAnimation;
        if (stateType == typeof(StompPlayerState)) return _stompStartAnimation; // Stomp needs careful handling, maybe sub-states or checks within the state
        if (stateType == typeof(BackflipPlayerState)) return _backflipAnimation;
        if (stateType == typeof(DashPlayerState)) return _dashAnimation;
        if (stateType == typeof(GlidingPlayerState)) return _glideAnimation;

        // --- Interactions ---
        if (stateType == typeof(LedgeHangingPlayerState)) return _ledgeHangIdleAnimation; // Needs logic for movement animation
        if (stateType == typeof(LedgeClimbingPlayerState)) return _ledgeClimbAnimation;
        if (stateType == typeof(WallDragPlayerState)) return _wallDragAnimation;
        if (stateType == typeof(WallClimbPlayerState)) return _wallClimbAnimation;
        if (stateType == typeof(PoleClimbingPlayerState)) return _poleClimbIdleAnimation; // Needs logic for movement animation
        if (stateType == typeof(SwimPlayerState)) return _swimIdleAnimation; // Needs logic for movement animation
        if (stateType == typeof(RailGrindPlayerState)) return _railGrindAnimation;
		if (stateType == typeof(GrapplePlayerState)) return _fallAnimation; //TODO: add grapple animation
		if (stateType == typeof(AimingGrapplePlayerState)) return _idleAnimation; //TODO: add aiming grapple animation

        // --- Misc ---
        if (stateType == typeof(HurtPlayerState)) return _hurtAnimation;
        if (stateType == typeof(DiePlayerState)) return _dieAnimation;
        

        return null;
    }

    /// <summary>
    /// Adjusts the playback speed of movement animations based on player velocity.
    /// </summary>
    private void UpdateAnimationSpeed()
    {
        if (_currentState == null || !_currentState.IsPlaying) return;

        // Adjust speed for animations where it makes sense (Walk, Run, Crawl, etc.)
        bool isMovingState = _currentState.Clip == _walkAnimation.Clip ||
                             _currentState.Clip == _runAnimation.Clip ||
                             _currentState.Clip == _crawlAnimation.Clip ||
                             _currentState.Clip == _holdWalkAnimation.Clip; // Add other relevant clips

        if (isMovingState)
        {
            float lateralSpeed = _player.lateralVelocity.magnitude;
            float topSpeed = _player.running ? _player.stats.current.runningTopSpeed : _player.stats.current.topSpeed; // Adjust based on state if needed
            if (topSpeed > 0.1f) // Avoid division by zero or tiny numbers
            {
                 // Normalize speed relative to the current max speed for that state
                _currentState.Speed = Mathf.Max(_minLateralAnimSpeed, lateralSpeed / topSpeed);
            }
            else
            {
                _currentState.Speed = _minLateralAnimSpeed; // Or 1.0f if preferred when stationary
            }
        }
        else
        {
            // Reset speed for non-movement animations or handle specific cases
             _currentState.Speed = 1.0f;

             // Example: Speed up rail grind based on velocity?
             if (_currentState.Clip == _railGrindAnimation.Clip)
             {
                 float grindSpeed = _player.velocity.magnitude; // Rough speed along rail
                 float maxGrindSpeed = _player.stats.current.grindTopSpeed;
                 if (maxGrindSpeed > 0.1f) {
                     _currentState.Speed = Mathf.Clamp(grindSpeed / maxGrindSpeed, 0.5f, 2.0f); // Example range
                 } else {
                      _currentState.Speed = 1.0f;
                 }
             }

             // Add logic for Ledge Hang Move, Pole Climb Move, Swim Move if using mixers or speed adjustment
        }
    }
}