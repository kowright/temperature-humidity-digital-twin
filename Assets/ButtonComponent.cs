using Assets;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class ButtonComponent : MonoBehaviour
{
    [SerializeField] GameObject topOfButton;
    [SerializeField] private string side;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float duration = 0.5f;
    private Renderer objRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    [SerializeField] private EventHub eventHub;
    [SerializeField] private float degrees;

    void OnEnable()
    {
        if (eventHub != null)
        {
            eventHub.OnButtonPressed += HandleButtonPressed;
        }
    }

    void OnDisable()
    {
        if (eventHub != null)
        {
            eventHub.OnButtonPressed -= HandleButtonPressed;
        }
    }

    void Awake()
    {
        objRenderer = topOfButton.GetComponent<Renderer>();
        originalColor = objRenderer.material.color;
    }

    private void HandleButtonPressed(ButtonPayload payload)
    {
        if (string.IsNullOrEmpty(payload.button) || payload.button != side)
        {
            return;
        }
 
        eventHub.RequestServoMove(degrees);

        if(payload.button == side)
        {
            TriggerButtonVisualFlash();
        }
    }
    public void On2DButtonPressed()
    {
        TriggerButtonVisualFlash();
    }

    public void TriggerButtonVisualFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        objRenderer.material.color = flashColor;
        yield return new WaitForSeconds(duration);
        objRenderer.material.color = originalColor;
        flashCoroutine = null;
    }
}
