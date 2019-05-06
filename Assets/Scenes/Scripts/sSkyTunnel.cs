using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sSkyTunnel : MonoBehaviour
{
    // Основной объект. Нужен здесь, чтобы узнать, получаем мы данные из Сети или из файла. Если из сети - ничего не делать.
    sFlightRadar _FlightRadar;

    // Объект для получения исходных данных
    sWebData _WebData;

    // Start is called before the first frame update
    void Start()
    {
        // Основной объект. Нужен здесь, чтобы узнать, получаем мы данные из Сети или из файла
        _FlightRadar = transform.GetComponent<sFlightRadar>();

        if (!_FlightRadar.DataFromWeb) // Если из сети - ничего не делать.
        {
            // Объект для получения исходных данных
            _WebData = transform.GetComponent<sWebData>();

            // Разбор полученных данных по отдельным самолетам

            //StartCoroutine(myFuncSeparateFlightData());

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
