using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAttackTrigger : MonoBehaviour
{
    public enum TriggerType { Start, Finish }
    public TriggerType triggerType;
    public TimeAttackManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerType == TriggerType.Start)
        {
            Debug.Log("Start Now");
            manager.StartTimer();
        }
        else if (triggerType == TriggerType.Finish)
        {
            manager.StopTimer();
            Debug.Log("Finish! Final Time: " + manager.GetFinalTime());
        }
    }
}
