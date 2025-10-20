using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class WallClimbPlayerState : PlayerState
    {
        // Distance along wall normal from player to wall, cached on enter
        protected float m_collisionDistance;

        // 0..1 percentages inside wall area (like UVs) from last clamp
        protected float m_wallHPercentage; // across width (x)
        protected float m_wallVPercentage; // across height (y)

        protected const float k_wallOffset = 0.01f; // tiny hover from wall plane
        
        protected override void OnEnter(Player player)
        {
            player.ResetJumps();
            player.ResetAirSpins();
            player.ResetAirDash();
            
            player.velocity = Vector3.zero;
            player.climbWall.GetDirectionToWall(player.transform, out m_collisionDistance);
            player.climbWall.RotateToWall(player.transform);

            //player.skin.position += player.transform.rotation * player.stats.current.poleClimbSkinOffset;
        }

        protected override void OnExit(Player player)
        {
            //player.skin.position -= player.transform.rotation * player.stats.current.poleClimbSkinOffset;

            ResetUpAlignment(player);
        }

        protected override void OnStep(Player player)
        {
            var dirToWall = player.climbWall.GetDirectionToWall(player.transform);
            var inputDir = player.inputs.GetMovementDirection();

            HandleHorizontalMovement(player, inputDir);
            HandleVerticalMovement(player, inputDir);

            if (player.inputs.GetJumpDown())
            {
                var localAway = -dirToWall;
                player.FaceDirection(localAway);
                player.DirectionalJump(
                    localAway,
                    player.stats.current.wallJumpHeight,
                    player.stats.current.wallJumpDistance);
                player.states.Change<FallPlayerState>();
                return;
            }

            if (player.isGrounded && inputDir.z < 0)
            {
                player.states.Change<IdlePlayerState>();
                return;
            }

            player.climbWall.RotateToWall(player.transform);

            player.FaceDirection(player.climbWall.normal);

            var horizontalPad = player.radius;
            var verticalPad = player.height * 0.5f + player.center.y;

            var stickDistance = m_collisionDistance + k_wallOffset;

            var clampedPos = player.climbWall.GetClampedStickPoint(
                player.transform.position,
                stickDistance,
                new Vector2(horizontalPad, verticalPad),
                out var t //(x,y) 0..1 across wall
            );

            m_wallHPercentage = t.x;
            m_wallVPercentage = t.y;

            player.transform.position = clampedPos;
        }

        public override void OnContact(Player player, Collider other) { }

        protected virtual void HandleVerticalMovement(Player player, Vector3 inputDirection)
        {
            var speed = player.verticalVelocity.y;
            var upAccel = player.stats.current.climbUpAcceleration;
            var downAccel = player.stats.current.climbDownAcceleration;
            var friction = player.stats.current.climbFriction;

            var climbingUp = inputDirection.z > 0 && m_wallVPercentage < 1f;
            var climbingDown = inputDirection.z < 0 && m_wallVPercentage > 0f;

            if (climbingUp)
                speed += upAccel * Time.deltaTime;
            else if (climbingDown)
                speed -= downAccel * Time.deltaTime;

            if ((!climbingUp && !climbingDown) || Mathf.Sign(speed) != Mathf.Sign(inputDirection.z))
                speed = Mathf.MoveTowards(speed, 0, friction * Time.deltaTime);

            speed = Mathf.Clamp(
                speed,
                -player.stats.current.climbDownTopSpeed,
                player.stats.current.climbUpTopSpeed
            );

            player.verticalVelocity = Vector3.up * speed;
        }
        
        protected virtual void HandleHorizontalMovement(Player player, Vector3 inputDirection)
        {
            var speed = Vector3.Dot(player.lateralVelocity, player.localRight);
            var friction = player.stats.current.climbFriction;
            var topSpeed = player.stats.current.climbRotationTopSpeed;
            var accel = player.stats.current.climbRotationAcceleration;

            if (inputDirection.x > 0)
                speed += accel * Time.deltaTime;
            else if (inputDirection.x < 0)
                speed -= accel * Time.deltaTime;

            if (Mathf.Approximately(inputDirection.x, 0f) || Mathf.Sign(speed) != Mathf.Sign(inputDirection.x))
                speed = Mathf.MoveTowards(speed, 0, friction * Time.deltaTime);

            speed = Mathf.Clamp(speed, -topSpeed, topSpeed);

            // Move along player's localRight (aligned with wall right due to RotateToWall)
            player.lateralVelocity = player.localRight * speed;
        }
        
        protected virtual void ResetUpAlignment(Player player)
        {
            if (player.gravityField)
                return;

            var target = Quaternion.FromToRotation(player.transform.up, Vector3.up);
            player.transform.rotation = target * player.transform.rotation;
        }
    }
}