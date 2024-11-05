using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItem : MonoBehaviour, Item
{
    public void Use(FPSController player)
    {
        player._currentWeapon.currentAmmo += 5;
        if (player._currentWeapon.currentAmmo > player._currentWeapon.maxAmmo)
        {
            player._currentWeapon.currentAmmo = player._currentWeapon.maxAmmo;
        }
        player.InvokeAmmoChanged();
        //Destroy(this);
    }
}
