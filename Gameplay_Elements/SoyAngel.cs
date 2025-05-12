using UnityEngine;

public class SoyAngel : MonoBehaviour
{
    //1: right, -1: left
    public int direction = 1;
    public float speed = 1f;
    private float rotationSpeed = 360f;
    private float radius;
    // [SerializeField] private ParticleSystem deathVfx;
    private SoyGameController gc;

    void Start()
    {
        gc = SoyGameController.Instance;
        radius = GetComponent<CircleCollider2D>().radius;
        rotationSpeed = speed * Mathf.Rad2Deg / radius;
    }

    void FixedUpdate()
    {
        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;
    }

    void Update()
    {
        if (gc.IsPaused)
            return;
        float angle = -direction * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, angle);    
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoyClimber climber = other.GetComponent<SoyClimber>();
            if (climber != null)
                climber.Die();

        }
        else if (other.CompareTag("Rock"))
        {
            Mineral rock = other.GetComponent<Mineral>();
            if (rock != null)
                rock.Explode(false);
        }
    }

    public void Die()
    {
        // deathVfx.transform.SetParent(null);
        // deathVfx.gameObject.SetActive(true);
        // deathVfx.Play();
        Destroy(gameObject);
    } 
}
