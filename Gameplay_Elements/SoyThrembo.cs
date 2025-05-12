using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyThrembo : MonoBehaviour
{
    [SerializeField] private ParticleSystem pickupVfx;
    [SerializeField] private AudioClip pickupSound;

    void Start()
    {
        float maxHeight = Godson.Instance.transform.position.y + 7.5f;
        if (transform.position.y > maxHeight)
        {
            StartCoroutine(MoveDownToY(maxHeight));
        }
    }

    
    private IEnumerator MoveDownToY(float targetY)
    {
        float speed = 2f;
        while (transform.position.y > targetY)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
            transform.position = pos;
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoyClimber climber = other.GetComponent<SoyClimber>();
            if (climber != null)
                OnPickUp();

        }
    }

    private void OnPickUp()
    {
        SoyAudioManager.Instance.Play(pickupSound);
        PlayPickUpVfx();

        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");
        foreach (GameObject rock in rocks)
        {
            Mineral mineral = rock.GetComponent<Mineral>();
            if (mineral != null)
            {
                mineral.Explode(false);
            }
        }

        SoyAngel[] angels = FindObjectsByType<SoyAngel>(FindObjectsSortMode.None);
        foreach (SoyAngel angel in angels)
        {
            angel.Die();
        }

        SoyMeteor[] meteors = FindObjectsByType<SoyMeteor>(FindObjectsSortMode.None);
        foreach (SoyMeteor meteor in meteors)
        {
            Destroy(meteor.gameObject);
        }

        SoyWasp[] wasps = FindObjectsByType<SoyWasp>(FindObjectsSortMode.None);
        foreach (SoyWasp wasp in wasps)
        {
            Destroy(wasp.gameObject);
        }
    }


    private void PlayPickUpVfx()
    {
        pickupVfx.transform.SetParent(null);
        pickupVfx.gameObject.SetActive(true);
        pickupVfx.Play();
        Destroy(gameObject);
        Godson.Instance.OnPickedUpThrembo();
    }
}
