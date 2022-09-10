using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum TimePeriod
{
    Seconds = 0,
    Minutes,
    Hours,
    Days,
    Months,
    Years
};

public class ProgressBar : MonoBehaviour
{
    [SerializeField] RectTransform _inputBlocker;
    RawImage _inputBlockerImage;
    [SerializeField] RectTransform _setInputField;
    [SerializeField] RectTransform _setInputButton;
    [SerializeField] TMPro.TextMeshProUGUI _subtitle;

    [SerializeField] Button _startButton;
    [SerializeField] Button _resetButton;

    [SerializeField] Image _progressCircle;
    [SerializeField] TMPro.TextMeshProUGUI _timeUI;
    [SerializeField] TMPro.TextMeshProUGUI _status;
    [SerializeField] TMPro.TextMeshProUGUI _startDateUI;

    string _vice;
    TimePeriod _currentTimePeriod;
    System.DateTime _startTime;
    System.TimeSpan _totalTime;
    
    int _started = -1;
    bool Started 
    { 
        get
        { 
            return _started == 1; 
        }
    }

    const string viceKey = "viceKey";

    const string startYearKey = "startYearKey";
    const string startMonthKey = "startMonthKey";
    const string startDayKey = "startDayKey";
    const string startHourKey = "startHourKey";
    const string startMinuteKey = "startMinuteKey";
    const string startSecondsKey = "startSecondsKey";

    const string hasStartedKey = "hasStartedKey";
    const string totalTimeKey = "totalTimeKey";
    PlayerPrefs player_prefs;

    private void Awake()
    {
        _inputBlockerImage = _inputBlocker.GetComponent<RawImage>();
        _inputBlockerImage.DOFade(0, 0);
        _setInputField.DOAnchorPosX(-1100, 0);
        _setInputButton.DOAnchorPosX(1100, 0);

        _inputBlocker.gameObject.SetActive(false);
    }

    private void Start()
    {
        CheckSaveData();

        if (!Started)
        {
            _startDateUI.text = System.DateTime.Now.ToString("D");
        }
    }

    private void Update()
    {
        if(Started)
        {
            SetTimePeriod();
            CalculateProgressCircle();
            SetNumberTimePeriod();
        }
    }

    void CheckSaveData()
    {
        if (PlayerPrefs.HasKey(hasStartedKey))
        {
            _started = PlayerPrefs.GetInt(hasStartedKey, -1);
        }

        if(Started)
        {
            _startButton.gameObject.SetActive(false);
            _resetButton.gameObject.SetActive(true);
        }
        else
        {
            return;
        }

        int savedYear = 0;
        int savedMonth = 0;
        int savedDay = 0;
        int savedHour = 0;
        int savedMinute = 0;
        int savedSecond = 0;

        if (PlayerPrefs.HasKey(viceKey))
        {
            _vice = PlayerPrefs.GetString(viceKey, "");
        }

        if (PlayerPrefs.HasKey(startYearKey))
        {
            savedYear = PlayerPrefs.GetInt(startYearKey, -1);
        }

        if (PlayerPrefs.HasKey(startMonthKey))
        {
            savedMonth = PlayerPrefs.GetInt(startMonthKey, -1);
        }

        if (PlayerPrefs.HasKey(startDayKey))
        {
            savedDay = PlayerPrefs.GetInt(startDayKey, -1);
        }

        if (PlayerPrefs.HasKey(startHourKey))
        {
            savedHour = PlayerPrefs.GetInt(startHourKey, -1);
        }

        if (PlayerPrefs.HasKey(startMinuteKey))
        {
            savedMinute = PlayerPrefs.GetInt(startMinuteKey, -1);
        }

        if (PlayerPrefs.HasKey(startSecondsKey))
        {
            savedSecond = PlayerPrefs.GetInt(startSecondsKey, -1);
        }

        if(savedYear == -1 || savedMonth == -1 || savedDay == -1 || savedHour == -1 || savedMinute == -1 || savedSecond == -1)
        {
            Debug.LogError("Error in getting saved key");
        }

        _startTime = new System.DateTime(savedYear, savedMonth, savedDay, savedHour, savedMinute, savedSecond);
    }

