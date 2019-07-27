using UnityEngine;

/*This script Proceeds to freak TF out when entering water.
 * TODO: Fix this freaking the fuck out behavior.
 */
public class ThirdPerson_Camera : MonoBehaviour
{
    public ThirdPerson_Camera Instance;                     // Reference to the Main Camera.
    public Transform TargetLookTransform;                   // Transform of the camera will be looking at.
    public float Distance = 1.5f;                             // Start distance of camera.
    public float DistanceMin = 1f;                          // Minimum distance of camera zoom.
    public float DistanceMax = 3f;                          // Maximum distance of camera zoom.
    public float DistanceSmooth = 0.05f;                    // Camera zooming smooth factor.
    public float DistanceCameraResumeSmooth = 1f;           // Distance at which point smoothing is resumed after occlusion handling is no longer occuring.
    public float XSmooth = 0.05f;                           // Smoothness factor for x position calculations.
    public float YSmooth = 0.1f;                            // Smoothness factor for y position calculations.
    public float YMinLimit = -40f;
    public float YMaxLimit = 80f;
    public float OcclusionDistanceStep = 0.5f;
    public int MaxOcclusionChecks = 10;                     // Max number of times to check for occlusion.

    [System.NonSerialized]
    public float MouseX;
    [System.NonSerialized]
    public float MouseY;
    [System.NonSerialized]
    public float DesiredDistance;
    [System.NonSerialized]
    public float DistanceCameraSmooth;              // Camera smoothing distance (after occlusion is no longer happening).
    [System.NonSerialized]
    public float PreOccludedDistance;

    public Transform playersTransform;
    public bool DoUpdate { get; set; }
    private Transform myTransform;
    private Camera myCamera;
   
    private float _velocityX;
    private float _velocityY;
    private float _velocityZ;
    private float _velocityDistance;
    private float _startDistance;
    private Vector3 _position = new Vector3(768f, 3.5f, 903f);
    private Vector3 _desiredPosition = new Vector3(768f, 3.5f, 903f);

