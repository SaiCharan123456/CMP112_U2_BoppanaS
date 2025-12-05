using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] CharacterController controller;
    private float movementX;
    private float movementY;
    [SerializeField] float speed = 5;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float turningSpeed = 5.0f;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        groundedPlayer = controller.isGrounded;

        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        movement = cameraTransform.transform.TransformDirection(movement);
        
        Turn();

        controller.Move(movement * speed * Time.deltaTime);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        

    }

    void Turn()
    {
        Vector3 currentLookDirection = controller.velocity.normalized;
        currentLookDirection.y = 0;

        currentLookDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(currentLookDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();

        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnJump(InputValue JumpValue)
    {
        if (JumpValue.isPressed && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);            
        }
    }

}
