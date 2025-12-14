using UnityEngine;
using UnityEngine.InputSystem;


public class FPCameraController : MonoBehaviour
{
    [SerializeField] Transform Player;
    public Transform playerBody;
    private float mouseX;
    private float mouseY;
    private float yRotation;
    private float xRotation;
    [SerializeField] private float rotationSensitivity = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -44f, 44f);

        yRotation += mouseX;

        //yRotation = Mathf.Clamp(yRotation, -44, 44);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        Player.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void OnLook(InputValue lookValue)
    {
        Vector2 lookVector = lookValue.Get<Vector2>();

        mouseX = lookVector.x * rotationSensitivity * Time.deltaTime;
        mouseY = lookVector.y * rotationSensitivity * Time.deltaTime;

        
    }
}
