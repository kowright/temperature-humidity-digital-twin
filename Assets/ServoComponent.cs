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
    [SerializeField] private Animator animator;

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
        if (servoText != null)
        {
            servoText.text = "Servo Degree: " + angle;
            if (animator != null)
            {
                if (angle == 0)
                {
                    PlayForward();
                }
                else
                {
                    PlayReverse();
                }
            }
           
        }
    }

    public void PlayForward()
    {
        animator.Play(Animator.StringToHash("ServoAnimator"), 0, 0f);
    }

    public void PlayReverse()
    {
        animator.Play(Animator.StringToHash("ServoAnimatorReverse"), 0, 0f);
    }
}