using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class sTime : MonoBehaviour
{
    // Коэфициент ускорения времени
    public int TimeSpeed = 1;

    // Текстовый объект для вывода на экран времени
    [SerializeField]
    Text _Clock;

    // Объект для вывода на экран времени
    [NonSerialized]
    public DateTime DT;

    // Пары самолетов для отслеживания расстояний между ними
    [SerializeField]
    String[] _PairsToMonitor = { "244201,244202", "244203,244204", "244205,244206", "244207,244208", "244209,24420A", "24420A,24420B" };

    // Текстовый объект для вывода на экран пар отслеживания
    [SerializeField]
    Text _Pairs;

    // Точное время
    Stopwatch _StopWatch;
    public long StartTime;
    public long UnixStartTime; // стартовое время Unix

    // Момент установки нового коэфициента ускорения времени TimeSpeed
    private long _NewSpeedRealTime; // Реальное время в миллисекундах от начала работы программы
    private long _NewSpeedCurrentTime = 0; // Текущее время в программе с учетом коэфициента ускорения


    void Awake()
    {
        // Параметры времени
        _StopWatch = new Stopwatch();
        _StopWatch.Start();
        StartTime = _StopWatch.ElapsedMilliseconds;
        _NewSpeedRealTime = StartTime;
        UnixStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        DT = new DateTime(2019, 4, 1, 10, 00, 00); // год - месяц - день - час - минута - секунда

        // Пары самолетов для отслеживания расстояний между ними
    }

    public int CurrentTime()
    {
        //return (int)(_StopWatch.ElapsedMilliseconds * TimeSpeed - StartTime);
        return (int)(_NewSpeedCurrentTime + (_StopWatch.ElapsedMilliseconds - _NewSpeedRealTime) * TimeSpeed);
    }

    public void ChangeTimeSpeed(int TimeSpeedShift)
    {
        // Коэффициент времени не должен быть меньше 1
        if (TimeSpeed == 1 && TimeSpeedShift < 0)
        {
            return;
        }
        // Коэффициент времени не должен быть больше 10
        else if (TimeSpeed == 10 && TimeSpeedShift > 0)
        {
            return;
        }

        // Новые параметры времени
        _NewSpeedCurrentTime = CurrentTime(); // Текущее время в программе с учетом коэфициента ускорения (до установки нового значения коэффициента)
        _NewSpeedRealTime = _StopWatch.ElapsedMilliseconds; // Реальное время в миллисекундах от начала работы программы
        TimeSpeed += TimeSpeedShift; // Коэфициент ускорения времени
    }

    private void Update()
    {

        _Clock.text = DT.AddSeconds(CurrentTime() / 1000).ToString("HH:mm:ss", CultureInfo.CurrentCulture) + " (x" + TimeSpeed + ")";
    }

}
