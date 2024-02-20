using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform orientation;
    public Transform player;
    public Rigidbody rb;

    public float rotationSpeed;

    Vector3 inputDir;
    public bool moving;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    private void FixedUpdate()
    {
        moving = false;

        if (inputDir != Vector3.zero)
        {
            moving = true;
            player.forward = Vector3.Slerp(player.forward, inputDir.normalized, Time.fixedDeltaTime * rotationSpeed);
        }

        animator.SetBool("Walking", moving);
    }
}
