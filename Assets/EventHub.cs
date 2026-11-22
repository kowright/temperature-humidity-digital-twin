using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Assets;

[Serializable]
public struct EnvironmentPayload
{
    public string device;
    public float temperature_f;
    public float humidity;
}

[Serializable]
public struct ButtonPayload
{
    public string device;
    public string button; // left or right
    public string action; // pressed or hold 
}

[Serializable]
public struct ServoStatePayload
{
    public string device;
    public float angle;
    public string source;
}


public class EventHub : MonoBehaviour
{
    public event Action<ButtonPayload> OnButtonPressed;
    public event Action<float> OnServoStateReported;
    public event Action<EnvironmentPayload, bool, bool, DateTime> OnEnvironmentDataReceived;

    private readonly object _lock = new object();
    private readonly Queue<ButtonPayload> _buttonQueue = new Queue<ButtonPayload>();
    private readonly Queue<float> _servoStateQueue = new Queue<float>();

    private bool _hasNewEnvReading;
    private bool _hasReceivedAnyEnvReading;
    private EnvironmentPayload _latestEnvReading;
    private DateTime _lastEnvReadingTime;


    public void HandleIncomingMqttMessage(string topic, string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return;

        try
        { 
            if (topic == MQTTClientSubscriber.environmentTopic)
            {
                EnvironmentPayload env = JsonUtility.FromJson<EnvironmentPayload>(payload);
                lock (_lock)
                {
                    _latestEnvReading = env;
                    _hasNewEnvReading = true;
                    _hasReceivedAnyEnvReading = true;
                    _lastEnvReadingTime = DateTime.Now;

                }
            }
            else if (topic == MQTTClientSubscriber.buttonInputTopic)
            {
                Debug.Log($"Raw button payload: [{payload}]");
                ButtonPayload btn = JsonUtility.FromJson<ButtonPayload>(payload);

                
                lock (_lock)
                {
                    _buttonQueue.Enqueue(btn);
                }
            }
            else if (topic == MQTTClientSubscriber.servoStateTopic)
            {
                ServoStatePayload state = JsonUtility.FromJson<ServoStatePayload>(payload);
                lock (_lock)
                {
                    _servoStateQueue.Enqueue(state.angle);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse MQTT message on '{topic}': {ex.Message}");

        }
    }

    private void Update()
    {
        EnvironmentPayload envCurrent;
        bool hasNewEnv;
        bool hasAnyEnv;
        DateTime lastEnvTime;
        List<ButtonPayload> buttonsToDispatch = null;
        List<float> servoAnglesToDispatch = null;

        lock (_lock)
        {
            hasNewEnv = _hasNewEnvReading;
            hasAnyEnv = _hasReceivedAnyEnvReading;
            envCurrent = _latestEnvReading;
            lastEnvTime = _lastEnvReadingTime;
            _hasNewEnvReading = false;

            if (_buttonQueue.Count > 0)
            {
                buttonsToDispatch = new List<ButtonPayload>(_buttonQueue.Count);
                while (_buttonQueue.Count > 0)
                {
                    buttonsToDispatch.Add(_buttonQueue.Dequeue());
                }
            }

            if (_servoStateQueue.Count > 0)
            {
                servoAnglesToDispatch = new List<float>(_servoStateQueue.Count);
                while (_servoStateQueue.Count > 0)
                {
                    servoAnglesToDispatch.Add(_servoStateQueue.Dequeue());
                }
            }
        }

        if (hasNewEnv)
        {
            //ApplyEnvironmentVisuals(envCurrent);
            OnEnvironmentDataReceived?.Invoke(envCurrent, hasNewEnv, hasAnyEnv, lastEnvTime);
        }

        //if (hasAnyEnv)
        //{
        //    bool isStale = (DateTime.Now - lastEnvTime).TotalSeconds > staleAfterSeconds;
        //    UpdateEnvironmentReadouts(envCurrent, isStale);
        //}

        if (buttonsToDispatch != null)
        {
            foreach (ButtonPayload btn in buttonsToDispatch)
            {
                OnButtonPressed?.Invoke(btn);
            }
        }

        if (servoAnglesToDispatch != null)
        {
            foreach (float angle in servoAnglesToDispatch)
            {
                OnServoStateReported?.Invoke(angle);
            }
        }
    }

    //private void ApplyEnvironmentVisuals(EnvironmentTelemetryPayload reading)
    //{
    //    float t = Mathf.InverseLerp(minTempF, maxTempF, reading.temperature_f);
    //    float h = Mathf.InverseLerp(minHumidity, maxHumidity, reading.humidity);

    //    if (tempRenderer != null)
    //    {
    //        tempRenderer.material.color = Color.Lerp(coolColor, warmColor, t);
    //    }

    //    if (humidityRenderer != null)
    //    {
    //        humidityRenderer.material.color = Color.Lerp(coolColor, warmColor, h);
    //    }
    //}

    //private void UpdateEnvironmentReadouts(EnvironmentTelemetryPayload reading, bool isStale)
    //{
    //    if (temperatureReadout != null)
    //    {
    //        temperatureReadout.text = isStale
    //            ? "Stale/Disconnected"
    //            : $"{reading.temperature_f:F1} F";
    //    }

    //    if (humidityReadout != null)
    //    {
    //        humidityReadout.text = isStale
    //            ? "Stale/Disconnected"
    //            : $"{reading.humidity:F1}%";
    //    }
    //}

    // Call from a virtual UI button, slider, or 3D control to actuate the physical SG90
    public void PublishServoCommand(float targetAngle)
    {
        int clampedAngle = Mathf.RoundToInt(Mathf.Clamp(targetAngle, 0f, 180f));
        string json = $"{{\"target_angle\":{clampedAngle}}}";

        // Connect to your M2MqttUnity client publish method:
        // mqttClient.Publish(servoCommandTopic, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log($"[MQTT TX] Topic: {MQTTClientSubscriber.servoCommandTopic} | Payload: {json}");
    }
}
