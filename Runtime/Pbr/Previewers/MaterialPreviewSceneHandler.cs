using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if HDRP_PIPELINE_ENABLED
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Muse.Texture
{
    internal class MaterialPreviewSceneHandler: IDisposable
    {
        Scene m_Scene;
        Camera m_Camera;
        List<Light> m_Lights;
        ReflectionProbe m_ReflectionProbe;
        Renderer m_MaterialTarget;
        GameObject m_Target;
        PrimitiveObjectTypes? m_CurrentPreviewType = null;
        HdriEnvironment? m_CurrentHdriEnvironment = null;
        RenderSettingsData m_RenderSettingsData = new RenderSettingsData();
#if HDRP_PIPELINE_ENABLED
        HDAdditionalReflectionData m_ReflectionProbeAdditionalData;
        VolumeProfile m_VolumeProfile;
        HDRISky m_HdriSky;
#endif

        public Scene Scene => m_Scene;
        public Camera Camera => m_Camera;
        public List<Light> Lights => m_Lights;
        public ReflectionProbe ReflectionProbe => m_ReflectionProbe;
        public Renderer MaterialTarget => m_MaterialTarget;
        public GameObject Target => m_Target;

        public MaterialPreviewSceneHandler(Scene scene)
        {
            m_Scene = scene;
            InitializeScene();
        }

        void InitializeScene()
        {
            InitializeCamera();
            InitializeLights();
            InitializePrimitiveTarget();
        }

        void InitializeCamera()
        {
#if HDRP_PIPELINE_ENABLED
            var previewCamera = GameObject.Instantiate(Resources.Load<GameObject>("HDRP/Preview Scene Camera"));
            previewCamera.hideFlags = HideFlags.DontSave;
            m_VolumeProfile = previewCamera.GetComponentInChildren<Volume>().profile;
            m_HdriSky = m_VolumeProfile.components.Find(x => x is HDRISky) as HDRISky; 
#else
            var previewCamera = new GameObject("Preview Scene Camera", typeof(Camera))
            {
                hideFlags = HideFlags.DontSave
            };
#endif

            AddGameObject(previewCamera);
            m_Camera = previewCamera.GetComponent<Camera>();
            m_Camera.enabled = false;
            m_Camera.clearFlags = CameraClearFlags.Depth;
            m_Camera.fieldOfView = 15f;
            m_Camera.farClipPlane = 50f;
            m_Camera.nearClipPlane = 1f;
            m_Camera.renderingPath = RenderingPath.Forward;
            m_Camera.useOcclusionCulling = false;
            m_Camera.scene = m_Scene;
            m_Camera.clearFlags = CameraClearFlags.SolidColor;

            var defaultBackgroundColor = Color.clear;
            var colorSpace = QualitySettings.activeColorSpace;
            m_Camera.backgroundColor = colorSpace == ColorSpace.Gamma ? defaultBackgroundColor : defaultBackgroundColor.linear;

#if HDRP_PIPELINE_ENABLED
            var cameraHighDefData = m_Camera.gameObject.GetComponent<HDAdditionalCameraData>();
            cameraHighDefData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
            cameraHighDefData.dithering = true;
            cameraHighDefData.clearDepth = true;
            cameraHighDefData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            cameraHighDefData.backgroundColorHDR = Color.clear;

            cameraHighDefData.customRenderingSettings = true;
            cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, true);
            cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Dithering, true);
            cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Tonemapping, false);
            cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SkyReflection, false);
            cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReflectionProbe, true);
            //TODO: What other HDRP render features?
