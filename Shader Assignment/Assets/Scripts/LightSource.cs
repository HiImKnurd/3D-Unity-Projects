using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[ExecuteInEditMode]
public class LightSource : MonoBehaviour
{
    [SerializeField] private int count = 1;
    public enum Type
    {
        DIRECTIONAL = 0,
        POINT = 1,
        SPOT = 2,
    }
    [SerializeField] 
    private Type _type;
    [SerializeField]
    private Vector3 _direction = new Vector3(0, -1, 0);
    [SerializeField]
    private Material _material;
    [SerializeField]
    private Color _color;
    [SerializeField]
    [Range(0f, 1f)]
    private float _smoothness;
    [SerializeField]
    [Range(0f, 1f)]
    private float _specularStrength;
    [SerializeField]
    [Range(0f, 10f)]
    private float intensity;
    [SerializeField]
    private Vector3 attentuation = new Vector3 (1.0f, 0.09f, 0.032f);

    [SerializeField]
    [Range(0f, 360f)]
    private float _spotlightCutoff = 20f;
    [SerializeField]
    [Range(0f, 360f)]
    private float _spotlightInnerCutoff = 10f;

    [SerializeField]
    private int _quantizationCount = 15;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _direction = transform.rotation * new Vector3(0, -1, 0);
        _direction = _direction.normalized;

        SendToShader();
    }
    private void SendToShader()
    {
        _material.SetVector("_lightPosition" + count, transform.position);
        _material.SetVector("_lightDirection" + count, _direction);
        _material.SetVector("_lightColor" + count, _color);
        _material.SetFloat("_smoothness" + count, _smoothness);
        _material.SetFloat("_specularStrength" + count, _specularStrength);
        _material.SetInteger("_lightType" + count, (int)_type);
        _material.SetFloat("_lightIntensity" + count, intensity);
        _material.SetVector("_attenuation" + count, attentuation);
        _material.SetFloat("_spotlightCutoff" + count, _spotlightCutoff);
        _material.SetFloat("_spotlightInnerCutoff" + count, _spotlightInnerCutoff);
        _material.SetInteger("_quantizationCount", (int)_quantizationCount);
    }

    public Vector3 GetDirection()
    {
        return _direction;
    }

    public Material GetMaterial() 
    {
        return _material;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
        Gizmos.DrawRay(transform.position, _direction * 10f);
    }
}
