using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;

public class sWebData : MonoBehaviour
{

    // Структура массивов для построения туннелей (при получении данных из файла)
    struct TunneData
    {
        public List<long> time_position; // Unix timestamp (seconds) for the last position update. Can be null if no position report was received by OpenSky within the past 15s.
        public List<float> longitude; // WGS-84 longitude in decimal degrees. Can be null.
        public List<float> latitude; // WGS-84 latitude in decimal degrees. Can be null.
        public List<float> geo_altitude; // Geometric altitude in meters. Can be null.
        public List<float> true_track; // True track in decimal degrees clockwise from north (north=0°). Can be null.
        public List<Vector3> position; // Положение в сцене.
    }

    // Словарь данных для построения туннелей. Ключ - HEX код ICAO, значения - структура массивов List с данными туннелей
    Dictionary<String, TunneData> _SkyTunnel = new Dictionary<String, TunneData>();


    // Желательное время цикла запроса данных, сек.
    [SerializeField]
    float _WebCycleTime = 5.0f;

    // Рисовать каждые N ворота небесного туннеля.
    [SerializeField]
    int _FramePaintCount = 3;

    // Полный текст запроса к серверу
    public String URL;

    // Время прихода новых данных от сервера.
    public long ResponseTime = 0;

    // Текстовый объект для приема данных от сервера.
    public String ResponseStr = "";

    // Флаг: Имеются новые необработанные данные.
    public bool NewData = false;

    // Трансформ шаблона небесного туннеля
    Transform _SampleTunnel;

    // Трансформ шаблона ворот небесного туннеля
    Transform _SampleFrame;

    // Трансформ для динамически созданных небесных туннелей
    Transform _SkyTunnels;

    // Параметры времени
    sTime _Time;

    // Объект с методами для записи данных в файлы
    sRecord _Record;

    // Объект FileStream для чтения файла web-данных
    FileStream _RecFile;

    // Основной объект
    sFlightRadar myFlightRadar;


    // Start is called before the first frame update
    void Start()
    {
        // Трансформ шаблона туннеля - получить указатель и сразу спрятать
        _SampleTunnel = GameObject.Find("SampleTunnel").transform;
        _SampleTunnel.gameObject.SetActive(false);
        // Трансформ шаблона ворот - получить указатель и сразу спрятать
        _SampleFrame = GameObject.Find("SampleFrame").transform;
        _SampleFrame.gameObject.SetActive(false);
        // Трансформ для динамически созданных небесных туннелей
        _SkyTunnels = GameObject.Find("SkyTunnels").transform;

        // Параметры времени
        _Time = transform.GetComponent<sTime>();

        // Ссылка на объект с методами для записи данных в файлы
        _Record = transform.GetComponent<sRecord>();

        // Основной объект
        myFlightRadar = transform.GetComponent<sFlightRadar>();
    }


    // Запросить в Интернете, получить, и записать полетные данные в текстовую строку
    public IEnumerator GetWebData()
    {
        long myWebRequestTime = 0;
        int myWebRequestCount = 0;
        int myDataTraffic = 0;

        yield return new WaitForEndOfFrame();
        _Record.MyLog("RawData", "@@@ GetWebData(): Начну выполнять запросы через ~ 1 секунду");
        yield return new WaitForSeconds(1);
        _Record.MyLog("RawData", "@@@ GetWebData(): Подождали 1 секунду, начинаем");

        while (true)
        {
            myWebRequestTime = _Time.CurrentTime();
            _Record.MyLog("RawData", "@@@ GetWebData(): Начинаю запрос. Время = " + myWebRequestTime + " myURL = " + URL);

            // Готовим запрос
            UnityWebRequest myRequest = UnityWebRequest.Get(URL);
            // Выполняем запрос и получаем ответ
            yield return myRequest.SendWebRequest();
            // Зафиксируем время ответа и интервал времени от предыдущего ответа
            ResponseTime = _Time.CurrentTime(); // Время получения данных от сервера


            myWebRequestTime = ResponseTime - myWebRequestTime; // Время, которое выполняли запрос и получали ответ
            myWebRequestCount++; // Номер запроса

            if (myRequest.isNetworkError || myRequest.isHttpError)
            {
                _Record.MyLog("RawData", "@@@ GetWebData(): Запрос не выполнен. Номер запроса = " + myWebRequestCount + " Время на запрос/ответ = " + myWebRequestTime);
                _Record.MyLog("RawData", "@@@ GetWebData(): Ошибка " + myRequest.error + " Продолжу работать через ~3 секунды");
                yield return new WaitForSeconds(3);
            }
            else
            {
                // Results as text
                ResponseStr = myRequest.downloadHandler.text;
                _Record.WebData("OpenSky", ResponseStr);
                // Установим флаг "Имеются новые необработанные данные"
                NewData = true;
                // Отчитаемся о результатах запроса
                myDataTraffic += ResponseStr.Length;
                _Record.MyLog("RawData", "@@@ GetWebData(): Запрос выполнен. NewData = " + NewData + " Номер запроса = " + myWebRequestCount + " Время прихода ответа = " + ResponseTime + " Время на запрос/ответ = " + myWebRequestTime + " Получена строка длиной = " + ResponseStr.Length + " Общий траффик авиаданных = " + myDataTraffic);
                _Record.MyLog("RawData", "@@@ GetWebData(): " + ResponseStr);
            }

            myRequest.Dispose(); // завершить запрос, освободить ресурсы

            // Переждать до конца рекомендованного времени цикла, секунд (если запрос занял времени меньше)
            float myWaitTime = Mathf.Max(0.0f, (_WebCycleTime - myWebRequestTime / 1000.0f));
            _Record.MyLog("RawData", "@@@ GetWebData(): Переждем до следующего запроса секунд: " + myWaitTime + ".");
            yield return new WaitForSeconds(myWaitTime);
            _Record.MyLog("RawData", "@@@ GetWebData(): Переждали еще секунд: " + myWaitTime + " Буду делать следующий запрос");
        }

    }

