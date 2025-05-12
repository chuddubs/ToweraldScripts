using UnityEngine;

public class SoyStomper : MonoBehaviour
{
    [SerializeField] private SoyCharacterController ctrlr;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Buggie"))
        {
            if (ctrlr.rb.linearVelocityY >= 0)
                return;
            SoyBuggie buggie = collider.GetComponent<SoyBuggie>();
            ctrlr.rb.linearVelocityY = 6f;
            buggie.OnCrushed(true);
        }
    }
}
