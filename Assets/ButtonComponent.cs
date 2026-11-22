using Assets;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class ButtonComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject topOfButton;
    [SerializeField] private string side;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float duration = 0.5f;
    private readonly object _lock = new object();
    private Renderer objRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    [SerializeField] private EventHub eventHub;

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
        // Cache the starting color
        originalColor = objRenderer.material.color;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleButtonPressed(ButtonPayload payload)
    {
        Debug.Log("button pressed");
        try
        {
            //EnvironmentTelemetry parsed = JsonUtility.FromJson<EnvironmentTelemetry>(payload);

            Debug.Log("physical button pressed " + payload);
            //lock (_lock)
            //{

            //}

            if(payload.button == side)
            {
                TriggerButtonVisualFlash();
            }
           
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse MQTT telemetry: {ex.Message}");
        }
    }
    public void On2DButtonPressed()
    {
        //try
        //{
        //    EnvironmentTelemetry parsed = JsonUtility.FromJson<EnvironmentTelemetry>(jsonPayload);
        //    Debug.Log("parsed" + parsed);
        //    //lock (_lock)
        //    //{

        //    //}
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogWarning($"Failed to parse MQTT telemetry: {ex.Message}");

        //}

        TriggerButtonVisualFlash();

        Debug.Log(side + " button pressed");
    }

    public void TriggerButtonVisualFlash()
    {
        // Stop an existing flash if triggered again rapidly so colors don't get stuck
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
