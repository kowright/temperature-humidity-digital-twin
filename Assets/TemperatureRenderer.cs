using System;
using TMPro;
using UnityEngine;

public class TemperatureRenderer : MonoBehaviour
{
    [Serializable]
    public struct EnvironmentTelemetry
    {
        public string device;
        public float temperature_f;
        public float humidity;
    }

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
    private EnvironmentTelemetry _latestReading;
    private bool _hasReceivedAnyReading;

    private DateTime _lastReadingTime;
    public void OnMqttPayloadReceived(string jsonPayload)
    {
        try
        {
            EnvironmentTelemetry parsed = JsonUtility.FromJson<EnvironmentTelemetry>(jsonPayload);
            lock (_lock)
            {
                _latestReading = parsed;
                _hasNewReading = true;
                _hasReceivedAnyReading = true;
                _lastReadingTime = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse MQTT telemetry: {ex.Message}");
        }
    }

    private void Update()
    {
        EnvironmentTelemetry current;
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

    private void ApplyVisuals(EnvironmentTelemetry reading)
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

        Debug.Log($"Room Twin Updated -> Temp: {reading.temperature_f:F1}°F | Humidity: {reading.humidity:F1}%");
    }

    private void ApplyReadouts(EnvironmentTelemetry current, bool isStale)
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
