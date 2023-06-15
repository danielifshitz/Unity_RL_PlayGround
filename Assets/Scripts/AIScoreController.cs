using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIScoreController : MonoBehaviour
{
    private static AIScoreController instance;
    private Dictionary<float, int> StageWins;

    // Static method to access the singleton instance
    public static AIScoreController Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
        else
        {
            // Set the instance
            instance = this;
            // Make the Score Manager persistent across scenes
            DontDestroyOnLoad(gameObject);
        }
    }

    // Other methods for score management
    public void IncrementScore(float StageDifficulty)
    {
        if (StageWins.ContainsKey(StageDifficulty))
        {
            StageWins[StageDifficulty]++;
        }
        else
        {
            StageWins.Add(StageDifficulty, 0);
        }
    }

    public int GetStageWins(float StageDifficulty)
    {
        if (!StageWins.ContainsKey(StageDifficulty))
        {
            StageWins.Add(StageDifficulty, 0);
        }

        return StageWins[StageDifficulty];
    }
}
