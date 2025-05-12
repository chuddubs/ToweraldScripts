using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostItem : MonoBehaviour
{
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(0f, 0.2f));
        ApplyRandomSpin(rb);
    }

    private void ApplyRandomSpin(Rigidbody2D rigid)
    {
        float randomTorque = Random.Range(120f, 200f);
        float randomDirection = Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
        rigid.AddTorque(randomTorque * randomDirection);
    }
}
