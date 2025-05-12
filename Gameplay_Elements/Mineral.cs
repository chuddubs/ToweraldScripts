using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mineral : MonoBehaviour
{
    [SerializeField]
    private bool      isGem;
    public bool      landed = false;
    public bool       hitHead = false;
    private bool      slowed = false;
    private float     baseGravity;
    private           Rigidbody2D rb;
    private Transform playerTransform;
    private Collider2D col;
    private bool magnetActive;
    private bool magnetWasActive;
    [SerializeField] private ParticleSystem explosionVFX;
    [SerializeField] private GameObject debrisPrefab;
    private SoyGameController gameController;
    private SoyTimeController timeController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravity = rb.gravityScale;
        gameController = SoyGameController.Instance;
        timeController = SoyTimeController.Instance;
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!landed)
        {
            if (collision.collider.CompareTag("Mineral") || collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (collision.collider.CompareTag("Mineral"))
                {
                    if (collision.collider.GetComponent<Mineral>().landed == false)
                        return;
                }
                landed = true;
                SoyAudioManager.Instance.PlayMineralLanding(isGem); 
            }
        }
    }

    public void Explode(bool spawnGem = true)
    {
        int debrisCount = Random.Range(3, 6);
        Material rockMat = GetComponent<MeshRenderer>().material;
        Material rockMatDark = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = Instantiate(debrisPrefab,transform.position,Quaternion.identity);
            float radius = Random.Range(0.1f, 0.3f);
            debris.GetComponent<PolygonMeshGenerator>().Generate(Random.Range(3, 6), radius, Random.Range(0.01f, 0.55f), false);
            debris.GetComponent<MeshRenderer>().material = rockMat;
            debris.transform.GetChild(0).GetComponent<MeshRenderer>().material = rockMatDark;
            Rigidbody2D rb = debris.GetComponent<Rigidbody2D>();
            Vector2 force = Random.insideUnitCircle.normalized * Random.Range(25f, 80f);
            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-200f, 200f));
            Destroy(debris, 2f);
        }
        PlayExplosionVFX();
        SoyAudioManager.Instance.PlayRockBreak();
        if (spawnGem)
            SpawnGem();
        Destroy(gameObject);
    }

    private void PlayExplosionVFX()
    {
        explosionVFX.transform.SetParent(null);
        explosionVFX.gameObject.SetActive(true);
        explosionVFX.Play();
    }

    private void SpawnGem()
    {
        ObjectSpawner.Instance.SpawnGemWithParams(transform.position);
    }

    void FixedUpdate()
    {
        if (isGem || landed)
            return;

        magnetActive = gameController.HasActiveItem(Item.Magnet);
        float multiplier = timeController.CurrentMultiplier;

        //Gravity and spin scaling
        rb.gravityScale = baseGravity * multiplier;
        rb.angularVelocity *= multiplier;

        if (multiplier < 1f && !slowed)
        {
            rb.linearVelocity *= multiplier;
            slowed = true;
        }
        else if (multiplier == 1f && slowed)
        {
            slowed = false;
        }

        if (magnetActive && !magnetWasActive)
            magnetWasActive = true;
        else if (!magnetActive && magnetWasActive)
            magnetWasActive = false;

        // Magnet repulsion logic
        if (magnetActive && playerTransform != null)
        {
            Vector2 toPlayer = playerTransform.position - transform.position;
            float distance = toPlayer.magnitude;

            float repelRadius = 2.5f;
            if (distance <= repelRadius)
            {
                Vector2 direction = toPlayer.normalized;

                float maxSpeed = 8f;
                Vector2 targetVelocity = -direction * maxSpeed;

                float smoothFactor = 0.5f;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, smoothFactor);
            }
        }
    }


    public bool CanDamage()
    {
        return !hitHead && !landed;
    }

}
