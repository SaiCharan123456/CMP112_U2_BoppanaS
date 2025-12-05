using UnityEngine;
using UnityEngine.InputSystem;


public class TPCameraController : MonoBehaviour
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform Player;
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

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        cameraTarget.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        //Player.rotation = Quaternion.Euler(0f, yRotation, 0f);
        //Debug.Log(Player.transform.rotation);
    }

    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle< -360f)
        {
            lfAngle += 360f;
        }
        if (lfAngle > 360f)
        {
            lfAngle -= 360f;
        }
        
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

        

    void OnLook(InputValue lookValue)
    {
        Vector2 lookVector = lookValue.Get<Vector2>();

        mouseX = lookVector.x * rotationSensitivity * Time.deltaTime;
        mouseY = lookVector.y * rotationSensitivity * Time.deltaTime;

        
    }
}
