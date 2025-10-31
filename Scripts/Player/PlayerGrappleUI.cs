using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 

namespace PLAYERTWO.PlatformerProject
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Grapple UI")]
    [RequireComponent(typeof(PlayerInputManager))] 
    public class PlayerGrappleUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The UI Image component for the crosshair.")]
        public Image crosshairImage;

        [Header("Crosshair Sprites")]
        public Sprite defaultCrosshair;
        public Sprite lockedCrosshair;

        [Header("Aim Settings")]
        [Tooltip("How fast the crosshair moves when using a gamepad stick.")]
        [SerializeField] private float gamepadAimSensitivity = 300f;
        
        [Tooltip("How sensitive the mouse input is.")]
        [SerializeField] private float mouseAimSensitivity = 1.0f;

        // --- NEW ---
        protected PlayerInputManager _inputManager;
        protected RectTransform _crosshairRect; 
        
        // This is the new "source of truth" for the crosshair's position
        private Vector2 _currentCrosshairPos;

        /// <summary>
        /// The current, clamped screen position of the crosshair.
        /// Other scripts (like AimingGrapplePlayerState) will read this.
        /// </summary>
        public Vector2 CurrentCrosshairScreenPosition { get; private set; }
        // --- END NEW ---

        protected virtual void Start()
        {
            _inputManager = GetComponent<PlayerInputManager>();
            
            if (crosshairImage == null)
            {
                Debug.LogError("GrappleCrosshair UI Image is not assigned!", this);
                enabled = false;
                return;
            }
            
            _crosshairRect = crosshairImage.GetComponent<RectTransform>();
            _crosshairRect.pivot = new Vector2(0.5f, 0.5f); // Ensure pivot is center

            crosshairImage.gameObject.SetActive(false);
        }

        protected virtual void LateUpdate()
        {
            if (!crosshairImage.gameObject.activeInHierarchy)
                return;

            UpdateCrosshairPosition();
        }

        /// <summary>
        /// Moves the crosshair based on mouse or gamepad delta input.
        /// </summary>
        protected virtual void UpdateCrosshairPosition()
        {
            if (_inputManager == null || _crosshairRect == null) return;

            Vector2 lookDelta = Vector2.zero;
            bool isUsingMouse = _inputManager.IsLookingWithMouse();

            if (isUsingMouse)
            {
                // Read mouse delta (how much it moved this frame)
                lookDelta = Mouse.current.delta.ReadValue() * mouseAimSensitivity;
            }
            else
            {
                // Read gamepad stick input (this is an axis value, not a delta)
                // We use GetLookDirection() as it has deadzones applied
                Vector3 stickInput = _inputManager.GetLookDirection();
                // Convert (x, 0, z) from GetLookDirection to (x, y) for screen space
                Vector2 stickDelta = new Vector2(stickInput.x, stickInput.z);
                
                // Scale by sensitivity and time to get a per-second rate
                lookDelta = stickDelta * gamepadAimSensitivity * Time.deltaTime;
            }

            // Add the delta to our current position
            _currentCrosshairPos += lookDelta;

            // Clamp the position to keep it on-screen
            _currentCrosshairPos.x = Mathf.Clamp(_currentCrosshairPos.x, 0, Screen.width);
            _currentCrosshairPos.y = Mathf.Clamp(_currentCrosshairPos.y, 0, Screen.height);

            // Update the public property and the UI's actual position
            CurrentCrosshairScreenPosition = _currentCrosshairPos;
            _crosshairRect.position = CurrentCrosshairScreenPosition;
        }

        /// <summary>
        /// Shows or hides the crosshair UI element and manages the cursor state.
        /// </summary>
        public void ShowCrosshair(bool show)
        {
            if (crosshairImage != null)
            {
                crosshairImage.gameObject.SetActive(show);
            }

            if (show)
            {
                // --- INITIALIZE CROSSHAIR POSITION ---
                // If we are starting aim, snap the crosshair to the center
                // or to the current mouse position.
                if (_inputManager.IsLookingWithMouse())
                {
                    _currentCrosshairPos = Mouse.current.position.ReadValue();
                }
                else
                {
                    _currentCrosshairPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
                
                // Run one update immediately to place it
                UpdateCrosshairPosition();

                // --- SET CURSOR STATE FOR AIMING ---
                // We lock the cursor to center and hide it, 
                // so we only see our UI crosshair.
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // --- RESTORE CURSOR STATE ---
                // When we stop aiming, re-lock and hide the cursor for normal gameplay.
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        /// <summary>
        /// Sets the crosshair's sprite to the 'locked' or 'default' version.
        /// </summary>
        public void SetCrosshairLocked(bool isLocked)
        {
            if (crosshairImage == null) return;
            Sprite targetSprite = isLocked ? lockedCrosshair : defaultCrosshair;
            if (crosshairImage.sprite != targetSprite)
            {
                crosshairImage.sprite = targetSprite;
            }
        }
    }
}