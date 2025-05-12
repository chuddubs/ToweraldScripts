using UnityEngine;

public class SoyMeteor : MonoBehaviour
{
    public float speed = 20f;
    [SerializeField] private Transform sprite;
    [SerializeField] private AudioClip warningSound;
    private int rotationDirection = 1; 
    private float rotationSpeed = 1;
    private SoyGameController gc;

    void Start()
    {
        gc = SoyGameController.Instance;
        rotationDirection = UnityEngine.Random.value < 0.5f ? 1 : -1;
        rotationSpeed = Random.Range(180f, 360f);
    }

    void FixedUpdate()
    {
        transform.position += Vector3.down * speed * Time.fixedDeltaTime;
    }

    void Update()
    {
        if (gc.IsPaused)
            return;
        float angle = rotationDirection * rotationSpeed * Time.deltaTime;
        sprite.Rotate(0f, 0f, angle);
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
}
