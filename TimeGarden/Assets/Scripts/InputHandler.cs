using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    #region Stick origin
    public Vector2 GetAngleOrigin(StickOrigin origin) => origin switch
    {
        StickOrigin.Up => Vector2.up,
        StickOrigin.Right => Vector2.right,
        StickOrigin.Down => Vector2.down,
        StickOrigin.Left => Vector2.left,
        _ => Vector2.zero
    };
    public enum StickOrigin
    {
        Up, Right, Down, Left
    }
    #endregion

    /// <summary>
    /// The current time of the world, in hours.
    /// </summary>
    public static float Time => time * multiplier;
    private static float time;

    private static float multiplier;
    public static void SetMultiplier(float multiplier)
    {
        InputHandler.multiplier = multiplier;
    }

    public static Action<float> OnTimeUpdate = (_) => { };

    public StickOrigin Origin;
    public bool Reversed;

    private InputAction stick;
    private float currentAngle;

    private void Awake()
    {
        stick = InputSystem.actions.FindActionMap("Player").FindAction("Stick");
    }
    private void OnEnable()
    {
        stick.started += OnStarted;
        stick.performed += OnPerformed;
        stick.canceled += OnCancelled;
    }
    private void OnDisable()
    {
        stick.started -= OnStarted;
        stick.performed -= OnPerformed;
        stick.canceled -= OnCancelled;
    }
    void OnStarted(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        if (value.sqrMagnitude < 0.1f) return;

        Process(value);
    }
    void OnPerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        if (value.sqrMagnitude < 0.1f) return;

        Process(value);
    }
    void OnCancelled(InputAction.CallbackContext context)
    {
        return;
    }

    void Process(Vector2 value)
    {
        float angle = Vector2.SignedAngle(GetAngleOrigin(Origin), value.normalized);
        if (Reversed) angle *= -1;

        //Remap the range (0, -180) => (0, 180) and (180, 0) => (180, 360)
        //There are smart ways to do this but im not wasting time on them
        if (angle < 0)
        {
            OnNewAngle(Mathf.InverseLerp(0, -180f, angle) * 180f);
        }
        else
        {
            OnNewAngle((Mathf.InverseLerp(180, 0, angle) * 180f) + 180f);
        }
    }

    void OnNewAngle(float rotation)
    {
        float previousRotation = ModuloWithNegative(currentAngle, 360f);
        int revolutions = Mathf.FloorToInt(currentAngle / 360f);

        if (previousRotation > 270 && rotation < 90)
        {
            revolutions++;
        }
        else if (previousRotation < 90 && rotation > 270)
        {
            revolutions--;
        }
        currentAngle = revolutions * 360f + rotation;

        float hour = currentAngle / 30f;

        float prevTime = time;
        time = hour;
        OnTimeUpdate.Invoke(time - prevTime);
    }
    float ModuloWithNegative(float target, float value)
    {
        if (value <= 0) return target;
        while (target < 0) target += value;

        return target % value;
    }
}
