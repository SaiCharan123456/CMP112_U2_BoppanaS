using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform FPSCameraTransform;
    [SerializeField] Transform TPSCameraTransform;
    [SerializeField] GameObject FPSCamera;
    [SerializeField] GameObject TPSCamera;
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject playerBody;
    private float movementX;
    private float movementY;
    [SerializeField] float speed = 5;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float turningSpeed = 5.0f;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool isMoving;

    [SerializeField] Animator animator;


    void Update()
    {
        groundedPlayer = controller.isGrounded;

        Vector3 movement = new Vector3(movementX, 0.0f, movementY);


        if (TPSCamera.activeSelf)
        {
            movement = TPSCameraTransform.transform.TransformDirection(movement);
        }
        if (FPSCamera.activeSelf)
        {
            movement = FPSCameraTransform.transform.TransformDirection(movement);
        }
            
        
           
        if (movementX != 0 || movementY != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        animator.SetBool("isRunning", isMoving);


        controller.Move(movement * speed * Time.deltaTime);
    
        if (TPSCamera.activeSelf)
        {
            Turn();
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        

    }

    void Turn()
    {
        Vector3 currentLookDirection = controller.velocity.normalized;

        currentLookDirection.y = 0;        

        Quaternion targetRotation = Quaternion.LookRotation(currentLookDirection);

        playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, targetRotation, Time.deltaTime * turningSpeed);
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
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);  

            animator.SetTrigger("Jump" );
        }
    }

}
