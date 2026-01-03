using UnityEngine;

public class TPCameraController : CameraController
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraTarget1;

    protected override void HandleRotation()
    {
        base.HandleRotation();

        cameraTarget.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        cameraTarget1.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