    // Получать данные, записанные в файле, выдавать по одной текстовой строке в указанное время
    public IEnumerator GetFileData()
    {
        int FileProcTime = 0;
        int RecordsCount = 0;

        yield return new WaitForEndOfFrame();
        _Record.MyLog("RawData", "@@@ GetFileData(): Начну читать файл через ~ 1 секунду");
        yield return new WaitForSeconds(1);
        _Record.MyLog("RawData", "@@@ GetFileData(): Подождали 1 секунду, начинаем");

        // Считать весь файл строку за срокой и записать в массив RecData
        FileProcTime = _Time.CurrentTime();
        string[] RecData = File.ReadAllLines(Path.Combine(_Record.RecDir, "Record.txt"));
        RecordsCount = RecData.Length;
        FileProcTime = _Time.CurrentTime() - FileProcTime;
        _Record.MyLog("RawData", "@@@ GetFileData(): Файл считан. Время чтения: " + FileProcTime + " Всего записей: " + RecordsCount);

        // Разобрать время каждой записи и создать параллельный массив времен (на одну запись больше)
        // Инициализация массива
        int[] RecTime = new int[RecordsCount + 1];
        // Время первой записи
        long.TryParse(RecData[0].Substring(8, 10), out long Rec0UnixTime);
        _Time.UnixStartTime = Rec0UnixTime;
        RecTime[0] = 0;
        // Времена остальных записей
        for (int i = 1; i < RecordsCount; i++)
        {
            long.TryParse(RecData[i].Substring(8, 10), out long RecUnixTime);
            RecTime[i] = (int)(RecUnixTime - Rec0UnixTime) * 1000;
        }
        RecTime[RecordsCount] = RecTime[RecordsCount] + 5000;
        FileProcTime = _Time.CurrentTime() - FileProcTime;
        _Record.MyLog("RawData", "@@@ GetFileData(): Все записи обработаны, создан параллельный массив времен. Время обработки: " + FileProcTime);

        // Разобрать по данные из файла по отдельным самолетам
        SeparateFileData(RecData);
        _Record.MyLog("RawData", "@@@ GetFileData(): Данные для построения туннелей разобраны по отдельным самолетам.");

        // Выдавать по одной строке во время, определенное в массиве времен
        for (int i = 0; i < RecordsCount; i++)
        {
            // Строка - запись данных
            ResponseStr = RecData[i];
            // Время, которому соответствует запись
            ResponseTime = _Time.CurrentTime();

            // Установить флаг "Имеются новые необработанные данные"
            NewData = true;

            _Record.MyLog("RawData", "@@@ GetFileData(): Выдана в обработку запись № " + i + ", время: " + RecTime[i]);

            float myWaitTime = (RecTime[i + 1] - _Time.CurrentTime()) / 1000.0f;
            _Record.MyLog("RawData", "@@@ GetFileData(): Переждем до следующей выдачи секунд: " + myWaitTime + ".");
            yield return new WaitForSeconds(myWaitTime);
            _Record.MyLog("RawData", "@@@ GetFileData(): Переждали еще секунд: " + myWaitTime + " Буду делать следующую выдачу");
        }

    }

