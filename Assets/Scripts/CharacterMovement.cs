using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 8f; // Adjust as needed
    private bool isJumping = false;

    private void Update()
    {
        // Get user input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Rotate towards the terrain normal
        RotateTowardsNormal();

        // Move the character
        MoveCharacter(direction);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            Jump();
        }
    }

    private void MoveCharacter(Vector3 direction)
    {
        // Move the character based on the direction
        transform.Translate(direction * speed * Time.deltaTime, Space.Self);
    }

    private void RotateTowardsNormal()
    {
        // Raycast to determine the ground normal
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            // Rotate towards the terrain normal
            Vector3 slopeNormal = hit.normal;
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, slopeNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        // Apply an upward force for jumping
        GetComponent<Rigidbody>().velocity = new Vector3(0, jumpForce, 0);
        isJumping = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Reset isJumping when colliding with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
}
