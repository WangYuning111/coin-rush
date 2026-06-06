using UnityEngine;

public class ufoCameraController : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform ufoTarget;

    [Header("Camera Parameters")]
    public float distanceFromTarget = 8f;
    public float mouseSensitivity = 2f;
    public float maxViewAngle = 80f;
    
    private float currentXAngle;
    private float currentYAngle;

    void Start()
    {
        currentXAngle = 45f;
        currentYAngle = ufoTarget.eulerAngles.y;

        SetCursorState(true);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            SetCursorState(false);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            SetCursorState(true);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseRotation();
        }
    }

    void LateUpdate()
    {
        ApplyCameraPosition();
    }

    void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYAngle += mouseX;
        currentXAngle -= mouseY;

        currentXAngle = Mathf.Clamp(currentXAngle, 5f, maxViewAngle);
    }

    void ApplyCameraPosition()
    {
        if (ufoTarget == null) return;

        Quaternion rotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
        Vector3 direction = new Vector3(0, 0, -distanceFromTarget);
        Vector3 position = ufoTarget.position + rotation * direction;

        transform.rotation = rotation;
        transform.position = position;
    }

    void SetCursorState(bool isLocked)
    {
        Cursor.visible = !isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}