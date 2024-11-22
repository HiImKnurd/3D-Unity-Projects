using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponItem : MonoBehaviour, Item
{
    private Weapon weaponScript;

    private void Start()
    {
        weaponScript = GetComponent<Weapon>();
    }
    public void Use(FPSController player)
    {
        transform.parent = player._weaponHolder.transform;
        transform.localPosition = weaponScript.hipPosition;
        transform.localRotation = Quaternion.identity;
        player.weapons.Add(weaponScript);
        this.gameObject.layer = 7; // set to weapon layer
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (var child in children) {
            child.gameObject.layer = 7;
        }
        this.gameObject.SetActive(false);
    }
}
