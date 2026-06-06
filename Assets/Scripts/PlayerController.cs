using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public enum IKMode
    {
        Off,
        WalkAndIdleOnly,
        Global
    }

    private CharacterController controller;
    private Animator animator;
    private Transform cameraTransform;

    [Header("Basic Movement")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float sprintSpeed = 10.0f;
    public float rotationSpeed = 15.0f;
    [Tooltip("Animation playback speed multiplier during sprint")]
    public float sprintAnimationSpeedMultiplier = 1.2f;
    [Header("Jump Settings")]
    public float jumpHeight = 1.2f;
    public float gravity = -30.0f;
    public float fallMultiplier = 2.5f;
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    [Header("Landing Settings")]
    [Tooltip("Minimum air time required to trigger landing animation (seconds)")]
    public float landAnimationThreshold = 0.3f;
    private float airborneTime = 0f;
    private bool wasGrounded;

    [Header("IK Settings")]
    public IKMode currentIKMode = IKMode.Global; 
    [Tooltip("Foot offset from ground (prevents foot sinking)")]
    [Range(0, 0.2f)]
    public float footOffset = 0.05f;
    [Tooltip("Raycast distance downward from foot")]
    public float raycastDistance = 0.5f;
    [Tooltip("Smoothness of IK weight changes")]
    public float ikSmoothness = 10.0f;

    [Header("IK UI")]
    public Text ikTextOff;
    public Text ikTextWalkAndIdle;
    public Text ikTextGlobal;

    [Header("Debug UI Settings")]
    public Text velocityDisplayText;

    private float leftFootIKWeight = 0;
    private float rightFootIKWeight = 0;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool isRunToggled = false;
    private bool isSprinting = false;
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool hasMovementInput; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        if (groundCheck == null) Debug.LogError("...");
        wasGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        UpdateIKUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            currentIKMode = (IKMode)(((int)currentIKMode + 1) % 3);
            UpdateIKUI();
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            playerVelocity.x = 0;
            playerVelocity.z = 0;
            animator.SetFloat("MoveInput", 0);
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (!wasGrounded && isGrounded)
        {
            if (airborneTime > landAnimationThreshold)
            {
                animator.SetTrigger("Land");
            }
        }
        if (!isGrounded)
        {
            airborneTime += Time.deltaTime;
        }
        else
        {
            airborneTime = 0f;
        }
        wasGrounded = isGrounded;
        animator.SetBool("IsGrounded", isGrounded);
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        hasMovementInput = input.magnitude >= 0.1f;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && hasMovementInput;
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSprinting)
        {
            isRunToggled = !isRunToggled;
        }
        if (isSprinting)
        {
            animator.speed = sprintAnimationSpeedMultiplier;
        }
        else
        {
            animator.speed = 1.0f;
        }
        moveDirection = Vector3.zero;
        if (hasMovementInput)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            moveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isRunToggled)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        if (isGrounded)
        {
            if (hasMovementInput)
            {
                playerVelocity.x = moveDirection.x * currentSpeed;
                playerVelocity.z = moveDirection.z * currentSpeed;
            }
            else
            {
                playerVelocity.x = 0;
                playerVelocity.z = 0;
            }
        }
        animator.SetFloat("MoveInput", input.magnitude, 0.1f, Time.deltaTime);
        animator.SetBool("IsRunning", isRunToggled);
        animator.SetBool("IsSprinting", isSprinting);
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        UpdateVelocityUI();
    }

    private void OnAnimatorMove()
    {
        Vector3 rootMotionDelta = animator.deltaPosition;
        Vector3 scriptedMoveDelta = new Vector3(playerVelocity.x, 0, playerVelocity.z) * Time.deltaTime;
        Vector3 finalMove = scriptedMoveDelta + rootMotionDelta;
        controller.Move(finalMove);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        if (playerVelocity.y < 0 && !isGrounded)
        {
            playerVelocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public bool IsGrounded() { return isGrounded; }
    public bool IsRunning() { return isRunToggled; }
    public bool IsSprinting() { return isSprinting; }

    public bool HasMovementInput()
    {
        return hasMovementInput;
    }

    private void UpdateIKUI()
    {
        if (ikTextOff == null || ikTextWalkAndIdle == null || ikTextGlobal == null) return;

        switch (currentIKMode)
        {
            case IKMode.Off:
                ikTextOff.enabled = true;
                ikTextWalkAndIdle.enabled = false;
                ikTextGlobal.enabled = false;
                break;
            case IKMode.WalkAndIdleOnly:
                ikTextOff.enabled = false;
                ikTextWalkAndIdle.enabled = true;
                ikTextGlobal.enabled = false;
                break;
            case IKMode.Global:
                ikTextOff.enabled = false;
                ikTextWalkAndIdle.enabled = false;
                ikTextGlobal.enabled = true;
                break;
        }
    }

    private void UpdateVelocityUI()
    {
        if (velocityDisplayText == null) return;

        float horizontalSpeed = new Vector2(playerVelocity.x, playerVelocity.z).magnitude;
        float verticalSpeed = playerVelocity.y;
        velocityDisplayText.text = $"Horizontal: {horizontalSpeed:F2} m/s\nVertical: {verticalSpeed:F2} m/s";
    }

    void OnAnimatorIK(int layerIndex)
    {
        bool shouldIKBeActive = false;

        switch (currentIKMode)
        {
            case IKMode.Off:
                shouldIKBeActive = false;
                break;

            case IKMode.Global:
                shouldIKBeActive = isGrounded && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
                break;

            case IKMode.WalkAndIdleOnly:
                bool isIdle = isGrounded && !hasMovementInput;
                bool isWalking = isGrounded && hasMovementInput && !isRunToggled && !isSprinting;
                shouldIKBeActive = (isIdle || isWalking) && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
                break;
        }

        if (shouldIKBeActive)
        {
            leftFootIKWeight = Mathf.Lerp(leftFootIKWeight, 1, Time.deltaTime * ikSmoothness);
            rightFootIKWeight = Mathf.Lerp(rightFootIKWeight, 1, Time.deltaTime * ikSmoothness);
        }
        else
        {
            leftFootIKWeight = Mathf.Lerp(leftFootIKWeight, 0, Time.deltaTime * ikSmoothness);
            rightFootIKWeight = Mathf.Lerp(rightFootIKWeight, 0, Time.deltaTime * ikSmoothness);
        }

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);

        if (leftFootIKWeight > 0.01f)
        {
            RaycastHit hit;
            Vector3 leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            if (Physics.Raycast(leftFootPos + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance + 0.5f, groundMask))
            {
                Vector3 targetPos = hit.point + new Vector3(0, footOffset, 0);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, targetPos);
                Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * animator.GetIKRotation(AvatarIKGoal.LeftFoot);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, targetRot);
            }
        }
        if (rightFootIKWeight > 0.01f)
        {
            RaycastHit hit;
            Vector3 rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
            if (Physics.Raycast(rightFootPos + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance + 0.5f, groundMask))
            {
                Vector3 targetPos = hit.point + new Vector3(0, footOffset, 0);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, targetPos);
                Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * animator.GetIKRotation(AvatarIKGoal.RightFoot);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, targetRot);
            }
        }
    }
}