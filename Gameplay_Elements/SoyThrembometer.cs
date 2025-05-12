using System.Numerics;
using UnityEngine;

public class SoyThrembometer : MonoBehaviour
{
    [SerializeField] private ObjectSpawner spawner;
    private float nextRockSpawn;

    void Update()
    {
        if (spawner == null)
            return;
        nextRockSpawn = spawner.NextRockHorizontalPos;
        transform.position = new UnityEngine.Vector3(nextRockSpawn, transform.position.y, transform.position.z);
    }
}
