using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Parameters")]
    public float normalDistance = 5.0f;
    public float normalHeightOffset = 1.5f;

    [Header("General Parameters")]
    public float minDistance = 1.0f;
    public float maxDistance = 10.0f;
    public float rotateSpeed = 250.0f;
    public float zoomSpeed = 10.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Player Controller Reference")]
    public PlayerController playerController;

    [Header("Running Camera Auto-Reset")]
    [Tooltip("Target distance camera auto-zooms to when entering running/sprinting state")]
    public float runTargetDistance = 3.5f;
    [Tooltip("Speed of camera auto-zoom")]
    public float runResetSpeed = 2.0f;

    private float x = 0.0f;
    private float y = 0.0f;
    private bool isCursorVisible = true;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        UpdateCursorState();
    }

    void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        if (Time.timeScale == 0f) return;

        isCursorVisible = Input.GetKey(KeyCode.LeftAlt);
        UpdateCursorState();

        if ((playerController.IsRunning() || playerController.IsSprinting()) && playerController.HasMovementInput())
        {
            normalDistance = Mathf.Lerp(normalDistance, runTargetDistance, Time.deltaTime * runResetSpeed);
        }

        if (!isCursorVisible)
        {
            x += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            normalDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            normalDistance = Mathf.Clamp(normalDistance, minDistance, maxDistance);
        }

        Vector3 lookAtPoint = target.position + (target.up * normalHeightOffset);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = lookAtPoint - (rotation * Vector3.forward * normalDistance);

        transform.rotation = rotation;
        transform.position = position;
    }

    private void UpdateCursorState()
    {
        if (isCursorVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
