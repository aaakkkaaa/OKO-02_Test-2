using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class sTime : MonoBehaviour
{
    // Коэфициент ускорения или замедления времени
    [SerializeField]
    float _TimeSpeed = 1.0f;

    // Текстовый объект для вывода на экран времени
    [SerializeField]
    Text _Clock;

    // Объект для вывода на экран времени
    DateTime _DT;

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

    void Awake()
    {
        // Параметры времени
        _StopWatch = new Stopwatch();
        _StopWatch.Start();
        StartTime = _StopWatch.ElapsedMilliseconds;
        UnixStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        _DT = new DateTime(2019, 4, 1, 10, 01, 00); // год - месяц - день - час - минута - секунда

        // Пары самолетов для отслеживания расстояний между ними
    }

    public int CurrentTime()
    {
        return (int)(_StopWatch.ElapsedMilliseconds * _TimeSpeed - StartTime);
    }

    private void Update()
    {
        _Clock.text = _DT.AddSeconds(CurrentTime() / 1000).ToString("HH:mm:ss", CultureInfo.CurrentCulture);
    }

}
