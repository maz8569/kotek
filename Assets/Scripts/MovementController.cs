using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private CharacterController characterController;

    private int isWalkingHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int isGroundedHash;
    private int isAttackingHash;

    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private Vector3 currentRunMovement;
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float runMultiplier = 2.0f;
    [SerializeField] private Transform orientation;
    private bool isMovementPressed;
    private bool isRunPressed;

    private Animator animator;

    private float groundedGravity = -.1f;
    private float gravity = -9.8f;

    private float prevY;
    private bool prevIsGrounded;
    private bool isJumpPressed = false;
    private bool isJumping = false;
    private float initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 1.0f;
    [SerializeField] private float maxJumpTime = 0.5f;
    [SerializeField] private float jumpCooldown = 0.1f;

    private bool isAttackPressed = false;
    private bool readytoAttack = true;
    public float attackCooldown;

    [SerializeField] private MenuControl menuWindow;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("Walking");
        isRunningHash = Animator.StringToHash("Running");
        isJumpingHash = Animator.StringToHash("Jumping");
        isGroundedHash = Animator.StringToHash("Grounded");
        isAttackingHash = Animator.StringToHash("Attacking");

        inputActions.CharacterControls.Move.started += OnMovementInput;
        inputActions.CharacterControls.Move.performed += OnMovementInput;
        inputActions.CharacterControls.Move.canceled += OnMovementInput;
        inputActions.CharacterControls.Run.started += OnRun;
        inputActions.CharacterControls.Run.canceled += OnRun;
        inputActions.CharacterControls.Jump.started += OnJump;
        inputActions.CharacterControls.Jump.canceled += OnJump;
        inputActions.CharacterControls.Attack.started += OnAttack;
        inputActions.CharacterControls.Attack.canceled += OnAttack;
        inputActions.CharacterControls.Menu.performed += OnPause;

        SetupJumpVariables();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        menuWindow.gameObject.SetActive(true);
        inputActions.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UnPause(object sender, EventArgs e)
    {
        inputActions.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        isAttackPressed = context.ReadValueAsButton();
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        Debug.Log(gravity);
        Debug.Log(initialJumpVelocity);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
    }

    private void HandleAnimatios()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
        bool isJumpingAnimating = animator.GetBool(isJumpingHash);
        bool isGrounded = animator.GetBool(isGroundedHash);
        bool isAttacking = animator.GetBool(isAttackingHash);

        if(isMovementPressed && !isWalking) animator.SetBool(isWalkingHash, true);
        else if(!isMovementPressed && isWalking) animator.SetBool(isWalkingHash, false);

        if (isRunPressed && !isRunning) animator.SetBool(isRunningHash, true);
        else if (!isRunPressed && isRunning) animator.SetBool(isRunningHash, false);

        //if (isJumpingAnimating) animator.SetBool(isJumpingHash, false);

        if (prevIsGrounded && !isGrounded) animator.SetBool(isGroundedHash, true);
        else if (!prevIsGrounded && isGrounded) animator.SetBool(isGroundedHash, false);

        if (isAttackPressed && readytoAttack && !isAttacking) 
        {
            readytoAttack = false;
            animator.SetBool(isAttackingHash, true);
            Invoke(nameof(ResetAttack), attackCooldown);
        }
        else if (!isAttackPressed && isAttacking) animator.SetBool(isAttackingHash, false);
    }

    private void ResetAttack()
    {
        readytoAttack = true;
    }

    private void HandleGravity()
    {
        //bool isFalling = currentMovement.y <= 0;
        if(characterController.isGrounded)
        {
            //animator.SetBool(isJumpingHash, false);
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentRunMovement.y + gravity * Time.deltaTime;
            float nextYVelocity = (previousYVelocity +  newYVelocity) * .5f; 
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
    }

    private void HandleJump()
    {
        if (!isJumping && prevIsGrounded && isJumpPressed)
        {
            isJumping = true;
            animator.SetBool(isJumpingHash, true);
            Invoke(nameof(StopJumpAnitmation), jumpCooldown);
            currentMovement.y = initialJumpVelocity * 0.5f;
            currentRunMovement.y = initialJumpVelocity * 0.5f;
        }
        else if(isJumping && prevIsGrounded && !isJumpPressed)
        {
            isJumping = false;
        }
    }

    private void StopJumpAnitmation()
    {
        animator.SetBool("Jumping", false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleAnimatios();
        prevY = currentMovement.y;
        currentMovement = orientation.forward * currentMovementInput.y + orientation.right * currentMovementInput.x;
        currentRunMovement = currentMovement * runMultiplier;
        currentMovement.y = prevY;
        currentRunMovement.y = prevY;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        HandleGravity();
        HandleJump();

        if (isRunPressed) { 
            characterController.Move(movementSpeed  * Time.deltaTime * currentRunMovement);
        }
        else
        {
            characterController.Move(movementSpeed * Time.deltaTime * currentMovement);
        }
        prevIsGrounded = characterController.isGrounded;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        menuWindow.unpause += UnPause;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        menuWindow.unpause -= UnPause;
    }
}
