using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class LookRandom : MonoBehaviour
{
    [SerializeField] private Transform aimTargetTransform;
    private Vector3 targetPosition;
    private Vector3 origin;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = origin =
        aimTargetTransform.localPosition;
        StartCoroutine(ChangeTargetPosition());
    }
    // Update is called once per frame
    void Update()
    {
        if (aimTargetTransform.localPosition !=
        targetPosition)
        {
            float speed = 2;
            //gradually move the transform to the target position
        aimTargetTransform.localPosition =
        Vector3.Lerp(aimTargetTransform.localPosition, targetPosition,
        Time.deltaTime * speed);
        }
    }
    IEnumerator ChangeTargetPosition()
    {
        //Wait an amount of time to wait before changing the position
        yield return new WaitForSeconds(3);
        float x = Random.Range(-3, 3);
        float y = Random.Range(-1, 1);
        float z = Random.Range(2, 5);
        //Update the target position, by offsetting the origin point
        targetPosition = origin + new Vector3(x, y, z);
        //Loop
        StartCoroutine(ChangeTargetPosition());
    }
}
