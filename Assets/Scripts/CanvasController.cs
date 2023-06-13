using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private TextMeshProUGUI GameTime;

    private float PlayingTime = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float myTime = Time.realtimeSinceStartup;
        string timeText = System.TimeSpan.FromSeconds(myTime).ToString("hh':'mm':'ss");
        GameTime.text = timeText;
    }
}
