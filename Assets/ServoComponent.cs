using Assets;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class ServoComponent : MonoBehaviour
{
    [SerializeField] private EventHub eventHub;

    void OnEnable()
    {
        if (eventHub != null)
        {
            eventHub.OnServoStateReported += HandleServoState;
        }
    }

    void OnDisable()
    {
        if (eventHub != null)
        {
            eventHub.OnServoStateReported -= HandleServoState;
        }
    }
    public void PublishServoCommand(float targetAngle)
    {
        int clampedAngle = Mathf.RoundToInt(Mathf.Clamp(targetAngle, 0f, 180f));
        string json = $"{{\"target_angle\":{clampedAngle}}}";

        // Connect to your M2MqttUnity client publish method:
        // mqttClient.Publish(servoCommandTopic, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);

        //Debug.Log($"[MQTT TX] Topic: {servoCommandTopic} | Payload: {json}");
    }

    private void HandleServoState(float angle)
    {
        Debug.Log("Servo state changed to " + angle);
    }
}