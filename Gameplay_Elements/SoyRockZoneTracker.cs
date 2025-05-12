using System.Collections.Generic;
using UnityEngine;

public class SoyRockZoneTracker : MonoBehaviour
{
    private HashSet<Mineral> insideRocks = new HashSet<Mineral>();

    public int LandedRockCount
    {
        get
        {
            int count = 0;
            foreach (Mineral m in insideRocks)
            {
                if (m != null && m.landed)
                    count++;
            }
            return count;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Rock"))
            return;
        Mineral m = other.GetComponent<Mineral>();
        if (m != null)
        {
            insideRocks.Add(m);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Rock"))
            return;
        Mineral m = other.GetComponent<Mineral>();
        if (m != null)
        {
            insideRocks.Remove(m);
        }
    }

    private void Update()
    {
        insideRocks.RemoveWhere(m => m == null);
    }
}
