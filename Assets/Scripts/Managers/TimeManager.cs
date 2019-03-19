using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
 
    [System.Serializable]
    public struct TimeStamp
    {
        public int hours, minutes, seconds;
        public float secondsTotal;
    }

    public TimeStamp timeStamp;
    public float currentTime = 720;
    public int currentDay = 1;
    public string formattedTime = "00:00";
    public TextMeshProUGUI timeText;

    private float subTimeTT = 1f;

    private int[] timeOffsets = { 70, 70, 70, 70, 70, 40, 30, 20, 10, 0, 0, 0, 0, 0, 0, 0, 10, 20, 30, 40, 70, 70, 70, 70 };

    void Start()
    {
        GameManager.manager.timeManager = this;

        UpdateFormattedTime();
        UpdateDirectionalLight();
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= 1439)
        { // if the last tick was 23:59, add a new day
            currentTime = 0;
            currentDay++;
        }

        UpdateFormattedTime();
        UpdateDirectionalLight();
    }

    void UpdateFormattedTime()
    {
        timeStamp.secondsTotal = currentTime;
        timeStamp.hours = (int)Mathf.Floor(currentTime / 60);
        timeStamp.minutes = (int)Mathf.Floor(currentTime % 60);
        timeStamp.seconds = (int)Mathf.Floor((currentTime * 60) % 60);

        formattedTime = timeStamp.hours.ToString("00") + ":" + timeStamp.minutes.ToString("00") + ":" + timeStamp.seconds.ToString("00");

        if (timeText != null && timeText.text != formattedTime)
        {
            timeText.text = formattedTime;
        }
    }

    void UpdateDirectionalLight()
    {
        int curHour = Mathf.RoundToInt(Mathf.Floor(currentTime / 60));
        int curMinute = Mathf.RoundToInt(currentTime % 60);
        float hrPercent = (float)curMinute / 60;

        float curTimeOffset = timeOffsets[curHour];
        float nextTimeOffset = 0;

        if (curHour >= 23)
        {
            nextTimeOffset = timeOffsets[0];
        }
        else
        {
            nextTimeOffset = timeOffsets[curHour + 1];
        }

        Transform dLight = GameObject.FindGameObjectWithTag("MainLight").transform;
        float correctTimeOffset = curTimeOffset + ((nextTimeOffset - curTimeOffset) * hrPercent);

        dLight.localEulerAngles = new Vector3(correctTimeOffset, dLight.localEulerAngles.y, dLight.localEulerAngles.z);
    }

    public void Load ()
    {
        timeStamp = GameManager.manager.saveManager.SaveData.worldData.time;
        currentTime = timeStamp.secondsTotal;

        UpdateFormattedTime();
        UpdateDirectionalLight();
    }

    public void SetTimeScale (float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
