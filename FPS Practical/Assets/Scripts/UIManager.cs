using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text _ammocountText;
    private void OnEnable()
    {
        FPSController.OnAmmoChanged += UpdateAmmoCount;
    }
    private void OnDisable()
    {
        FPSController.OnAmmoChanged -= UpdateAmmoCount;
    }
    public void UpdateAmmoCount(int currentAmmo, int maxAmmo)
    {
       _ammocountText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }
}