    private void Start()
    {
        DoUpdate = true;

        if (Instance == null)
        {
            Instance = this;
        }

        // If main camera is null, set as main camera
        if (Camera.main == null)
        {
            tag = "MainCamera";
        }

        //cache Transform
        myTransform = transform;
        //cache Camera
        myCamera = Camera.main;

        // Ensure our distance is between min and max (valid)
        Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);
        _startDistance = Distance;
        Reset();
    }

    private void LateUpdate()
    {
        if (DoUpdate)
        {
            if (TargetLookTransform == null)
            {
                return;
            }

            var count = 0;
            do
            {
                CalculateDesiredPosition();
                count++;
            } while (CheckIfOccluded(count));

            UpdatePosition();
        }
    }

    // Smoothing.
    private void CalculateDesiredPosition()
    {
        // Evaluate distance.
        ResetDesiredDistance();
        Distance = Mathf.SmoothDamp(Distance, DesiredDistance, ref _velocityDistance, DistanceCameraSmooth);

        // Calculate desired position.
        _desiredPosition = CalculatePosition(MouseY, MouseX, Distance);
    }

    private bool CheckIfOccluded(int count)
    {
        var isOccluded = false;
        var nearestDistance = CheckCameraPoints(TargetLookTransform.position, _desiredPosition);

        if(!Mathf.Approximately(nearestDistance,-1))
        //if (nearestDistance != -1)
        {
            if (count < MaxOcclusionChecks)
            {
                isOccluded = true;
                Distance -= OcclusionDistanceStep;

                // 0.25 is a good default value.
                if (Distance < 0.25f)
                {
                    Distance = 0.25f;
                }
            }
            else
            {
                Distance = nearestDistance - myCamera.nearClipPlane; //changed getComponent<Camera>() to myCamera 
            }
            DesiredDistance = Distance;
            DistanceCameraSmooth = DistanceCameraResumeSmooth;
        }

        return isOccluded;
    }

    private Vector3 CalculatePosition(float rotX, float rotY, float rotDist)
    {
        var direction = new Vector3(0, 0, -rotDist);                      // -distance because we want it to point behind our character.
        var rotation = Quaternion.Euler(rotX, rotY, 0);

        return TargetLookTransform.position + (rotation * direction);
    }

    private float CheckCameraPoints(Vector3 from, Vector3 to)
    {
        var nearestDistance = -1f;

        RaycastHit hitInfo;

        var clipPlanePoints = ThirdPersonHelper.ClipPlaneAtNear(to);

        /*
        // Draw the raycasts going through the near clip plane vertexes.
        Debug.DrawLine(from, to + myTransform.forward * -myCamera.nearClipPlane, Color.red);
        Debug.DrawLine(from, clipPlanePoints.UpperLeft, Color.red);
        Debug.DrawLine(from, clipPlanePoints.UpperRight, Color.red);
        Debug.DrawLine(from, clipPlanePoints.LowerLeft, Color.red);
        Debug.DrawLine(from, clipPlanePoints.LowerRight, Color.red);
        Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight, Color.red);
        Debug.DrawLine(clipPlanePoints.UpperRight, clipPlanePoints.LowerRight, Color.red);
        Debug.DrawLine(clipPlanePoints.LowerRight, clipPlanePoints.LowerLeft, Color.red);
        Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.UpperLeft, Color.red);
        */

        if (Physics.Linecast(from, clipPlanePoints.UpperLeft, out hitInfo)) //&& !hitInfo.collider.CompareTag(PLAYER_TAG))
            nearestDistance = hitInfo.distance;
        if (Physics.Linecast(from, clipPlanePoints.LowerLeft, out hitInfo)) //&& !hitInfo.collider.CompareTag(PLAYER_TAG))
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;
        if (Physics.Linecast(from, clipPlanePoints.UpperRight, out hitInfo)) //&& !hitInfo.collider.CompareTag(PLAYER_TAG))
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;
        if (Physics.Linecast(from, to + myTransform.forward * -myCamera.nearClipPlane, out hitInfo)) //&& !hitInfo.collider.CompareTag(PLAYER_TAG))
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;

        return nearestDistance;
    }

    private void ResetDesiredDistance()
    {
        if (DesiredDistance < PreOccludedDistance)
        {
            var pos = CalculatePosition(MouseY, MouseX, PreOccludedDistance);
            var nearestDistance = CheckCameraPoints(TargetLookTransform.position, pos);

            if (CheckBehindCam(_desiredPosition) && (nearestDistance == -1 || nearestDistance > PreOccludedDistance))
            {
                DesiredDistance = PreOccludedDistance;
            }
        }
    }

    private bool CheckBehindCam(Vector3 to) // Checks the area behind the camera to make sure the camera can back up to its desired spot
    {
        RaycastHit hitInfo;

        var pos = CalculatePosition(MouseY, MouseX, PreOccludedDistance);

        var clipPlanePoints = ThirdPersonHelper.ClipPlaneAtNear(to);

        /*
        //These lines are drawn when it starts to fuck up and make the camera snap into player's head.
        Debug.DrawLine(myTransform.position, pos, Color.blue);
        Debug.DrawLine(clipPlanePoints.UpperLeft, pos, Color.blue);
        Debug.DrawLine(clipPlanePoints.UpperRight, pos, Color.blue);
        Debug.DrawLine(clipPlanePoints.LowerLeft, pos, Color.blue);
        Debug.DrawLine(clipPlanePoints.LowerRight, pos, Color.blue);
        */
        if (Physics.Linecast(clipPlanePoints.UpperLeft, pos, out hitInfo))
        {
            return false;
        }
        if (Physics.Linecast(clipPlanePoints.UpperLeft, pos, out hitInfo))
        {
            return false;
        }
        if (Physics.Linecast(clipPlanePoints.UpperRight, pos, out hitInfo))
        {
            return false;
        }
        if (Physics.Linecast(clipPlanePoints.LowerLeft, pos, out hitInfo))
        {
            return false;
        }
        if (Physics.Linecast(clipPlanePoints.LowerRight, pos, out hitInfo))
        {
            return false;
        }
        return true;
    }

    private void UpdatePosition()
    {
        var posX = Mathf.SmoothDamp(_position.x, _desiredPosition.x, ref _velocityX, XSmooth * Time.deltaTime);
        var posY = Mathf.SmoothDamp(_position.y, _desiredPosition.y, ref _velocityY, YSmooth * Time.deltaTime);
        var posZ = Mathf.SmoothDamp(_position.z, _desiredPosition.z, ref _velocityZ, XSmooth * Time.deltaTime);

        _position = new Vector3(posX, posY, posZ);



        myTransform.position = _position;
        // make sure our camera doesn't snap into our head. (go below our DistanceMin set threshold).
        Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);

        myTransform.LookAt(TargetLookTransform.position);
    }

    public void Reset()
    {
        MouseX = 0f;
        MouseY = 10f;
        Distance = _startDistance;
        DesiredDistance = Distance;
        PreOccludedDistance = Distance;
    }
}
