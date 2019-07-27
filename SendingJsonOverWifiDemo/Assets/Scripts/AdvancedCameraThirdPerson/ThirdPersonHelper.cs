using UnityEngine;

public class ThirdPersonHelper : MonoBehaviour 
{
    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }
    
    // Clamp angle to between -360, 360.
    public static float ClampingAngle (float angle, float min, float max)
    {
        do {
                if (angle < -360)
                {
                    angle += 360;
                }
                if (angle > 360)
                {
                    angle -= 360;
                }
        } while (angle < -360 || angle > 360);

        return Mathf.Clamp (angle, min, max);
    }
    
    public static ClipPlanePoints ClipPlaneAtNear (Vector3 pos)
    {
        Camera mainCamera = Camera.main;

        var clipPlanePoints = new ClipPlanePoints ();
        
        // Ensure there is a main camera for us to check.
        if (!Camera.main)
            return clipPlanePoints;
        
        // Properties to determine the main camera's near clip plane.
        var mainCameraTransform = mainCamera.transform;
        var halfFieldOfView = (mainCamera.fieldOfView / 2) * Mathf.Deg2Rad;
        var aspect = mainCamera.aspect;
        var distance = mainCamera.nearClipPlane;
        var height = distance * Mathf.Tan (halfFieldOfView);
        var width = height * aspect;
        
        clipPlanePoints.LowerRight = pos + mainCameraTransform.right * width;
        clipPlanePoints.LowerRight -= mainCameraTransform.up * height;
        clipPlanePoints.LowerRight += mainCameraTransform.forward * distance;
        
        clipPlanePoints.LowerLeft = pos - mainCameraTransform.right * width;
        clipPlanePoints.LowerLeft -= mainCameraTransform.up * height;
        clipPlanePoints.LowerLeft += mainCameraTransform.forward * distance;
        
        clipPlanePoints.UpperRight = pos + mainCameraTransform.right * width;
        clipPlanePoints.UpperRight += mainCameraTransform.up * height;
        clipPlanePoints.UpperRight += mainCameraTransform.forward * distance;
        
        clipPlanePoints.UpperLeft = pos - mainCameraTransform.right * width;
        clipPlanePoints.UpperLeft += mainCameraTransform.up * height;
        clipPlanePoints.UpperLeft += mainCameraTransform.forward * distance;
        
        return clipPlanePoints;
    }
}
