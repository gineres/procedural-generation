using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float jumpForce = 10f;
    public bool invertHorizontal = false;
    public bool invertVertical = false;

    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerHeight;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerHeight = characterController.height;
    }

    private void Update()
    {
        GroundCheck();

        float horizontalInput = invertHorizontal ? -Input.GetAxis("Horizontal") : Input.GetAxis("Horizontal");
        float verticalInput = invertVertical ? -Input.GetAxis("Vertical") : Input.GetAxis("Vertical");
        Vector3 playerInput = new Vector3(horizontalInput, 0, verticalInput);
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);

        Vector3 moveDirection = playerInput.x * transform.right + playerInput.z * transform.forward;
        characterController.Move(moveDirection * walkSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    private void GroundCheck()
    {
        groundedPlayer = characterController.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }
}
