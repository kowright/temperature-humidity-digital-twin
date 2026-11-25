using Assets;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TemperatureRenderer : MonoBehaviour
{

    [Header("Target Scene Visuals")]
    [SerializeField] private Renderer tempRenderer;
    [SerializeField] private Renderer humidityRenderer;
    [SerializeField] TextMeshProUGUI temperatureReadout;
    [SerializeField] TextMeshProUGUI humidityReadout;
    [SerializeField] TextMeshProUGUI deviceLocationReadout;
    [SerializeField] private Color coolColor = new Color(0.20f, 0.55f, 0.95f);
    [SerializeField] private Color warmColor = new Color(0.95f, 0.30f, 0.20f);

    [Header("Temperature Range (°F)")]
    [SerializeField] private float minTempF = 65f;
    [SerializeField] private float maxTempF = 85f;

    [Header("Temperature Range (°F)")]
    [SerializeField] private float minHumidity = 30f;
    [SerializeField] private float maxHumidity = 40f;

    [Header("Staleness Settings")]
    [SerializeField] private float staleAfterSeconds = 5f;

    [Header("Location Settings")]
    [SerializeField] private string readingDeviceRoom = "room";

    private readonly object _lock = new object();
    private bool _hasNewReading;
    private EnvironmentPayload _latestReading;
    private bool _hasReceivedAnyReading;

    private DateTime _lastReadingTime;

    [SerializeField] private EventHub eventHub;

    void OnEnable()
    {
        if (eventHub != null)
        {
            eventHub.OnEnvironmentDataReceived += HandleEnvironmentData;
        }
    }

    void OnDisable()
    {
        if (eventHub != null)
        {
            eventHub.OnEnvironmentDataReceived += HandleEnvironmentData; 
        }
    }

    private void HandleEnvironmentData(EnvironmentPayload payload, bool hasNewReading, bool hasAnyReading, DateTime lastReadingTime) 
    {
        try
        {
            lock (_lock)
            {
                _latestReading = payload;
                _hasNewReading = hasNewReading;
                _hasReceivedAnyReading = hasAnyReading;
                _lastReadingTime = lastReadingTime;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse MQTT telemetry: {e.Message}");
        }
    }

    private void Update()
    {
        EnvironmentPayload current;
        bool hasNew;
        bool hasAny;
        DateTime lastTime;

        lock (_lock)
        {
            hasNew = _hasNewReading;
            hasAny = _hasReceivedAnyReading;
            current = _latestReading;
            lastTime = _lastReadingTime;
            _hasNewReading = false;
        }

        if (hasNew)
        {
            ApplyVisuals(current);
        }

        if (hasAny)
        {
            double elapsedSeconds = (DateTime.Now - lastTime).TotalSeconds;
            bool isStale = elapsedSeconds > staleAfterSeconds;
            ApplyReadouts(current, isStale);
        }
    }

    private void ApplyVisuals(EnvironmentPayload reading)
    {
        float t = Mathf.InverseLerp(minTempF, maxTempF, reading.temperature_f);
        float h = Mathf.InverseLerp(minHumidity, maxHumidity, reading.humidity);
        if (tempRenderer != null)
        {
            tempRenderer.material.color = Color.Lerp(coolColor, warmColor, t);
        }

        if(humidityRenderer != null)
        {
            humidityRenderer.material.color = Color.Lerp(coolColor, warmColor, h);
        }

    }

    private void ApplyReadouts(EnvironmentPayload current, bool isStale)
    {
        if (temperatureReadout != null)
        {
            temperatureReadout.text = isStale
                ? "Stale/Disconnected"
                : $"Temperature \n{current.temperature_f:F1} F";
        }

        if (humidityReadout != null)
        {
            humidityReadout.text = isStale
                ? "Stale/Disconnected"
                : $"Humidity \n{current.humidity:F1}%";
        }
        if(deviceLocationReadout != null)
        {
            deviceLocationReadout.text = isStale ?
                "Cannot read device in " + readingDeviceRoom :
                $"Reading from {current.device} in {readingDeviceRoom}";
        }
    }
}
