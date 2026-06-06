using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ufoController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [Tooltip("Movement speed (m/s)")]
    public float moveSpeed = 8f;

    [Tooltip("Turn smoothing speed")]
    public float turnSmoothTime = 0.1f;

    [Header("References")]
    [Tooltip("Please drag in the main camera from the scene")]
    public Transform mainCamera;

    private Rigidbody rb;
    private float turnSmoothVelocity;
    private Vector3 currentMoveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                Vector3 cameraRight = mainCamera.right;
                cameraRight.y = 0;
                cameraRight.Normalize();

                currentMoveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }
            else
            {
                currentMoveDirection = direction;
            }
        }
        else
        {
            currentMoveDirection = Vector3.zero;
        }
    }

    void ApplyMovement()
    {
        if (currentMoveDirection != Vector3.zero)
        {
            Vector3 targetPosition = rb.position + currentMoveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    void ApplyRotation()
    {
        if (mainCamera != null)
        {
            float targetRotation = mainCamera.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }
    }
}