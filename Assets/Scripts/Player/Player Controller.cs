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

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip footstepsClip1; // First footstep clip
    [SerializeField] AudioClip footstepsClip2; // Second footstep clip
    private float footstepTimer = 0.0f; // Timer to alternate between footsteps
    private float footstepInterval = 0.5f; // Time interval to switch between footstep sounds

    private Vector3 knockbackVelocity;
    private float knockbackDecay = 10f;


    void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (knockbackVelocity.magnitude > 0.1f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        Vector3 movement = new Vector3(movementX, 0.0f, movementY);


        if (TPSCamera.activeSelf)
        {
            movement = TPSCameraTransform.transform.TransformDirection(movement);
        }

        if (FPSCamera != null)
        {
            if (FPSCamera.activeSelf)
            {
                movement = FPSCameraTransform.transform.TransformDirection(movement);
            }
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

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                // Alternate between the two footstep sounds using a normal if-else block
                if (audioSource.clip == footstepsClip1)
                {
                    audioSource.clip = footstepsClip2;
                }
                else
                {
                    audioSource.clip = footstepsClip1;
                }

                Enemy.HearSound(transform.position); // Notify enemies of footstep sound

                audioSource.Play();
                footstepTimer = 0.0f; // Reset the timer
            }
        }
        else
        {
            audioSource.Stop(); // Stop footstep sound if not moving
            footstepTimer = 0.0f; // Reset the timer when not moving
        }



        controller.Move(movement * speed * Time.deltaTime);
    
        if (TPSCamera.activeSelf)
        {
            Turn();
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        

    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0;
        knockbackVelocity = direction.normalized * force;
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
