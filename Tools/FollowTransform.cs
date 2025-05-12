using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform toFollow;
    public bool x = false;
    public float xOffset = 0f;
    public bool y = false;
    public float yOffset = 0f;
    public bool z = false;
    public float zOffset = 0f;

    Vector3 pos;
    void Update()
    {
        pos = transform.position;
        if (x)
            pos.x = toFollow.position.x + xOffset;
        if (y)
            pos.y = toFollow.position.y + yOffset;
        if (z)
            pos.z = toFollow.position.z + zOffset;
        transform.position = pos;
    }
}
