using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    private int JumpCounter = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetJump()
    {
        return JumpCounter;
    }

    public void ZeroJump()
    {
        JumpCounter = 0;
    }

    public int Jump()
    {
        JumpCounter++;
        return JumpCounter;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.tag == "Obstacle")
        {
            JumpCounter = 0;
        }
    }
}
