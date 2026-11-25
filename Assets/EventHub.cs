using System;
using System.Collections.Generic;
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
    public Action<float> OnPublishServoCommand;

    private readonly object _lock = new object();
    private readonly Queue<ButtonPayload> _buttonQueue = new Queue<ButtonPayload>();
    private readonly Queue<float> _servoStateQueue = new Queue<float>();

    private bool _hasNewEnvReading;
    private bool _hasReceivedAnyEnvReading;
    private EnvironmentPayload _latestEnvReading;
    private DateTime _lastEnvReadingTime;

    public void Setup(Action<float> publishToServo)
    {
        OnPublishServoCommand = publishToServo;
    }

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
            OnEnvironmentDataReceived?.Invoke(envCurrent, hasNewEnv, hasAnyEnv, lastEnvTime);
        }

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
    public void RequestServoMove(float targetAngle)
    {
        OnPublishServoCommand?.Invoke(targetAngle);
    }
}
