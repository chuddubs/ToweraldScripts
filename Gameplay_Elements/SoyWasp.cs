using UnityEngine;

public class SoyWasp : MonoBehaviour
{
    public float speed = 20f;
    [SerializeField] private ParticleSystem deathVfx;
    private bool init = false;
    public bool leftToRight = true;
    private Vector2 endPosition = Vector2.zero;

    public void Init(bool _leftToRight)
    {
        leftToRight = _leftToRight;
        if (leftToRight)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        endPosition.x = transform.position.x + (leftToRight ? Random.Range(8f, 10f) : -Random.Range(8f, 10f));
        endPosition.y = transform.position.y - Random.Range(5f, 10f);
        init = true;
    }


    void FixedUpdate()
    {
        if (!init)
            return;
        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, endPosition, speed * Time.fixedDeltaTime);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        if (Vector2.Distance(newPos, endPosition) < 0.05f)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SoyClimber climber = collision.collider.GetComponent<SoyClimber>();
            if (climber != null)
                climber.Die();
        }
    }

    public void Die()
    {
        SoyAudioManager.Instance.PlaySplat();
        deathVfx.transform.SetParent(null);
        deathVfx.gameObject.SetActive(true);
        deathVfx.Play();
        Godson.Instance.OnWaspDied(transform.position);
        Destroy(gameObject);
    }
}
