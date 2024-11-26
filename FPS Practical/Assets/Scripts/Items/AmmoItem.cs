using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItem : MonoBehaviour, Item
{
    public void Use(FPSController player)
    {
        player._currentWeapon.RefillAmmo();
        player.InvokeAmmoChanged();
    }
}
