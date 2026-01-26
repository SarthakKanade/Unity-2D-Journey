using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;

    int score = 0;

    void OnEnable()
    {
        GameEvents.OnScoreGained += AddScore;
    }

    void OnDisable()
    {
        GameEvents.OnScoreGained -= AddScore;
    }

    private void AddScore(int additionalScore)
    {
        score += additionalScore;
        scoreText.text = "Score: " + score;
    }
}
