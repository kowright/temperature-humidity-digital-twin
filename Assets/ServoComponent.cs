using Assets;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class ServoComponent : MonoBehaviour
{
    [SerializeField] private EventHub eventHub;
    [SerializeField] private TextMeshProUGUI servoText;

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

    private void HandleServoState(float angle)
    {
        Debug.Log("Servo state changed to " + angle);
        if (servoText != null)
        {
            servoText.text = "Servo Degree: " + angle;
        }
    }
}