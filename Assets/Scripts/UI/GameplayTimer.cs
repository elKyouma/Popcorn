using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private float timeElapsed;
    private bool timerActive;
    [SerializeField] private float scoreIncreaseRate = 10f;
    public int score = 0;

    void Start()
    {
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (timerActive)
        {
            timeElapsed += Time.deltaTime;
            AccumulateScore();
        }
    }

    public void StartTimer()
    {
        timerActive = true;
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void ResetTimer()
    {
        timeElapsed = 0;
        score = 0; 
        UpdateScoreDisplay();
    }

    private void AccumulateScore()
    {
        score = (int)(timeElapsed * scoreIncreaseRate);
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private string FormatTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60);
        int seconds = Mathf.FloorToInt(timeToFormat % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