#endif
        }

        void InitializeLights()
        {
            m_Lights = new List<Light>() { CreateLight(), CreateLight() };

            foreach (var light in m_Lights)
            {
                AddGameObject(light.gameObject);
            }

            m_Lights[0].intensity = 1f;
            m_Lights[0].transform.rotation = Quaternion.Euler(50f, -30f, 0.0f);
            m_Lights[0].transform.position = new Vector3(0.0f, 3.0f, 0.0f);
            m_Lights[0].color = Color.white;
            m_Lights[0].enabled = false;

            m_Lights[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
            m_Lights[1].color = new Color(0.4f, 0.4f, 0.45f, 0.0f) * 0.7f;
            m_Lights[1].intensity = .6f;

#if HDRP_PIPELINE_ENABLED
            foreach (var light in m_Lights)
            {
                var additionalLightData = light.gameObject.AddComponent<HDAdditionalLightData>();
                additionalLightData.type = HDLightType.Directional;
                additionalLightData.intensity = light.intensity;
                additionalLightData.color = light.color;

                //TODO: What other HDRP light features?
            }
#endif
        }

        static Light CreateLight()
        {
            var newPreviewLight = new GameObject("PreviewLight", typeof(Light))
            {
                hideFlags = HideFlags.DontSave
            };

            var component = newPreviewLight.GetComponent<Light>();
            component.type = LightType.Directional;
            component.intensity = 1f;
            component.enabled = false;
            return component;
        }

        internal void InitializeReflectionProbe(HdriEnvironment environment = HdriEnvironment.Default)
        {
            m_CurrentHdriEnvironment = environment;
            var reflectionCubemap = HdriProvider.GetHdri(m_CurrentHdriEnvironment.Value); 
            
#if !HDRP_PIPELINE_ENABLED
            m_RenderSettingsData.skybox = new Material(Shader.Find("Skybox/Cubemap"))
            {
                hideFlags = HideFlags.DontSave
            };
            m_RenderSettingsData.ambientMode = AmbientMode.Skybox;
            m_RenderSettingsData.ambientIntensity = 1.5f;
            m_RenderSettingsData.ambientSkyColor = Color.white;
            m_RenderSettingsData.ambientGroundColor = Color.white;
            m_RenderSettingsData.ambientEquatorColor = Color.white;

            m_RenderSettingsData.skybox.SetTexture("_Tex", reflectionCubemap);
            m_RenderSettingsData.defaultReflectionMode = DefaultReflectionMode.Custom;
            m_RenderSettingsData.reflectionBounces = 0;
            m_RenderSettingsData.reflectionIntensity = 0f;
            
#endif
            
            if (m_ReflectionProbe == null)
            {
                var reflectionGo = new GameObject();
                m_ReflectionProbe = reflectionGo.AddComponent<ReflectionProbe>();
                m_ReflectionProbe.intensity = 1f;
                m_ReflectionProbe.mode = ReflectionProbeMode.Custom;
                
                m_ReflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                m_ReflectionProbe.importance = 1;
                m_ReflectionProbe.size = new Vector3(100f, 100f, 100f);
#if HDRP_PIPELINE_ENABLED
                m_ReflectionProbeAdditionalData = m_ReflectionProbe.gameObject.AddComponent<HDAdditionalReflectionData>();
#endif

                AddGameObject(reflectionGo); 
            }
#if !HDRP_PIPELINE_ENABLED
            m_RenderSettingsData.ApplyCurrentSettings();
            DynamicGI.UpdateEnvironment();
#endif

            m_ReflectionProbe.customBakedTexture = reflectionCubemap;
            
#if HDRP_PIPELINE_ENABLED
            m_HdriSky.hdriSky.Override(reflectionCubemap);
            m_ReflectionProbeAdditionalData.customTexture = reflectionCubemap;
            m_ReflectionProbeAdditionalData.RequestRenderNextUpdate();
#endif
        }
        
        public void InitializePrimitiveTarget(PrimitiveObjectTypes primitiveObject = PrimitiveObjectTypes.Sphere)
        {
            if(m_CurrentPreviewType == primitiveObject)
                return;
            
            m_CurrentPreviewType = primitiveObject;
            
            if (m_Target != null)
            {
                if(Application.isPlaying)
                    GameObject.Destroy(m_Target);
                else
                    GameObject.DestroyImmediate(m_Target);
            }
            
            m_Target =PrimitiveObjectsProvider.GetPrimitiveInstance(m_CurrentPreviewType.Value);
            m_Target.name = "PrimitiveTarget";
            m_Target.hideFlags = HideFlags.DontSave;
            m_MaterialTarget = m_Target.GetComponentInChildren<Renderer>();
            AddGameObject(m_Target);
        }

        void AddGameObject(GameObject go)
        {
            SceneManager.MoveGameObjectToScene(go, m_Scene);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SceneManager.UnloadSceneAsync(m_Scene);
            }
            else
            {
                UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(m_Scene);
            }
#else
            SceneManager.UnloadSceneAsync(m_Scene);
#endif
        }
    }
}