    // Разобрать по данные из файла по отдельным самолетам
    void SeparateFileData(string[] RecData)
    {
        int RecordsCount = RecData.Length;
        print("================== Разобрать по данные из файла по отдельным самолетам ===================");
        print("Всего записей в файле: " + RecordsCount);
        for (int i = 1; i < RecordsCount; i++)
        {
            // Парсим строку и создаем объект JSON
            dynamic myJObj = JObject.Parse(RecData[i]);
            // Узел "states" - массив состояний самолетов (статических векторов)
            JArray myAcList = myJObj.states;

            // Создаем одиночный экземпляр структуры массивов
            TunneData OnePlaneTunnel = new TunneData(); // Данные одного туннеля

            // Создадим из объекта JSON структуру, добавим ее в словарный массив (или перепишем, если такая уже имеется)
            for (int j = 0; j < myAcList.Count; j++)
            {
                // JSON-массив параметров для одного самолета, полученный из myJObj (элементы массива разных типов)
                JArray myAcItem = (JArray)myAcList[j];

                // Вспомогательные переменные
                long time_position; // Unix timestamp (seconds) for the last position update. Can be null if no position report was received by OpenSky within the past 15s.
                float longitude; // WGS-84 longitude in decimal degrees. Can be null.
                float latitude; // WGS-84 latitude in decimal degrees. Can be null.
                float geo_altitude; // Geometric altitude in meters. Can be null.
                float true_track; // True track in decimal degrees clockwise from north (north=0°). Can be null.

                // Код ИКАО самолета. Unique ICAO 24-bit address of the transponder in hex string representation.
                string myKey = myAcItem[0].ToString();

                // Время последних данных о положении самолета
                long.TryParse(myAcItem[3].ToString(), out time_position);
                // Долгота
                Single.TryParse(myAcItem[5].ToString(), out longitude);
                // Широта
                Single.TryParse(myAcItem[6].ToString(), out latitude);
                // Высота по GPS
                Single.TryParse(myAcItem[13].ToString(), out geo_altitude);
                // Курс в градусах
                Single.TryParse(myAcItem[10].ToString(), out true_track);

                // Если самолет с таким ключом уже есть в словаре
                if (_SkyTunnel.TryGetValue(myKey, out OnePlaneTunnel)) // Считаем его параметры в экземпляр структуры массивов OnePlaneTunnel
                {
                    // Добавим данные новой точки в каждый массив структуры
                    OnePlaneTunnel.time_position.Add(time_position);
                    OnePlaneTunnel.longitude.Add(longitude);
                    OnePlaneTunnel.latitude.Add(latitude);
                    OnePlaneTunnel.geo_altitude.Add(geo_altitude);
                    OnePlaneTunnel.true_track.Add(true_track);
                    OnePlaneTunnel.position.Add(SceneCoordinates(longitude, latitude, geo_altitude));
                    // Перепишем пополненную структуру в словаре
                    _SkyTunnel[myKey] = OnePlaneTunnel;
                }
                else  // Создадим новый самолет в словаре
                {
                    // Инициализируем массивы в структуре массивов
                    OnePlaneTunnel.time_position = new List<long> { time_position };
                    OnePlaneTunnel.longitude = new List<float> { longitude };
                    OnePlaneTunnel.latitude = new List<float> { latitude };
                    OnePlaneTunnel.geo_altitude = new List<float> { geo_altitude };
                    OnePlaneTunnel.true_track = new List<float> { true_track };
                    OnePlaneTunnel.position = new List<Vector3> { SceneCoordinates(longitude, latitude, geo_altitude) };
                    // Добавим новую запись в словарь
                    _SkyTunnel.Add(myKey, OnePlaneTunnel);
                }

            }

        }
        print("Всего самолетов: " + _SkyTunnel.Count);
        print("Записей для каждого:");
        foreach (string myKey in _SkyTunnel.Keys)
        {
            print(myKey + ": " + _SkyTunnel[myKey].time_position.Count);
        }
        print("================== Данные из файла по отдельным самолетам разобраны ===================");
    }

    // Координаты сцены из широты и долготы
    Vector3 SceneCoordinates(float longitude, float latitude, float altitude)
    {
        Vector2d worldPosition = Conversions.GeoToWorldPosition(latitude, longitude, myFlightRadar.myCenterMercator, myFlightRadar.myWorldRelativeScale);
        Vector3 ScenePosition = new Vector3();
        ScenePosition.x = (float)worldPosition.x;
        ScenePosition.y = altitude;
        ScenePosition.z = (float)worldPosition.y;
        ScenePosition += myFlightRadar.myPosShift;

        return ScenePosition;
    }

    // Update is called once per frame
    void Update()
    {

        // Клавиша 1 - Построить тоннель для 244210
        if (Input.GetKeyDown("2"))
        {
            BuildTunnel("244210");
        }

    }

    // Построить туннель
    public void BuildTunnel(string myKey)
    {
        // Создать новый туннель
        Transform NewTunnel = Instantiate(_SampleTunnel);
        NewTunnel.name = "Tunnel_" + myKey;
        NewTunnel.gameObject.SetActive(true);
        NewTunnel.parent = _SkyTunnels;

        // Данные одного туннеля
        TunneData OnePlaneTunnel = _SkyTunnel[myKey];

        for (int i = 1; i < OnePlaneTunnel.time_position.Count; i += _FramePaintCount)
        {
            // Создать новые ворота
            Transform NewFrame = Instantiate(_SampleFrame);
            NewFrame.name = "Frame_" + i;
            NewFrame.gameObject.SetActive(true);
            NewFrame.parent = NewTunnel;
            NewFrame.position = OnePlaneTunnel.position[i];
            Vector3 myEu = Vector3.zero;
            myEu.y = OnePlaneTunnel.true_track[i];
            NewFrame.eulerAngles = myEu;
        }
    }
}