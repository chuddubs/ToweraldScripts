using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoyHelmet : SoyItem
{
    // public Helmet type;
    
    public FollowTransform  followTransform;
    private void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
}

public enum Helmet
{
    None,       //tanks 0 hits (this is not a spawnable helmet)
    Tinfoil,    //tanks 1 hit : Uncommon - Grey
    Serious,    //tanks 3 hits : Most Common - Grey
    HardHat,    //tanks 5 hits : Uncommon - Blue
    Miner,      //tanks 7 hits : Rare - Gold
    PickelHaube //tanks 10 hits : Very Rare - Purple

}
