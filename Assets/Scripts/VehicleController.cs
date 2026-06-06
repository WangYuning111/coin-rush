using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraDistance = 8f;
    public float mouseSensitivity = 2f;
    public float maxPitchAngle = 80f;

    [Header("Vehicle Movement Settings")]
    public float moveSpeed = 10f;
    public float turnSmoothSpeed = 10f;

    private Rigidbody rb;
    private float yaw;
    private float pitch;
    private bool isAltPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("Main Camera not assigned, auto-finding main camera in scene");
        }

        if (mainCamera != null)
        {
            Vector3 startAngles = mainCamera.transform.eulerAngles;
            yaw = startAngles.y;
            pitch = startAngles.x;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCursorInput();
        if (!isAltPressed)
        {
            HandleCameraRotation();
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            UpdateCameraPosition();
        }
    }

    void FixedUpdate()
    {
        HandleVehicleMovement();
    }

    void HandleCursorInput()
    {
        if (Time.timeScale == 0f) return;

        isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (isAltPressed)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            yaw += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
            // Debug.Log($"Camera rotation: Yaw={yaw:F1}, Pitch={pitch:F1}");
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back * cameraDistance;
        Vector3 cameraPosition = transform.position + Vector3.up * 2f + direction;

        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.LookAt(transform.position); 
    }

    void HandleVehicleMovement()
    {
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 moveDelta = camForward * verticalInput * moveSpeed * Time.fixedDeltaTime;
            Vector3 targetPosition = rb.position + moveDelta;

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSmoothSpeed * Time.fixedDeltaTime);

            rb.MovePosition(targetPosition);
            rb.MoveRotation(newRotation);

            // Debug.Log($"Vehicle moving: Input={verticalInput}, Speed={rb.velocity.magnitude:F2}");
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.1f);
        }
    }
}