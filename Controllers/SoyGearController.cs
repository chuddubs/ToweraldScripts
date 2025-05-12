using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyGearController : MonoBehaviour
{
    private SoyClimber climber;
    public SoyHelmet   helmet;
    public AudioClip   helmetPickupSound;
    public AudioClip[] itemPickupSounds;

    public void SetClimber(SoyClimber _cl, SoyCharacterController _ctrler)
    {
        climber = _cl;
        _ctrler.onSpriteChanged += UpdateAttachPoints;
    }

    public void UpdateAttachPoints(SoyCharacterSprite sprite)
    {
        helmet.followTransform.toFollow = sprite.helmetSpot;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("PickupItem"))
            EquipItem(collider.GetComponent<SoyItem>());
    }

    public void EquipItem(SoyItem item)
    {
        if (climber.charCtrlr.dead)
            return;
        switch (item.itemType)
        {
            case Item.Sproke:
                climber.DrinkSproke();
                break;
            case Item.GreenArrow:
                climber.TakeGreenArrow(item.remainingUses);
                break;
            case Item.SoyMilk:
                climber.DrinkSoyMilk();
                break;
            case Item.Magnet:
                climber.TakeMagnet();
                break;
            case Item.Thrembometer:
                climber.TakeThremboMeter();
                break;
            case Item.Ouchie:
                climber.TakeOuchie();
                break;
            case Item.Kebab:
                climber.EatKebab();
                break;
            case Item.Nut:
                climber.EatNut();
                break;
            case Item.Meds:
                climber.TakeMeds();
                break;
            case Item.Helmet:
                EquipHelmet(item);
                break;
        }
        Destroy(item.gameObject);
    }

    private void EquipHelmet(SoyItem newHelmet)
    {
        //ignore if this helmet is less durable than our current equipped helmet
        if (newHelmet.remainingUses < helmet.remainingUses)
            return;
        // Remove the current helmet model if one exists
        if (helmet.remainingUses > 0 && helmet.transform.childCount > 0)
            Destroy(helmet.transform.GetChild(0).gameObject);
        // Update durability
        helmet.remainingUses = newHelmet.remainingUses;
        SoyUI_ActiveItemsController.Instance.OnPickUpHelmet(helmet.remainingUses);
        // Attach the new helmet model
        Transform newSprite = newHelmet.transform.GetChild(0);
        newSprite.SetParent(helmet.transform, false);
        newSprite.localPosition = Vector3.zero;
        newSprite.localRotation = Quaternion.identity;
        newSprite.localScale = Vector3.one;

        SoyAudioManager.Instance.Play(helmetPickupSound);
    }

}
