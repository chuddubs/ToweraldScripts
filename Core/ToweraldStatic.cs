using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ToweraldStatic
{
    public static bool  isMobile = false;
    public static bool  isLeftie = false;
    public static float sprokeTimeFactor = 0.1f;

    public static readonly Dictionary<Item, float> timedItemDuration = new()
    {
        { Item.Sproke,        10f },
        { Item.SoyMilk,       20f },
        { Item.Magnet,        30f },
        { Item.Thrembometer,  22.5f },
        { Item.Ouchie,        18f },
        { Item.Kebab,         12f },
        { Item.Nut,           20f },
        { Item.Meds,          15f }
    };

    public static Dictionary<Helmet, float> helmetRarity = new Dictionary<Helmet, float>()
    {
        { Helmet.Tinfoil, 5f },     //Uncommon - Grey (because not valuable)
        { Helmet.Serious, 8f },     //Common - Grey
        { Helmet.HardHat, 6f },     //Uncommon - Green
        { Helmet.Miner, 4f },       //Uncommon - Blue  
        { Helmet.PickelHaube, 3f }  //Rare - Gold
    };

    public static Dictionary<Item, float> itemRarity = new Dictionary<Item, float>()
    {
        { Item.Helmet,         20f },  // Most Common - Grey
        { Item.Sproke,         7f },  // Common - Grey
        { Item.Thrembometer,   6f },  // Common - Grey
        { Item.GreenArrow,     5f },  // Uncommon - Green
        { Item.Magnet,         5f },  // Uncommon - Green
        { Item.Ouchie,         4f },  // Uncommon - Blue
        { Item.SoyMilk,        4f },  // Uncommon - Blue
        { Item.Kebab,          3f },  // Rare - Gold
        { Item.Nut,            3f },  // Rare - Gold
        { Item.Meds,           2f },   // Legendary
    };

    public static Dictionary<Item, float> bossItemRarity = new Dictionary<Item, float>()
    {
        { Item.Helmet,     8f },  // Most common
        { Item.Magnet,      3f },  // Rare
        { Item.Nut,         1f },  // Legendary
    };

    public static Dictionary<int, float> gemRarity = new Dictionary<int, float>()
    {
        { 0, 20f },         //Quartz
        { 1, 17.25f },      //Amber
        { 2, 15f },         //Amethyst
        { 3, 12.25f },      //Topaz
        { 4, 11.5f },       //Tanzanite
        { 5, 9f },          //Sapphire
        { 6, 7f },          //Emerald
        { 7, 5f },          //Ruby
        { 8, 3f }           //Damon
    };

    private static readonly HashSet<Item> timedItems = new()
    {
        Item.Sproke,
        Item.SoyMilk,
        Item.Magnet,
        Item.Thrembometer,
        Item.Ouchie,
        Item.Kebab,
        Item.Nut,
        Item.Meds
    };

    public static bool IsTimedItem(Item item) => timedItems.Contains(item);

    public static float GetDurationForItem(Item item)
    {
        return timedItemDuration.TryGetValue(item, out var duration) ? duration : 0f;
    }


    public static T WeightedRandomChoice<T>(Dictionary<T, float> weights)
    {
        float totalWeight = weights.Values.Sum();
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0;

        foreach (var pair in weights)
        {
            cumulative += pair.Value;
            if (roll < cumulative)
                return pair.Key;
        }

        // Fallbacks
        return weights.Keys.First();
    }

    public static float TriangularDistribution(float minimum, float peak, float maximum)
    {
        if (minimum > maximum)
            (minimum, maximum) = (maximum, minimum);

        // Clamp peak into valid range
        if (peak < minimum || peak > maximum)
            peak = Mathf.Clamp(peak, minimum, maximum);

        // Ensure positive range
        float range = maximum - minimum;
        if (range <= 0f)
            return 0.01f;

        // Triangular distribution
        float v = Random.value;
        float result = (v < (peak - minimum) / range) ?
              minimum + Mathf.Sqrt(v * range * (peak - minimum))
            : maximum - Mathf.Sqrt((1f - v) * range * (maximum - peak));

        // Final clamp to strictly positive
        return Mathf.Max(result, 0.01f);
    }
}
