using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    public string name;
    public List<int> attackTransitions = new List<int>();
    public float cancelTime = 0.7f;
    public float damage = 5f;
    public float hitstun;
    public float forwardKnockback, upwardKnockback;
    public bool knockdown;

    public AudioClip attackSFX;
    public AudioClip hitSFX;
}

[System.Serializable]
public class Combo
{
    public string name;
    public Attack[] attacks;
}
public class ComboHandler : MonoBehaviour
{
    public List<Combo> combos = new List<Combo>();
}
