using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ShellCreater : MonoBehaviour
{
    [SerializeField] Shader _shellShader;
    [SerializeField] Mesh _shellMesh;

    [SerializeField]
    [Range(1, 128)]
    int _shellCount = 8;

    [SerializeField]
    [Range(0f, 1f)]
    float _shellHeight = 0.2f;

    [SerializeField]
    [Range(1, 1000)]
    int _shellDensity = 100;

    [SerializeField]
    [Range(0f, 10f)]
    float _thickness = 1f;

    public Color _color = Color.green;

    Material _shellMaterial;
    public MaterialPropertyBlock[] _shellPropertyBlock;
    public GameObject[] shells;

    void OnEnable()
    {
        _shellMaterial = new Material(_shellShader);
        shells = new GameObject[_shellCount];
        _shellPropertyBlock = new MaterialPropertyBlock[_shellCount];

        for (int i = 0; i < _shellCount; i++)
        {
            _shellPropertyBlock[i] = new MaterialPropertyBlock();
            shells[i] = new GameObject("SHELL" + i.ToString());
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();
            shells[i].GetComponent<MeshRenderer>().material = _shellMaterial;
            shells[i].GetComponent<MeshFilter>().mesh = _shellMesh;
            shells[i].transform.SetParent(this.transform, false);

            _shellPropertyBlock[i].SetInteger("_shellCount", _shellCount);
            _shellPropertyBlock[i].SetInteger("_shellIndex", i);
            _shellPropertyBlock[i].SetFloat("_shellLength", _shellHeight);
            _shellPropertyBlock[i].SetInteger("_density", _shellDensity);
            _shellPropertyBlock[i].SetFloat("_thickness", _thickness);
            _shellPropertyBlock[i].SetVector("_shellColor", _color);
            shells[i].GetComponent<MeshRenderer>().SetPropertyBlock(_shellPropertyBlock[i]);

        }
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _shellCount; ++i)
        {
            shells[i].GetComponent<MeshRenderer>().GetPropertyBlock(_shellPropertyBlock[i]);
            _shellPropertyBlock[i].SetInteger("_shellCount", _shellCount);
            _shellPropertyBlock[i].SetInteger("_shellIndex", i);
            _shellPropertyBlock[i].SetFloat("_shellLength", _shellHeight);
            _shellPropertyBlock[i].SetInteger("_density", _shellDensity);
            _shellPropertyBlock[i].SetFloat("_thickness", _thickness);
            _shellPropertyBlock[i].SetVector("_shellColor", _color);

            shells[i].GetComponent<MeshRenderer>().SetPropertyBlock(_shellPropertyBlock[i]);
        }
    }
    void OnDisable()
    {
        for (int i = 0; i < shells.Length; ++i)
        {
            Destroy(shells[i]);
        }

        shells = null;
    }
}
