using UnityEngine;

public class SoyBuggie : MonoBehaviour
{
    [SerializeField] private ParticleSystem splat;
    public GameObject gemToSpawn;
    private string rockTag = "Rock";
    private float crushNormalThreshold = -0.5f;


    public void OnCrushed(bool byPlayer = false)
    {
        if (byPlayer)
            ObjectSpawner.Instance.SpawnGemWithParams(transform.position, gemToSpawn, 0.2f);
        splat.transform.SetParent(null);
        splat.gameObject.SetActive(true);
        splat.Play();
        SoyAudioManager.Instance.PlaySplat();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(rockTag))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < crushNormalThreshold)
            {
                OnCrushed();
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
                break;
            }
        }
    }
}