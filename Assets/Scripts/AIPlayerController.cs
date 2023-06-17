using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AIPlayerController : Agent
{
    [SerializeField]
    private float Speed = 8f;
    [SerializeField]
    private EnvironmentSetup Environment;
    
    private JumpController Jump;
    private Rigidbody rb;
    private Vector3 StartingPosition;
    private GameObject Door;
    private float RewardShapping;
    private Color LineToDoor;
    private string LineToDoorName;
    private AIScoreController AIScoreController;
    private bool TrainedGood;
    private float IncreaseNegativeReward;
    // Start is called before the first frame update

    void Start()
    {
        Door = null;
        StartingPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        AIScoreController = AIScoreController.Instance;
        IncreaseNegativeReward = 1;
    }
    public override void Initialize()
    {
        Jump = GetComponentsInChildren<JumpController>()[0];
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Zero out velocities so that movement stops before a new episode begins
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        float number = Random.Range(0f, 1f);
        Door = FindDoor();
        RewardShapping = Environment.StageDifficulty;
        TrainedGood = AIScoreController.GetStageWins(RewardShapping) > 100;
        float z = StartingPosition.z;
        if (TrainedGood)
        {
            IncreaseNegativeReward = Mathf.Log10(AIScoreController.GetStageWins(RewardShapping));
            Vector3 capsuleScale = transform.localScale;
            float capsuleRadius = Mathf.Max(capsuleScale.x, capsuleScale.z) * 0.5f;
            z = Random.Range(Environment.D + capsuleRadius, Environment.U - capsuleRadius);
        }

        if (TrainedGood || number >= 0.3f)
        {
            transform.position = new Vector3(StartingPosition.x, StartingPosition.y, z); ;
            LineToDoor = Color.red;
            LineToDoorName = "red";
        }
        else if (number >= 0.1f)
        {
            MovePlayerToEmptyLocation();
            LineToDoor = Color.blue;
            LineToDoorName = "blue";
        }
        else if(number >= 0.03f)
        {
            transform.position = new Vector3(Door.transform.position.x - 1, StartingPosition.y, z);
            RewardShapping = 1;
            LineToDoor = Color.yellow;
            LineToDoorName = "yellow";
        }
        else
        {
            transform.position = new Vector3(Door.transform.position.x - 1, StartingPosition.y, Door.transform.position.z);
            RewardShapping = 1;
            LineToDoor = Color.cyan;
            LineToDoorName = "cyan";
        }
    }

    private void MovePlayerToEmptyLocation()
    {
        int maxAttempts = 100;
        Vector3 capsuleScale = transform.localScale;
        float capsuleRadius = Mathf.Max(capsuleScale.x, capsuleScale.z) * 0.5f;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate random position
            float x = Random.Range(Environment.L + capsuleRadius, Environment.R - capsuleRadius);
            float z = Random.Range(Environment.D + capsuleRadius, Environment.U - capsuleRadius);
            for (float y = StartingPosition.y; y < 5 + StartingPosition.y; y++)
            {
                Vector3 randomPosition = new Vector3(x, y, z);

                // Check if the position is empty
                if (IsPositionEmpty(randomPosition, capsuleRadius))
                {
                    // Instantiate the object at the empty position
                    transform.position = randomPosition;
                    return; // Exit the loop since an object was successfully instantiated
                }
            }
        }

        // If no empty position found after maxAttempts, raise an error
        Debug.LogError("Unable to find an empty position for object instantiation.");
    }

    private bool IsPositionEmpty(Vector3 position, float radius)
    {
        // Perform your custom logic to check if the position is empty
        // You can use Physics.CheckSphere or other methods to check for collisions or occupancy
        // Return true if the position is considered empty, false otherwise
        // Example:
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        return colliders.Length == 0; // Return true if no colliders found at the position
    }

    public GameObject FindDoor()
    {
        Vector3 location = transform.position;
        GameObject closesCloseDoor = null;
        GameObject closesOpentDoor = null;
        float closestCloseDistance = 0;
        float closestOpenDistance = 0;

        foreach (var door in Environment.Doors)
        {
            var tempDistance = Vector3.Distance(location, door.transform.position);

            if (door.tag == "Close door")
            {
                if (closesCloseDoor != null && tempDistance >= closestCloseDistance)
                {
                    continue;
                }

                closesCloseDoor = door;
                closestCloseDistance = tempDistance;
            }

            else
            {
                if (closesOpentDoor != null && tempDistance >= closestOpenDistance)
                {
                    continue;
                }

                closesOpentDoor = door;
                closestOpenDistance = tempDistance;
            }
        }

        // Debug.Log("Door location: " + closestDoor.transform.position);
        if (closesOpentDoor != null)
        {
            return closesOpentDoor;
        }

        return closesCloseDoor;
    }

    //public void ResetPlayer()
    //{
    //    transform.position = StartingPosition;
    //}

    /// <summary>
    /// Called when an action is received from either the player input or the neural network.
    ///
    /// actions[i] represents:
    /// Index 0: move vector x (+1 = right,     -1 = left)
    /// Index 2: move vector z (+1 = forward,   -1 = backward)
    /// Index 3: should jump (1 or 0)
    /// </summary>
    /// <param name="actions">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveY = 0;

        if (actions.ContinuousActions[2] > 0.5)
        {
            int jump = Jump.Jump();
            if (jump == 1)
            {
                // rb.AddForce(new Vector3(0, 25, 0) * Speed);
                moveY = 20;
            }

            else if (jump == 2)
            {
                // rb.AddForce(new Vector3(0, 15, 0) * Speed);
                moveY = 10;
            }
        }

        // Calculate movement vector
        Vector3 moveXZ = new Vector3(actions.ContinuousActions[0], moveY, actions.ContinuousActions[1]);

        // Add force in the direction of the move vector
        rb.AddForce(moveXZ * Speed);

        AddReward(-0.015f * IncreaseNegativeReward);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent position - 3 observations
        sensor.AddObservation(transform.position.normalized);

        // Observe the agent velocity - 6 observations
        sensor.AddObservation(rb.angularVelocity.normalized);
        sensor.AddObservation(rb.velocity.normalized);

        // Observe a normalized vector pointing to the nearest pickup - 3 observations
        if (Door != null)
        {
            sensor.AddObservation(Door.transform.position.normalized);
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
        }
        // 9 total observations
    }

    /// <summary>
    /// Called when the agent's collide enters a trigger collider
    /// </summary>
    /// <param name="other">The trigger collider</param>
    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void FixedUpdate()
    {
        if (Jump.GetJump() > 0)
            rb.velocity = new Vector3(rb.velocity.x * 0.95f, rb.velocity.y, rb.velocity.z * 0.95f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            Debug.Log("Loss (" + Environment.StageDifficulty + ") (" + LineToDoorName + ")");
            AddReward(-100f / RewardShapping);
            Environment.ResetEnvironment();
            EndEpisode();
        }

        else if (collision.gameObject.CompareTag("Open door"))
        {
            Debug.Log("Win (" + Environment.StageDifficulty + ") (" + LineToDoorName + ")");
            AddReward(25f * RewardShapping);
            if (LineToDoorName == "red")
            {
                AIScoreController.IncrementScore(RewardShapping);
            }

            Environment.ResetEnvironment();
            EndEpisode();
        }

        else if (collision.gameObject.CompareTag("DontStandOnIt"))
        {
            AddReward(-0.05f * IncreaseNegativeReward);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Door != null)
        {
            Debug.DrawLine(transform.position, Door.transform.position, LineToDoor);
        }
    }
}