    public void StartTracker()
    {
        if(!Started)
        {
            _startButton.gameObject.SetActive(false);
            _resetButton.gameObject.SetActive(true);

            _currentTimePeriod = TimePeriod.Seconds;
            _startTime = System.DateTime.Now;

            _started = 1;

            PlayerPrefs.SetInt(hasStartedKey, 1);

            PlayerPrefs.SetInt(startYearKey, _startTime.Year);
            PlayerPrefs.SetInt(startMonthKey, _startTime.Month);
            PlayerPrefs.SetInt(startDayKey, _startTime.Day);
            PlayerPrefs.SetInt(startHourKey, _startTime.Hour);
            PlayerPrefs.SetInt(startMinuteKey, _startTime.Minute);
            PlayerPrefs.SetInt(startSecondsKey, _startTime.Second);

            _setInputField.DOAnchorPosX(-1100, 1f);
            _setInputButton.DOAnchorPosX(1100, 1f);
            _inputBlockerImage.DOFade(0f, 1f).OnComplete(() => _inputBlocker.gameObject.SetActive(false));

            _subtitle.text = _vice + " free since";
            PlayerPrefs.SetString(viceKey, _vice);
        }
    }

    public void ResetTracker()
    {
        _started = 0;
        PlayerPrefs.SetInt(hasStartedKey, 0);

        _timeUI.text = "0";
        _status.text = "it's okay, try again!";
        _startDateUI.text = System.DateTime.Now.ToString("D");
        _progressCircle.fillAmount = 1f;

        _subtitle.text = "";

        _startButton.gameObject.SetActive(true);
        _resetButton.gameObject.SetActive(false);
    }

    public void SetVice(string value)
    {
        _vice = value;
    }

    public void ShowInput()
    {
        _inputBlocker.gameObject.SetActive(true);

        _inputBlockerImage.DOFade(0.6f, 1f);
        _setInputField.DOAnchorPosX(0, 1f);
        _setInputButton.DOAnchorPosX(0, 1f);
    }

    // e.g. X hours/days without Y 
    void SetNumberTimePeriod()
    {
        int result = 0;

        switch (_currentTimePeriod)
        {
            case TimePeriod.Seconds:
                {
                    result = _totalTime.Seconds;
                    break;
                }
            case TimePeriod.Minutes:
                {
                    result = _totalTime.Minutes;
                    break;
                }
            case TimePeriod.Hours:
                {
                    result = _totalTime.Hours;
                    break;
                }
            case TimePeriod.Days:
                {
                    result = _totalTime.Days;
                    break;
                }
        }

        var timePeriodString = result.ToString();

        _timeUI.text = timePeriodString;
        _status.text = _currentTimePeriod.ToString() + " without " + _vice;
        _startDateUI.text = System.DateTime.Now.ToString("D");
    }

    void CalculateProgressCircle()
    {
        var currentTime = System.DateTime.Now;
        _totalTime = currentTime - _startTime;

        _progressCircle.fillAmount = GetFillPercentage();
    }

    void SetTimePeriod()
    {
        switch (_currentTimePeriod)
        {
            case TimePeriod.Seconds:
                {
                    if (_totalTime.TotalSeconds > 59)
                    {
                        _currentTimePeriod = TimePeriod.Minutes;
                    }
                    break;
                }
            case TimePeriod.Minutes:
                {
                    if (_totalTime.TotalMinutes > 59)
                    {
                        _currentTimePeriod = TimePeriod.Hours;
                    }
                    break;
                }
            case TimePeriod.Hours:
                {
                    if (_totalTime.Hours > 24)
                    {
                        _currentTimePeriod = TimePeriod.Days;
                    }
                    break;
                }
            case TimePeriod.Days:// TODO: need accurate months
                {
                    if (_totalTime.Days > System.DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month)) 
                    {
                        _currentTimePeriod = TimePeriod.Months;
                    }
                    break;
                }
            default:
                break;
        }

    }


    float GetFillPercentage()
    {
        float result = 0;

        switch (_currentTimePeriod)
        {
            case TimePeriod.Seconds:
                {
                    result = _totalTime.Seconds / 60f;
                    break;
                }
            case TimePeriod.Minutes:
                {
                    result = _totalTime.Minutes / 60f;
                    break;
                }
            case TimePeriod.Hours:
                {
                    result = _totalTime.Hours / 24;
                    break;
                }
            case TimePeriod.Days:
                {
                    result = _totalTime.Days / System.DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month);
                    break;
                }
            //case TimePeriod.Months:
            //    result = timeSpan.Days * 7 * 4 / 12;
            //    break;
            //case TimePeriod.Years:
            //    // none
            //    break;
            default:
                break;
        }

        return result;
    }
}
