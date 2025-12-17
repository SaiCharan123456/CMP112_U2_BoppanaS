using UnityEngine;

public class TPCameraController : CameraController
{
    [SerializeField] private Transform cameraTarget;

    protected override void HandleRotation()
    {
        base.HandleRotation();

        cameraTarget.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
