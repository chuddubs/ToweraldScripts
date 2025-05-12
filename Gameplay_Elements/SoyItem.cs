using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyItem : MonoBehaviour
{
    public Item             itemType;
    public int              remainingUses = 1;

    public bool UpdateRemainingUses(int by)
    {
        remainingUses += by;
        SoyUI_ActiveItemsController.Instance.UpdateHelmetDurability(remainingUses);
        return remainingUses <= 0;
    }
}

public enum Item
{
    Sproke,
    GreenArrow,
    Thrembometer,
    Ouchie,
    Magnet,
    SoyMilk,
    Kebab,
    Nut,
    Meds,
    Helmet,
}
