using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public AudioClip collect;
    public int value = 0;

    [Header("Visual Effects")]
    [SerializeField] private GameObject PickUpVFX;
    [SerializeField] private GameObject x2VFX;

    private Rigidbody2D rb;
    private Collider2D gemCollider;
    private Transform playerTransform;

    private bool magnetActive;
    private bool magnetWasActive;
    private float originalGravityScale = 1f;
    private SoyGameController gc;

    void Awake()
    {
        gc = SoyGameController.Instance;
        rb = transform.parent.GetComponent<Rigidbody2D>();
        gemCollider = transform.parent.GetComponent<Collider2D>();
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (rb != null)
            originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        magnetActive = gc.HasActiveItem(Item.Magnet);

        if (magnetActive && !magnetWasActive)
        {
            rb.gravityScale = 0.3f;
            IgnoreObstacles(true);
            magnetWasActive = true;
        }
        else if (!magnetActive && magnetWasActive)
        {
            rb.gravityScale = originalGravityScale;
            IgnoreObstacles(false);
            magnetWasActive = false;
        }
    }

    void FixedUpdate()
    {
        if (magnetActive && playerTransform != null && rb != null)
        {
            Vector2 toPlayer = playerTransform.position - transform.position;
            float distance = toPlayer.magnitude;

            float pullRadius = 5f;
            if (distance <= pullRadius)
            {
                Vector2 direction = toPlayer.normalized;

                float maxSpeed = 15f;
                Vector2 targetVelocity = direction * maxSpeed;

                float smoothFactor = 0.2f; // 0 = instant, 1 = no change
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, smoothFactor);
            }
        }
    }

    private void IgnoreObstacles(bool ignore)
    {
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");
        foreach (GameObject rock in rocks)
        {
            foreach (var col in rock.GetComponents<Collider2D>())
            {
                if (col != null && gemCollider != null)
                    Physics2D.IgnoreCollision(gemCollider, col, ignore);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;

        bool soyMilk = SoyGameController.Instance.HasActiveItem(Item.SoyMilk);
        int scoreToAdd = soyMilk ? value * 2 : value;
        SoyGameController.Instance.IncreaseScore(scoreToAdd);
        SoyAudioManager.Instance.Play(collect, 0.6f);

        PlayVFX(PickUpVFX);
        if (soyMilk)
            PlayVFX(x2VFX, true);
        Destroy(transform.parent.gameObject);
    }

    private void PlayVFX(GameObject vfx, bool destroy = false)
    {
        if (vfx == null)
            return;
        vfx.transform.SetParent(null);
        vfx.SetActive(true);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();
        if (destroy)
            Destroy(vfx, 2.5f);
    }
}
