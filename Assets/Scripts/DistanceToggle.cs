using System.Collections.Generic;

using UnityEngine;

public class DistanceToggle : MonoBehaviour
{
    public Transform Tracking;
    public float ToggleDistance = 20f;
    public List<Behaviour> ToggledBehaviours;
    public List<GameObject> ToggledObjects;
    public List<Renderer> ToggledRenderers;
    public float CheckFrequency = 0.5f;


    [Header("Debug")]
    public bool currentState = false;
    public float timeSinceCheck = 0f;
    public float DistanceToTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckDistance();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceCheck += Time.deltaTime;

        if (timeSinceCheck < CheckFrequency)
        {
            return;
        }

        timeSinceCheck = 0f;

        CheckDistance();
    }

    void CheckDistance()
    {
        DistanceToTarget = (transform.position - Tracking.position).magnitude;
        var withinThreshold = DistanceToTarget <= ToggleDistance;
        if (withinThreshold != currentState)
        {
            currentState = withinThreshold;
            foreach (var obj in ToggledBehaviours)
            {
                obj.enabled = currentState;
            }
            foreach (var obj in ToggledObjects)
            {
                obj.SetActive(currentState);
            }
            foreach (var obj in ToggledRenderers)
            {
                obj.enabled = currentState;
            }
        }
    }
}
