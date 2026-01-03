using UnityEngine;

public class FPCameraController : CameraController
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerBody;

    protected override void HandleRotation()
    {
        base.HandleRotation();

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        player.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
