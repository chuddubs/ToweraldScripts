using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollision : MonoBehaviour
{
    public SoyClimber climber;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Rock"))
        {
            Mineral rock = collider.GetComponent<Mineral>();
            if (!rock.CanDamage())
                return;
            rock.hitHead = true;
            if (climber.DoesHitKill())
            {
                Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                climber.Die();
            }
        }
    }
}
