using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyDeader : MonoBehaviour
{
    public SoyClimber climber;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
            climber.Die();
    }
}
