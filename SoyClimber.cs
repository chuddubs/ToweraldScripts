using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SoyCharacterController))]
public class SoyClimber : MonoBehaviour
{
    public SoyGearController        gear;
    public GameObject               lostItemPrefab;
    public SoyCharacterController   charCtrlr;
    [SerializeField]
    private ParticleSystem          magnetVFX;
    [SerializeField]
    private ParticleSystem          immuneVFX;
    private float                   ouchieStatus = 0f;
    public bool                     immune = false;
    private int                     greenArrowAmmo = 0;
    public bool                     squirrelMode = false;

    [SerializeField]private GameObject             greenArrowProjectilePrefab;
    [SerializeField] private Transform              greenArrowProjectileSpawn;

    #region AUDIO_VARIABLES
    [Header("Audioclips")]
    public AudioClip               deathSound;
    public AudioClip               helmetBonk;
    public AudioClip               helmetLose;
    public AudioClip               sprokeDrink;
    public AudioClip               greenArrowTake;
    public AudioClip               greenArrowShoot;
    public AudioClip               soyMilkDrink;
    public AudioClip               magnetTake;
    public AudioClip               thrembometerTake;
    public AudioClip               ouchieTake;
    public AudioClip               kebabEat;
    public AudioClip               nutEat;
    public AudioClip               ouchSquirrel;
    public AudioClip               medsTake;
    #endregion

    private void Awake()
    {
        charCtrlr = GetComponent<SoyCharacterController>();
        gear.SetClimber(this, charCtrlr);
    }

    #region DAMAGE
    public void Die()
    {   
        if (squirrelMode)
            LoseNut();
        else
        {
            SoyAudioManager.Instance.Play(deathSound);
            LoseMagnet();
            if(SoyGameController.Instance.bossBattle)
                Godson.Instance.OnPlayerDie();
            charCtrlr.Die();   
        }
    }

    public bool DoesHitKill()
    {
        if (immune)
        {
            ShowImmuneVFX();
            return false;
        }
        if (gear.helmet.remainingUses == 0)
        {
            if (squirrelMode)
            {
                if(SoyGameController.Instance.bossBattle)
                    Godson.Instance.OnPlayerHit();
                SoyAudioManager.Instance.Play(ouchSquirrel);
                SoyGameController.Instance.LoseNutEarly();
                return false;
            }
            else
                return true;
        }
        SoyAudioManager.Instance.Play(helmetBonk);
        if (gear.helmet.UpdateRemainingUses(-1))
        {
            SoyAudioManager.Instance.Play(helmetLose);
            SoyUI_ActiveItemsController.Instance.OnLoseHelmet();
            LoseHelmet(gear.helmet);
        }
        if(SoyGameController.Instance.bossBattle)
            Godson.Instance.OnPlayerHit();
        return false;
    }

    private void ShowImmuneVFX()
    {
        immuneVFX.Play();
    }
    #endregion

    #region PICKUPS
    private void    LoseHelmet(SoyItem item)
    {
        Transform itemSprite = item.transform.GetChild(0);
        GameObject lostItemHolder = Instantiate(lostItemPrefab, itemSprite.position, itemSprite.rotation);
        itemSprite.SetParent(lostItemHolder.transform, true);
    }

    public void DrinkSproke()
    {
        SoyAudioManager.Instance.Play(sprokeDrink);
        SoyUI_ActiveItemsController.Instance.OnPickUpSproke();
        SoyGameController.Instance.TriggerTimedItem(Item.Sproke);
    }

    public void TakeMagnet()
    {
        SoyAudioManager.Instance.Play(magnetTake);
        SoyUI_ActiveItemsController.Instance.OnPickUpMagnet();
        SoyGameController.Instance.TriggerTimedItem(Item.Magnet);
        if (!magnetVFX.gameObject.activeSelf)
        {
            magnetVFX.gameObject.SetActive(true);
            magnetVFX.Play();
        }
    }

    public void DrinkSoyMilk()
    {
        SoyAudioManager.Instance.Play(soyMilkDrink);
        SoyUI_ActiveItemsController.Instance.OnPickUpSoyMilk();
        SoyGameController.Instance.TriggerTimedItem(Item.SoyMilk);  
    }

    public void TakeGreenArrow(int ammo)
    {
        SoyAudioManager.Instance.Play(greenArrowTake);
        greenArrowAmmo += ammo;
        SoyUI_ActiveItemsController.Instance.OnPickUpGreenArrow(greenArrowAmmo);
    }

    public void TakeThremboMeter()
    {
        SoyAudioManager.Instance.Play(thrembometerTake);
        SoyGameController.Instance.TriggerTimedItem(Item.Thrembometer);
    }

    public void TakeOuchie()
    {
        SoyAudioManager.Instance.Play(ouchieTake);
        ouchieStatus += 1.25f;
        float roll = Random.Range(0f, 100f);
        if (roll <= ouchieStatus)
            Die();
        SoyUI_ActiveItemsController.Instance.OnPickUpOuchie(ouchieStatus);
        SoyGameController.Instance.TriggerTimedItem(Item.Ouchie);
    }

    public void EatKebab()
    {
        SoyAudioManager.Instance.Play(kebabEat);
        SoyUI_ActiveItemsController.Instance.OnPickUpKebab();
        SoyGameController.Instance.TriggerTimedItem(Item.Kebab);  
    }

    public void EatNut()
    {
        SoyAudioManager.Instance.Play(nutEat);
        squirrelMode = true;
        SoyUI_ActiveItemsController.Instance.OnPickUpNut();
        SoyGameController.Instance.TriggerTimedItem(Item.Nut);
        transform.localScale = new Vector3(0.3f * Mathf.Sign(transform.localScale.x), 0.3f, 0.3f);
        charCtrlr.SetSquirrelSpeed(true);
    }

    public void TakeMeds()
    {
        SoyAudioManager.Instance.Play(medsTake);
        SoyUI_ActiveItemsController.Instance.OnPickUpMeds();
        SoyGameController.Instance.TriggerTimedItem(Item.Meds);
    }
    #endregion

    public void OnShootGreenArrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootGreenArrow();
        }
    }

    public void ShootGreenArrow()
    {
        if (charCtrlr.dead || greenArrowAmmo <= 0)
            return;
        SoyAudioManager.Instance.Play(greenArrowShoot);
        greenArrowAmmo -= 1;
        GameObject projectile = Instantiate(
            greenArrowProjectilePrefab,
            greenArrowProjectileSpawn.position,
            Quaternion.identity
        );
        SoyUI_ActiveItemsController.Instance.UpdateGreenArrow(greenArrowAmmo);
    }

    public void LoseMagnet()
    {
        if (magnetVFX.gameObject.activeSelf)
            magnetVFX.gameObject.SetActive(false);
    }

    public void LoseNut()
    {
        charCtrlr.SetSquirrelSpeed(false);
        float scaleX = Mathf.Sign(transform.localScale.x);
        if (scaleX == 0f)
            scaleX = 1f;
        transform.localScale = new Vector3(scaleX, 1f, 1f);
        squirrelMode = false;
    }

}
