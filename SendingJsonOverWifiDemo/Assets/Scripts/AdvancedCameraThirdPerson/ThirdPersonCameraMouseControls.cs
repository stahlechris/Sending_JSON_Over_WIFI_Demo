using UnityEngine;

[RequireComponent(typeof(ThirdPersonHelper))]
public class ThirdPersonCameraMouseControls : MonoBehaviour 
{
    #region Const String Values
    const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    const string MOUSE_X = "Mouse X";
    const string MOUSE_Y = "Mouse Y";
    #endregion

    private ThirdPerson_Camera _thirdPersonCamera;
    public float MouseXSensitivity = 5f;                    // Mouse X sensitivity.
    public float MouseYSensitivity = 5f;                    // Mouse Y sensitivity.
    public float MouseWheelSensitivity = 5f;                // Mouse wheel/scroll sensitivity.

    private void Start()
    {
        if (!_thirdPersonCamera)
        {
            _thirdPersonCamera = GetComponent<ThirdPerson_Camera>();
        }
    }

    private void LateUpdate()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        var deadZone = 0.01f;

        // If right mouse button is down, get mouse axis input for rotation.
        if (Input.GetMouseButton(1))
        {
            _thirdPersonCamera.MouseX += Input.GetAxis(MOUSE_X) * MouseXSensitivity;
            _thirdPersonCamera.MouseY -= Input.GetAxis(MOUSE_Y) * MouseYSensitivity;
        }

        // Clamp (limit) mouse Y rotation. Uses thirdPersonCameraHelper.cs.
        _thirdPersonCamera.MouseY = ThirdPersonHelper.ClampingAngle(_thirdPersonCamera.MouseY,
                                                                     _thirdPersonCamera.YMinLimit,
                                                                     _thirdPersonCamera.YMaxLimit
        );

        // Clamp (limit) mouse scroll wheel.
        if (Input.GetAxis(MOUSE_SCROLLWHEEL) > deadZone || Input.GetAxis(MOUSE_SCROLLWHEEL) < -deadZone)
        {
            _thirdPersonCamera.DesiredDistance = Mathf.Clamp(_thirdPersonCamera.Distance -
            Input.GetAxis(MOUSE_SCROLLWHEEL) * MouseWheelSensitivity,
                                                             _thirdPersonCamera.DistanceMin,
                                                             _thirdPersonCamera.DistanceMax
            );
            _thirdPersonCamera.PreOccludedDistance = _thirdPersonCamera.DesiredDistance;
            _thirdPersonCamera.DistanceCameraSmooth = _thirdPersonCamera.DistanceSmooth;
        }
    }
}
