using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraHandler : MonoBehaviour
{
    // Used for Camera controls
    private readonly List<Camera> _sceneCameras = new();
    private CameraInput _cameraInput;
    private PhysicsRaycaster _raycaster;
    private AudioListener _audioListener;
    private Player _player;

    // Horizontal Motion
    [SerializeField] [Range(5f, 50f)] private float _panSpeedMax = 10f;
    [SerializeField] private float _panAcceleration = 10f;
    [SerializeField] private float _panZoomHeightAdjust = 1.0f;
    private float _panSpeed;
    private Vector3 _panNextPosition;
    private Vector3 _panLastPosition;
    private Vector3 _panSnapPosition;
    private Vector3 _panVelocity;
    private Unit _panFollowTarget;

    // Vertical Motion
    [SerializeField] [Range(0.5f, 3f)] private float _zoomStepSize = 2f;
    [SerializeField] [Range(0.5f, 2f)] private float _zoomZoomSpeed = 2f;
    [SerializeField] private float _zoomDampening = 7.5f;
    [SerializeField] private float _zoomHeightMin = 5f;
    [SerializeField] private float _zoomHeightMax = 50f;
    private float _zoomHeight;

    // Rotation
    [SerializeField] [Range(0.1f, 1f)] private float _rotationSpeedMax = 0.25f;

    // screen edge motion -- NOT IMPLEMENTED
    // [SerializeField] [Range(0f, 0.1f)] float edgeTolerance = 0.05f; // TO DO -- For use with camera boundaries
    // [SerializeField] bool useScreenEdge = true; // TO DO -- For use with camera boundaries

    public static Camera ActiveCamera { get { return Camera.main.GetComponent<CameraHandler>().GetActiveCamera(); } }
    public Tuple<float, float> Zoom { get { return new Tuple<float, float>(_zoomHeight, _zoomDampening); } }
    private Vector3 PlayerPosition { get { return _player.transform.position; } set { _player.transform.position = value; } }
    private Quaternion PlayerRotation { get { return _player.transform.rotation; } set { _player.transform.rotation = value; } }
    private Vector3 LocalPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
    private bool InputPan { get { return _cameraInput.Controls.InputPan.IsPressed(); } }
    private Vector2 InputMovement { get { return _cameraInput.Controls.Movement.ReadValue<Vector2>(); } }
    private Vector2 InputRotation { get { return _cameraInput.Controls.Rotation.ReadValue<Vector2>(); } }
    private Vector2 MousePosition { get { return _cameraInput.Controls.Position.ReadValue<Vector2>(); } }

    private void Awake()
    {
        _cameraInput = new CameraInput();
        _raycaster = GetComponent<PhysicsRaycaster>();
        _audioListener = GetComponent<AudioListener>();
        _zoomHeight = transform.position.y;
        _player = GetComponentInParent<Player>();
    }

    private void OnEnable()
    {
        // Add bindings when enabled and begin tracking position

        _panLastPosition = transform.position;
        _cameraInput.Controls.ZoomCamera.performed += ZoomCamera;
        _cameraInput.Enable();
    }

    private void OnDisable()
    {
        // Remove bindings when disabled

        _cameraInput.Controls.ZoomCamera.performed -= ZoomCamera;
        _cameraInput.Disable();
    }

    private void Update()
    {
        UpdateKeyboardMovement();
        UpdateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
        UpdateCameraRotation();
        UpdateCameraFollow();
        UpdateCameraSnap();
        CheckActiveCamera();
    }

    public void AddSceneCamera(Camera addCamera)
    {
        // Adds camera from the current scene to be tracked

        if (!_sceneCameras.Contains(addCamera))
            _sceneCameras.Add(addCamera);
    }

    private void CheckActiveCamera()
    {
        // Ensures physics _raycaster is only active if no other camera is in use

        // If there are any scene cameras currently active, disable _raycaster on this camera
        if (_sceneCameras.Any() && _sceneCameras.Any(checkCamera => checkCamera.enabled))
        {
            _raycaster.enabled = false;
            _audioListener.enabled = false;
            return;
        }

        // If this is the only camera active, ensure _raycaster and audio listener are turned back on
        if (!_raycaster.enabled || !_audioListener.enabled)
        {
            _raycaster.enabled = true;
            _audioListener.enabled = true;
        }
    }

    private Camera GetActiveCamera()
    {
        // Returns currently active camera

        foreach (Camera camera in _sceneCameras)
            if (camera.enabled)
                return camera;

        return Camera.main;
    }

    private void UpdateVelocity()
    {
        // Tracks velocity of camera _inputMovement

        _panVelocity = (PlayerPosition - _panLastPosition) / Time.deltaTime;
        _panVelocity.y = 0;
        _panLastPosition = PlayerPosition;
    }

    private void UpdateKeyboardMovement()
    {
        // Translates keyboard input or mouse position into camera position values

        Vector2 readValue = InputPan ? Camera.main.ScreenToViewportPoint(MousePosition) - new Vector3(0.5f, 0.5f, 0f) : InputMovement;

        Vector3 inputValue = (readValue.x * GetCameraRight() + readValue.y * GetCameraForward()).normalized;

        //if (inputValue.sqrMagnitude > 0.1f && IsPositionInViewFrustum(PlayerPosition + LocalPosition, _panNextPosition + inputValue))

        if (inputValue.sqrMagnitude > 0.1f && IsPositionInViewFrustum(PlayerPosition + LocalPosition, _panNextPosition + inputValue))
        {
            _panNextPosition += inputValue;
            _panSnapPosition = Vector3.zero;
        }
    }

    private Vector3 GetCameraRight()
    {
        // Returns camera right with a flattened y axis

        Vector3 right = transform.right;
        right.y = 0;
        return right;
    }

    private Vector3 GetCameraForward()
    {
        // Returns camera forward with a flattened y axis

        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward;
    }

    private void UpdateBasePosition()
    {
        // Moves camera parent (Player) to new position

        if (_panNextPosition.sqrMagnitude > 0.1f)
        {
            float finalPanSpeed = _panSpeedMax + (_zoomHeight - _zoomHeightMin) * _panZoomHeightAdjust;
            _panSpeed = Mathf.Lerp(_panSpeed, finalPanSpeed, Time.deltaTime * _panAcceleration);
            PlayerPosition += _panSpeed * Time.deltaTime * _panNextPosition;
        }

        _panNextPosition = Vector3.zero;
    }

    private void UpdateCameraRotation()
    {
        // Rotate camera parent (Player) while holding middle mouse button or pressing Q/E

        float value = InputRotation.x;
        PlayerRotation = Quaternion.Euler(PlayerRotation.eulerAngles.x, value * _rotationSpeedMax + PlayerRotation.eulerAngles.y, PlayerRotation.eulerAngles.z);
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        // Sets the expected zoom height based on input

        float value = -inputValue.ReadValue<Vector2>().y / 100f; // divided for tuning

        if (Mathf.Abs(value) > 0.1f)
        {
            _zoomHeight = LocalPosition.y + value * _zoomStepSize;
            if (_zoomHeight < _zoomHeightMin)
                _zoomHeight = _zoomHeightMin;
            else if (_zoomHeight > _zoomHeightMax)
                _zoomHeight = _zoomHeightMax;
        }
    }

    private void UpdateCameraPosition()
    {
        // Pulls camera backwards, giving a "zooming out" feel

        Vector3 zoomTarget = new(LocalPosition.x, _zoomHeight, LocalPosition.z);
        Vector3 moveVector = new(-0.5f, 0f, 0.5f); // Keep x and z inverted to center camera
        zoomTarget -= _zoomZoomSpeed * (_zoomHeight - LocalPosition.y) * moveVector;

        LocalPosition = Vector3.Lerp(LocalPosition, zoomTarget, Time.deltaTime * _zoomDampening);
    }

    public void SetCameraSnap(Unit unit)
    {
        if (!unit)
            return;

        _panSnapPosition = unit.transform.position;
    }

    public void SetCameraFollow(Unit unit)
    {
        _panFollowTarget = unit;
    }

    public void SetCameraSnap(Vector3 position)
    {
        if (position == Vector3.zero)
            return;

        _panSnapPosition = position;
    }

    private void UpdateCameraSnap()
    {
        // If no snap target is set, exit
        if (_panSnapPosition == Vector3.zero)
            return;

        // Set new target position and pan speed
        Vector3 newPosition = new(_panSnapPosition.x, PlayerPosition.y, _panSnapPosition.z);
        _panSpeed = Mathf.Lerp(_panSpeed, _panSpeedMax, Time.deltaTime * _panAcceleration);

        // Move the camera focus target
        PlayerPosition = Vector3.Lerp(PlayerPosition, newPosition, 0.01f * _panSpeed);

        // Once position is reached, clear snap target
        if (Vector3.Distance(PlayerPosition, newPosition) <= 0.25f) 
            _panSnapPosition = Vector3.zero;
    }

    private void UpdateCameraFollow()
    {
        if (!_panFollowTarget)
            return;

        Vector3 targetPosition = _panFollowTarget.transform.position;

        // If distance to target is small enough, no need to update
        if (Vector3.Distance(PlayerPosition, targetPosition) <= 0.25f)
            return;

        // Set new target position and pan speed
        Vector3 newPosition = new(targetPosition.x, PlayerPosition.y, targetPosition.z);
        _panSpeed = Mathf.Lerp(_panSpeed, _panSpeedMax, Time.deltaTime * _panAcceleration);

        // Move the camera focus target
        PlayerPosition = Vector3.Lerp(PlayerPosition, newPosition, 0.01f * _panSpeed);
    }

    private bool IsPositionInViewFrustum(Vector3 originalPosition, Vector3 delta)
    {
        if (Map.MapGrid != null)
        {
            // Convert screen-space coordinates [0, 0], [1, 0], [0, 1], [1, 1] to world space
            // Note: The direction vectors don't change when we are trying to pan the camera, but this won't be true for arbitrary positions
            // Also, cameras look down the -Z direction, so need to flip signs

            /*
            Vector3 topLeft = ActiveCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, ActiveCamera.nearClipPlane)) - originalPosition;
            Vector3 topRight = ActiveCamera.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, ActiveCamera.nearClipPlane)) - originalPosition;
            Vector3 bottomLeft = ActiveCamera.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, ActiveCamera.nearClipPlane)) - originalPosition;
            Vector3 bottomRight = ActiveCamera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, ActiveCamera.nearClipPlane)) - originalPosition;
            List<Vector3> cameraDirVectors = new List<Vector3> { topLeft, topRight, bottomLeft, bottomRight };
            */

            /*
            List<Vector3> cameraDirVectors = new List<Vector3> { topLeft, topRight, bottomLeft, bottomRight };
            

            // Intersect camera rays with the grid
            Vector3 gridNormal;
            Vector3 gridPoint;
            Map.MapGrid.GetPlaneEquation(out gridPoint, out gridNormal);


            // Check if any of the intersection points lie within the grid
            List<Vector3> gridBounds = Map.MapGrid.GetGridBounds();

            
             for (int i = 0; i < cameraDirVectors.Count; i++)
             {
                 Vector3 cameraVector = cameraDirVectors[i];
                 Vector3 planeIntersection;
                 bool bRayHit = VectorUtils.RayPlaneIntersection(out planeIntersection, originalPosition + delta, cameraVector, gridPoint, gridNormal);

                 // Grid is probably clipping the near plean if bRayHit is false
                 if (!bRayHit) return false;

                 if (Map.MapGrid.ContainsPoint(planeIntersection))
                 {
                     return true;
                 }
             }
            */

            //if (!bRayHit) return false;
            //else if (Map.MapGrid.ContainsPoint(planeIntersection)) return true;


            // By BVAS89
            // Checks point at the middle top third of screen.
            Ray mRay = ActiveCamera.ViewportPointToRay(new Vector3(0.5f, 0.66f, ActiveCamera.nearClipPlane));
           // Debug.DrawRay(mRay.origin, mRay.direction * 1000, Color.cyan);
            RaycastHit[] mRayHits;
            mRayHits = Physics.RaycastAll(mRay);

            for (int i = 0; i < mRayHits.Length; i++)
            {
                if (Map.MapGrid.ContainsPoint(mRayHits[i].point))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
