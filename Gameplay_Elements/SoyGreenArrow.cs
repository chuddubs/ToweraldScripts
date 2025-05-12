using UnityEngine;

public class SoyGreenArrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private SoyGameController gc;

    void Awake()
    {
        gc = SoyGameController.Instance;
        Destroy(this.gameObject, 5f);   
    }

    void Update()
    {
        if (gc.IsPaused)
            return;
        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Rock"))
        {
            Mineral mineral = collider.GetComponent<Mineral>();
            mineral.Explode(!SoyGameController.Instance.bossBattle);
        }
        else if (collider.CompareTag("Wasp"))
        {
            SoyWasp wasp = collider.GetComponent<SoyWasp>();
                wasp.Die();
        }
    }
}
