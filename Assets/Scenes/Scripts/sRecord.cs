using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class sRecord : MonoBehaviour
// ********************** Запись данных в файлы ********************************************
{

    // Папка для записи файлов
    public String RecDir = "Record";
    // Словарь - массив файлов для записи данных. Ключ - имя файла, значение - объект StreamWriter
    Dictionary<String, StreamWriter> _RecFile = new Dictionary<String, StreamWriter>();
    /*
    Main     - Файл для записи по умолчанию
    RawData  - Файл для записи получаемых данных
    Thread   - Файл для записи в фоновом потоке
    ProcData - Файл для записи в процессе обработки данных (комбинированный поток: фоновый + корутина)
    Update   - Файл для записи в каждом кадре
    Banners  - Файл для отладки "Избавиться от наложения баннеров"

    ADSB_Exchange - Файл для записи исходных данных adsbexchange.com
    OpenSky - Файл для записи исходных данных opensky-network.org
    */


    // Отладочный параметр - запиcывать ли логи
    public bool WriteLog = true;

    // Запиcывать ли исходные данные web
    [SerializeField]
    bool _WriteWebData = true;

    // Запиcывать ли протокол опасных сближений
    public bool WriteAlerts = true;

    // Параметры времени
    sTime _Time;


    void Awake()
    {

        // ********************** Запись данных в файлы ********************************************

        // Создать папку
        Directory.CreateDirectory(RecDir);
        RecDir = Path.Combine(Directory.GetCurrentDirectory(), RecDir);

        if (WriteLog)
        {
            // Файл для записи по умолчанию
            AddToDic("Main");
            // Файл для записи получаемых данных
            AddToDic("RawData");
            // Файл для записи в фоновом потоке
            AddToDic("Thread");
            // Файл для записи в процессе обработки данных (комбинированный поток: фоновый + корутина)
            AddToDic("ProcData");
            // Файл для записи в каждом кадре
            AddToDic("Update");
            // Файл для отладки "Избавиться от наложения баннеров"
            AddToDic("Banners");
        }

        if (_WriteWebData)
        {
            //Adsbexchange - Файл для записи исходных данных adsbexchange.com
            //AddToDic("ADSB_Exchange");
            //OpenSky - Файл для записи исходных данных opensky-network.org
            AddToDic("OpenSky");
        }

        // ******************************************************************

        // Параметры времени
        _Time = transform.GetComponent<sTime>();

    }

    // Добавить в словарь имя файла и созданный объект StreamWriter
    public void AddToDic(String myRecFileName)
    {
        _RecFile.Add(myRecFileName, new StreamWriter(Path.Combine(RecDir, myRecFileName + ".txt")));
    }


    // ****************  4 перегруженных функции для записи лог-файлов   ********************************
    // Запись в указанный файл
    public void MyLog(string myRecName, String myInfo)
    {
        if (WriteLog)
        {
            int myCurrentTime = _Time.CurrentTime();
            _RecFile[myRecName].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
        }
    }

    // Запись в указанный файл с возможностью не добавлять время
    public void MyLog(string myRecName, String myInfo, bool myTime)
    {
        if (WriteLog)
        {
            if (myTime)
            {
                int myCurrentTime = _Time.CurrentTime();
                _RecFile[myRecName].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
            }
            else
            {
                _RecFile[myRecName].WriteLine(myInfo.Replace(".", ","));
            }
        }
    }

    // Запись в файл по умолчанию
    public void MyLog(String myInfo)
    {
        if (WriteLog)
        {
            _RecFile["Main"].WriteLine(myInfo.Replace(".", ","));
        }
    }

    // Запись в два файла
    public void MyLog(string myRecName1, string myRecName2, String myInfo)
    {
        if (WriteLog)
        {
            int myCurrentTime = _Time.CurrentTime();
            _RecFile[myRecName1].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
            _RecFile[myRecName2].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
        }
    }

    // Запись в файл web данных
    public void WebData(string myRecName, string myInfo)
    {
        if (_WriteWebData)
        {
            _RecFile[myRecName].WriteLine(myInfo);
        }
   }

    // Запись в файл опасных сближений
    public void AlertsLog(string myRecName, string myInfo)
    {
        if (WriteAlerts)
        {
            _RecFile[myRecName].WriteLine(myInfo);
        }
    }


    // ******************************************************************

    // Закрыть один лог-файл и удалить его запись из словаря лог-файлов
    public void Close(string myRecName)
    {
        if (WriteLog)
        {
            _RecFile[myRecName].Close();
            _RecFile.Remove(myRecName);
        }
    }

    // Закрыть все открытые лог-файлы
    public void CloseAll()
    {
        // Закрыть все открытые лог-файлы
        List<String> myKeys = new List<String>(_RecFile.Keys);
        for (int i = 0; i < myKeys.Count; i++)
        {
            _RecFile[myKeys[i]].Close();
        }
    }



}
