using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShadowMapRenderer : MonoBehaviour
{

    [SerializeField]
    private LightSource _lightSource;

    [SerializeField]
    private int _shadowMapResolution = 1024;

    [SerializeField]
    private float _shadowBias = 0.005f;

    private Camera _lightCamera;
    private RenderTexture _shadowMap;


    // Start is called before the first frame update
    void Start()
    {
        _lightSource = GetComponent<LightSource>();

        if(_lightSource == null)
        {
            Debug.Log("Shadowmapper requires a light source");
            return;
        }

        CreateLightCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (_lightCamera == null || _shadowMap == null)
            return;

        UpdateLightCamera();
        SendShadowDataToShader();
    }

    private void CreateLightCamera()
    {
        // Creating the shadow map render texture

        _shadowMap = new RenderTexture(_shadowMapResolution,
                                        _shadowMapResolution,
                                        24,
                                        RenderTextureFormat.Depth);
        _shadowMap.Create();

        GameObject lightCameraObject = new GameObject("Light Camera");
        _lightCamera = lightCameraObject.AddComponent<Camera>();
        _lightCamera.enabled = false;
        _lightCamera.clearFlags = CameraClearFlags.Depth;
        _lightCamera.backgroundColor = Color.white;
        _lightCamera.targetTexture = _shadowMap;

        _lightCamera.nearClipPlane = 0.1f;
        _lightCamera.farClipPlane = 10f;
        _lightCamera.orthographic = true;
        _lightCamera.orthographicSize = 10;

        lightCameraObject.transform.SetParent(_lightSource.transform, false);
    }

    private void UpdateLightCamera()
    {
        _lightCamera.transform.position = _lightSource.transform.position;
        _lightCamera.transform.forward = _lightSource.GetDirection();

        _lightCamera.Render();
    }

    private void SendShadowDataToShader()
    {
        Material material = _lightSource.GetMaterial();
        if (material == null)
            return;

        // Calculate light's view-porjection matrix
        Matrix4x4 lightViewProjMatrix = _lightCamera.projectionMatrix * _lightCamera.worldToCameraMatrix;

        // Sending shadow data
        material.SetTexture("_shadowMap", _shadowMap);
        material.SetFloat("_shadowBias", _shadowBias);
        material.SetMatrix("_lightViewProj", lightViewProjMatrix);
    }

    private void OnDestroy()
    {
        if(_shadowMap != null)
        {
            _shadowMap.Release();
        }

        if(_lightCamera != null)
        {
            Destroy(_lightCamera.gameObject);
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 512, 512), _shadowMap, ScaleMode.ScaleToFit, false);
    }
}
