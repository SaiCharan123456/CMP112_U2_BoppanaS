using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CameraController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] protected float rotationSensitivity = 100f;

    [Header("Pitch (X) Limits")]
    [SerializeField] protected float minPitch = -44f;
    [SerializeField] protected float maxPitch = 44f;

    protected float mouseX;
    protected float mouseY;
    protected float xRotation;
    protected float yRotation;

    protected virtual void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected virtual void Update()
    {
        HandleRotation();
    }

    protected void OnLook(InputValue lookValue)
    {
        Vector2 lookVector = lookValue.Get<Vector2>();
        mouseX = lookVector.x * rotationSensitivity * Time.deltaTime;
        mouseY = lookVector.y * rotationSensitivity * Time.deltaTime;
    }


    protected virtual void HandleRotation()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        yRotation += mouseX;
    }
}
