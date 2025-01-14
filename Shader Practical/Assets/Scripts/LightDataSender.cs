using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
//using Vector3 = SystemNumerics.Vector3;

[ExecuteInEditMode]
public class LightDataSender : MonoBehaviour
{
    //struct _light
    //{
    //    int _type;
    //    Vector3 _direction;
    //    Material _material;
    //    Color _color;
    //    float _smoothness;
    //    float _specularStrength;
    //    float intensity;
    //    Vector3 attentuation;
    //    float _spotlightCutoff;
    //    int _quantizationCount;
    //}
    [SerializeField]
    Material[] _material;

    [SerializeField]
    public LightSource[] _lights;

    public int _lightCount;

    public UnityEngine.Vector4[] _lightVectorData;
    public float[] _lightFloatData;

    // Start is called before the first frame update
    void Start()
    {
        _lightCount = _lights.Length;
        _lightVectorData = new UnityEngine.Vector4[32];
        _lightFloatData = new float[56];
    }

    // Update is called once per frame
    void Update()
    {
        for (int index = 0; index < _lightCount; index++)
        {
            int VectorIndex = index * 4;
            int FloatIndex = index * 7;
            LightSource light = _lights[index];
            //Vector3 color = new Vector3(light._color.r, light._color.g, light._color.b);
            
            _lightVectorData[index + VectorIndex] = light.gameObject.transform.position;
            _lightVectorData[index + 1 + VectorIndex] = light._direction;
            _lightVectorData[index + 2 + VectorIndex] = light.attentuation;
            _lightVectorData[index + 3 + VectorIndex] = light._color;

            _lightFloatData[index + FloatIndex] = (int) light._type;
            _lightFloatData[index + 1 + FloatIndex] = light._smoothness;
            _lightFloatData[index + 2 + FloatIndex] = light._specularStrength;
            _lightFloatData[index + 3 + FloatIndex] = light.intensity;
            _lightFloatData[index + 4 + FloatIndex] = light._spotlightCutoff;
            _lightFloatData[index + 5 + FloatIndex] = light._spotlightInnerCutoff;
            _lightFloatData[index + 6 + FloatIndex] = light._quantizationCount;
        }

        foreach (Material material in _material)
        {
            material.SetInteger("_lightCount", _lightCount);
            material.SetInteger("_vectorCount", _lightCount * 4);
            material.SetInteger("_floatCount", _lightCount * 6);
            material.SetVectorArray("_lightVectorData", _lightVectorData);
            material.SetFloatArray("_lightFloatData", _lightFloatData);
        }
    }
}
