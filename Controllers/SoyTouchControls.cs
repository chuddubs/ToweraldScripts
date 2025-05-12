using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Finger = UnityEngine.InputSystem.EnhancedTouch.Finger;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SoyTouchControls : MonoBehaviour
{
    public SoyCharacterController soyak;

    private Finger movementFinger;
    private Vector2 movementStartPos;
    private float movementTouchStartTime;

    private PlayerInput controls;

    // private Dictionary<int, Vector2> fingerStartPositions = new();
    // private Dictionary<int, float> fingerStartTimes = new();

    // private const float TapTimeThreshold = 0.2f;         // Seconds
    // private const float TapMoveThreshold = 20f;          // Pixels

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
        controls = new PlayerInput();
        SoyClimber climber = soyak.GetComponent<SoyClimber>();

        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Shoot.performed += ctx => climber.ShootGreenArrow();
        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void TogglePause()
    {
        if (!SoyGameController.InstanceExists)
            return;
        SoyGameController.Instance.TogglePause();
    }

    private void OnEnable()
    {
        controls.Enable();
        Touch.onFingerDown += HandleFingerDown;
        Touch.onFingerUp += HandleFingerUp;
    }

    private void OnDisable()
    {
        controls.Disable();
        Touch.onFingerDown -= HandleFingerDown;
        Touch.onFingerUp -= HandleFingerUp;
    }

    private void HandleFingerDown(Finger finger)
    {
        if (SoyGameController.Instance.IsPaused)
            return;

        if (IsFingerOverUI(finger))
            return;

        if (movementFinger == null)
        {
            movementFinger = finger;
            movementStartPos = finger.screenPosition;
            movementTouchStartTime = Time.time;
        }
    }

    private void HandleFingerUp(Finger finger)
    {
        if (SoyGameController.Instance.IsPaused)
            return;

        if (finger == movementFinger)
        {
            movementFinger = null;
            soyak.StopMoving();
        }
    }

    private bool IsFingerOverUI(Finger finger)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = finger.screenPosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private void Jump()
    {
        soyak.TryJump();
    }

    private float GetVeloFromRelativePos(float touchX)
    {
        float distanceForDeceleration = Screen.width / 6f;
        Vector2 soyPos = Camera.main.WorldToScreenPoint(soyak.transform.position);
        float dist = touchX - soyPos.x;
        float lerpValue = Mathf.InverseLerp(0.0f, distanceForDeceleration, Mathf.Abs(dist));
        return Mathf.Sign(dist) * lerpValue;
    }

    private void Update()
    {
        if (SoyGameController.Instance.IsPaused)
            return;

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (moveInput.x != 0)
        {
            soyak.Move(moveInput.x * 0.5f);
        }
        else if (movementFinger != null)
        {
            float velocity = GetVeloFromRelativePos(movementFinger.screenPosition.x);
            soyak.Move(velocity);
        }
        else
        {
            soyak.StopMoving();
        }
    }

}
