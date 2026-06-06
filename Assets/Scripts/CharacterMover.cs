using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class CharacterMover : MonoBehaviour
{
    [Header("Movement Speed")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    [Header("Rotation Settings")]
    [Tooltip("Normal smooth rotation speed")]
    public float rotationSpeed = 720f;
    [Tooltip("Angle threshold to trigger quick turn (degrees)")]
    public float quickTurnAngle = 90f;
    [Tooltip("Rotation speed during quick turn, higher value means faster rotation")]
    public float quickTurnRotationSpeed = 1440f;

    [Header("Physics")]
    public float gravity = 20.0f;
    [Header("Animation Parameters")]
    public float animationWalkSpeed = 1.0f;
    public float animationRunSpeed = 2.0f;
    public float animationDampTime = 0.1f;

    [Header("IK Settings")]
    [Tooltip("Master switch to enable/disable IK in game")]
    public bool ikActive = true;

    [Tooltip("Target layer for IK (ground layer)")]
    public LayerMask groundLayer;
    [Space]
    [Tooltip("Foot offset from ground to prevent sinking")]
    public float footOffset = 0.05f;

    private CharacterController characterController;
    private Animator animator;
    private Camera mainCamera;

    private bool isRunning = false;
    private float verticalVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontalInput, 0, verticalInput);

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isRunning = !isRunning;
        }

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();


        Vector3 cameraRight = mainCamera.transform.right;
         cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 desiredMoveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        animator.SetFloat("MoveMagnitude", inputDirection.magnitude, animationDampTime, Time.deltaTime);

        float currentMoveSpeed = isRunning ? runSpeed : walkSpeed;
        if (inputDirection.magnitude < 0.1f)
        {
            currentMoveSpeed = 0;
        }

        Vector3 localMoveDirection = transform.InverseTransformDirection(desiredMoveDirection);
        float animationSpeedRatio = runSpeed > 0 ? currentMoveSpeed / runSpeed : 0;

        animator.SetFloat("ForwardSpeed", localMoveDirection.z * animationSpeedRatio * animationRunSpeed, animationDampTime, Time.deltaTime);
        animator.SetFloat("StrafeSpeed", localMoveDirection.x * animationSpeedRatio * animationRunSpeed, animationDampTime, Time.deltaTime);

        if (desiredMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

              float angle = Vector3.Angle(transform.forward, desiredMoveDirection);

            float currentRotationSpeed = (angle > quickTurnAngle) ? quickTurnRotationSpeed : rotationSpeed;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentRotationSpeed * Time.deltaTime);
        }

        Vector3 moveVector = desiredMoveDirection * currentMoveSpeed;

        if (characterController.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        moveVector.y = verticalVelocity;
        characterController.Move(moveVector * Time.deltaTime);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !ikActive)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
             animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            return;
        }

        float ikWeight = 1.0f - animator.GetFloat("MoveMagnitude");

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);

        RaycastHit hit;
        Vector3 leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        if (Physics.Raycast(leftFootPos + Vector3.up * 0.5f, Vector3.down, out hit, 1.0f, groundLayer))
        {
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + new Vector3(0, footOffset, 0));

            Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRotation);
        }


        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);

        Vector3 rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        if (Physics.Raycast(rightFootPos + Vector3.up * 0.5f, Vector3.down, out hit, 1.0f, groundLayer))
        {
            animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + new Vector3(0, footOffset, 0));
            Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
        }
    }
}