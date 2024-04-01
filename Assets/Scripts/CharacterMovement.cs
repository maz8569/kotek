using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readytoJump;
    bool readytoAttack;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;

    public Animator animator;
    public float attackCooldown;

    public bool moving;
    public bool running;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
        readytoAttack = true;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readytoJump && grounded)
        {
            readytoJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(Input.GetMouseButtonDown(0) && readytoAttack && grounded)
        {
            readytoAttack = false;

            Attack();

            Invoke(nameof(ResetAttack), attackCooldown);
        }

        running = Input.GetKey(runKey) && grounded;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        moving = moveDirection != Vector3.zero;

        animator.SetBool("Walking", moving);
        animator.SetBool("Running", running);

        if (grounded)
        {
            if (running)
            {
                rb.AddForce(20f * movementSpeed * moveDirection.normalized, ForceMode.Force);
            }
            else
            {
                rb.AddForce(10f * movementSpeed * moveDirection.normalized, ForceMode.Force);
            }
        }
        else
        {
            rb.AddForce(10f * airMultiplier * movementSpeed * moveDirection.normalized, ForceMode.Force);
        }
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        animator.SetBool("Grounded", grounded);
        animator.SetBool("Jumping", false);

        MyInput();
        SpeedControl();

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > movementSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * movementSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        animator.SetBool("Jumping", true);

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readytoJump = true;
    }

    private void Attack()
    {
        animator.SetBool("Attacking", true);
    }

    private void StopAttack()
    {
        animator.SetBool("Attacking", false);
    }

    private void ResetAttack()
    {
        animator.SetBool("Attacking", false);
        readytoAttack = true;
    }
}
