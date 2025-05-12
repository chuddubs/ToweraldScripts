using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyFreezer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Rigidbody2D rb;
        if ((rb = collider.GetComponent<Rigidbody2D>()) != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
