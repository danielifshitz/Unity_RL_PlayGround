using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float Speed = 10;
    [SerializeField]
    private EnvironmentSetup Environment;

    private JumpController Jump;
    private Rigidbody rb;
    private float MovementX;
    private float MovementZ;
    private Vector3 StartingPosition;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Jump = GetComponentsInChildren<JumpController>()[0];
        Debug.Assert(Jump  != null);
        StartingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetPlayer()
    {
        transform.position = StartingPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void OnMove(InputValue movementValue)
    {
        var movementVector = movementValue.Get<Vector2>();
        MovementX = movementVector.x;
        MovementZ = movementVector.y;
    }

    public void OnJump()
    {
        int jump = Jump.Jump();
        if (jump == 1)
        {
            rb.AddForce(new Vector3(0, 25, 0) * Speed);
        }

        else if (jump == 2)
        {
            rb.AddForce(new Vector3(0, 15, 0) * Speed);
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(MovementX, 0, MovementZ) * Speed);
        if (Jump.GetJump() > 0)
            rb.velocity = new Vector3(rb.velocity.x * 0.96f, rb.velocity.y, rb.velocity.z * 0.96f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            ResetPlayer();
        }

        else if (collision.gameObject.CompareTag("Open door"))
        {
            Debug.Log("Win");
            ResetPlayer();
            Environment.ResetEnvironment();
        }
    }
}
