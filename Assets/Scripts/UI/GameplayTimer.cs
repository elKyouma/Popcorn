using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float timeElapsed;
    private bool timerActive;

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
            UpdateTimerDisplay();
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
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timeElapsed);
        }
    }

    private string FormatTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60);
        int seconds = Mathf.FloorToInt(timeToFormat % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
