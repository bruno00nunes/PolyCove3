using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 100f;          // Speed at which the camera moves using the Middle-Mouse Button
    public float panSpeed = 30f;            // Speed at which the camera moves using the Keyboard (WASD)
    public float panBorderThickness = 10f;  // At which distance from window edge does the game camera pan
    public float rotateSpeed = 20f;         // Speed at which the camera rotates
    public bool invertMousePan;             // Invert Mouse Panning. True = Brings camera with mouse; False = Drags camera away from mouse
    public bool useEdgePanning = true;      // Enable/Disable edgePanning
    public Vector2 panLimit;                // X and Y limits for the camera

    public Transform targetFollow;          // Target transform for camera to follow
    public Vector2 targetOffset;            // Offset from target
    public float followingSpeed = 5f;       // Speed when following a target

    public float zoomSpeed = 50f;           // Speed at which the camera moves
    public bool invertZoom;                 // True = camera goes down when scroll up; False = camera goes down when scroll up
    public float minCameraAltitude = 20f;   // Minimum altitude for camera
    public float maxCameraAltitude = 120f;  // Maximum altitude for camera
    
    private Transform _parentTransform;
    private const float MinCameraAngle = 20f;
    private const float MaxCameraAngle = 80f;
    private float _actualPanSpeed;

    [SerializeField] private LayerMask terrainLayer;

    private void Start()
    {
        _actualPanSpeed = panSpeed;
        var transform1 = transform;
        _parentTransform = transform1.parent;
    }

    private void LateUpdate()
    {
        Move();

        if (targetFollow)
            FollowTarget();

        CameraZoom();
    }

    // Update is called once per frame
    private void Move()
    {
        if (Input.GetKeyDown("left ctrl")) useEdgePanning = !useEdgePanning;
        if (Input.GetKeyDown("right ctrl")) invertZoom = !invertZoom;
        if (Input.GetKeyDown("left shift"))
        {
            zoomSpeed *= 2;
            panSpeed *= 2;
        }

        if (Input.GetKeyUp("left shift"))
        {
            zoomSpeed /= 2;
            panSpeed /= 2;
        }

        var movingForward = Input.GetKey("w") ||
                            Input.mousePosition.y >= Screen.height - panBorderThickness && useEdgePanning;
        var movingBack = Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness && useEdgePanning;
        var movingLeft = Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness && useEdgePanning;
        var movingRight = Input.GetKey("d") ||
                          Input.mousePosition.x >= Screen.width - panBorderThickness && useEdgePanning;

        if (Input.GetMouseButton(2) && !Input.GetKey("left shift"))
            MouseMove();
        else if (movingForward || movingBack || movingLeft || movingRight)
            KeyboardMove();
        RotateCamera();
    }

    private void MouseMove()
    {
        targetFollow = null;

        var pos = _parentTransform.position;

        pos -= _parentTransform.forward *
               (Input.GetAxis("Mouse Y") * dragSpeed * Time.deltaTime * (invertMousePan ? -1f : 1f));
        pos -= _parentTransform.right *
               (Input.GetAxis("Mouse X") * dragSpeed * Time.deltaTime * (invertMousePan ? -1f : 1f));

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);
        _parentTransform.position = pos;
    }

    private void KeyboardMove()
    {
        targetFollow = null;
        var pos = _parentTransform.position;

        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness && useEdgePanning)
            pos += _parentTransform.forward * (Time.deltaTime * _actualPanSpeed);
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness && useEdgePanning)
            pos -= _parentTransform.forward * (Time.deltaTime * _actualPanSpeed);
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness && useEdgePanning)
            pos -= _parentTransform.right * (Time.deltaTime * _actualPanSpeed);
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness && useEdgePanning)
            pos += _parentTransform.right * (Time.deltaTime * _actualPanSpeed);

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        _parentTransform.position = pos;
    }

    private void CameraZoom()
    {
        var pos = _parentTransform.position;
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
            scroll = 1f;
        else if (scroll < 0)
            scroll = -1f;

        pos.y += scroll * zoomSpeed * Time.deltaTime * (invertZoom ? 1f : -1f);

        Physics.Raycast(transform.position, Vector3.down, out var hit, Mathf.Infinity, terrainLayer);

        _actualPanSpeed = panSpeed;

        if (hit.distance < 5 && Math.Abs(hit.distance) > 0)
            _actualPanSpeed = panSpeed / 3f;
        else if (hit.distance < 10 && Math.Abs(hit.distance) > 0)
            _actualPanSpeed = panSpeed / 2.5f;
        else if (hit.distance < 15 && Math.Abs(hit.distance) > 0)
            _actualPanSpeed = panSpeed / 2f;
        else if (hit.distance < 20 && Math.Abs(hit.distance) > 0) _actualPanSpeed = panSpeed / 1.5f;

        if (hit.distance > 0 && hit.distance < minCameraAltitude)
            // pos.y += pos.y - hit.distance;
            pos.y += minCameraAltitude - hit.distance;

        pos.y = Mathf.Clamp(pos.y, hit.point.y + minCameraAltitude, maxCameraAltitude);

        var position = transform.position;
        var forward = _parentTransform.forward;
        var right = _parentTransform.right;

        if (Physics.Raycast(position, forward, out var frontRayHit, 1f, LayerMask.GetMask("Terrain")))
            pos.y += frontRayHit.collider.transform.localScale.y;
        if (Physics.Raycast(position, -forward, out var backRayHit, 1f, LayerMask.GetMask("Terrain")))
            pos.y += backRayHit.collider.transform.localScale.y;
        if (Physics.Raycast(position, right, out var rightRayHit, 1f, LayerMask.GetMask("Terrain")))
            pos.y += rightRayHit.collider.transform.localScale.y;
        if (Physics.Raycast(position, -right, out var leftRayHit, 1f, LayerMask.GetMask("Terrain")))
            pos.y += leftRayHit.collider.transform.localScale.y;

        _parentTransform.position = pos;
    }

    private void RotateCamera()
    {
        if (Input.GetKey(KeyCode.E))
        {
            _parentTransform.Rotate(0, Time.deltaTime * rotateSpeed, 0, Space.World);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            _parentTransform.Rotate(0, Time.deltaTime * -rotateSpeed, 0, Space.World);
        }
        else if (Input.GetMouseButton(2) && Input.GetKey("left shift"))
        {
            var transform1 = transform;

            _parentTransform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed, 0, Space.World);
            transform1.Rotate(Input.GetAxis("Mouse Y") * Time.deltaTime * 150f, 0, 0, Space.Self);

            var currentRotation = transform1.localRotation.eulerAngles;

            minCameraAltitude = currentRotation.x / 4;

            currentRotation.x = Mathf.Clamp(currentRotation.x, MinCameraAngle, MaxCameraAngle);
            currentRotation.y = 0;
            currentRotation.z = 0;
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }

    private void FollowTarget()
    {
        var position = _parentTransform.position;
        var position1 = targetFollow.position;
        var targetPos = new Vector3(position1.x + targetOffset.x, position.y, position1.z + targetOffset.y);
        position = Vector3.MoveTowards(position, targetPos, Time.deltaTime * followingSpeed);
        _parentTransform.position = position;
    }

    public void Follow(Transform transformToFollow)
    {
        targetFollow = transformToFollow;
    }
}