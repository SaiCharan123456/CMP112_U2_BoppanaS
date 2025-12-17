using Unity.Cinemachine;
using UnityEngine;
using static UnityEditor.SceneView;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Real Cameras")]
    [SerializeField] private GameObject firstPersonCamera;
    [SerializeField] private GameObject thirdPersonCamera; // Has CinemachineBrain

    [Header("Cinemachine Virtual Cameras")]
    [SerializeField] private CinemachineCamera thirdPersonVcam;
    [SerializeField] private CinemachineCamera thirdPersonAimVcam;

    private CameraMode currentMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SwitchToThirdPerson();
    }


    public void SwitchToFirstPerson()
    {
        currentMode = CameraMode.FirstPerson;

        firstPersonCamera.SetActive(true);
        thirdPersonCamera.SetActive(false);

        thirdPersonVcam.gameObject.SetActive(false);
        thirdPersonAimVcam.gameObject.SetActive(false);
    }

    public void SwitchToThirdPerson()
    {
        currentMode = CameraMode.ThirdPerson;

        firstPersonCamera.SetActive(false);
        thirdPersonCamera.SetActive(true);

        thirdPersonVcam.gameObject.SetActive(true);
        thirdPersonAimVcam.gameObject.SetActive(false);
    }

    public void SwitchToThirdPersonAim()
    {
        currentMode = CameraMode.ThirdPersonAim;

        firstPersonCamera.SetActive(false);
        thirdPersonCamera.SetActive(true);

        thirdPersonVcam.gameObject.SetActive(false);
        thirdPersonAimVcam.gameObject.SetActive(true);
    }

    public CameraMode GetCurrentMode()
    {
        return currentMode;
    }
}
