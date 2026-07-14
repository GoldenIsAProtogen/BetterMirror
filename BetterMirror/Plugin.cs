using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterMirror
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> _quality;
        
        private Transform _mirrorGTFC;
        private Camera    _mirrorCamGTFC;
        private int       _qualityCacheGTFC;

        private bool      _firstRun;
        private Transform _mirror;
        private Camera    _mirrorCam;
        private int       _qualityCache;

        private Plugin()
        {
            ConfigDescription qualityDescription = new ConfigDescription(
                    "Multiplies the mirror quality by this number",
                    new AcceptableValueRange<int>(1, 4)
            );

            _quality = Config.Bind(
                    "Settings",
                    "Mirror Quality Multiplier",
                    4,
                    qualityDescription);
        }

        private void Start() => SceneManager.sceneLoaded += CityLoadedCheck;

        private void CityLoadedCheck(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "City") return;
            
            Transform _city = GameObject.Find("City_Pretty").transform;

            _mirrorGTFC = _city.FindChildRecursive("DressingRoom_Mirrors_Prefab");
            _mirror = _city.Find("CosmeticsRoomAnchor/nicegorillastore_prefab/DressingRoom_Mirrors_Prefab");

            _mirror.GetChild(1).gameObject.SetActive(false);

            _mirrorCam                          = _mirror.GetComponentInChildren<Camera>();
            _mirrorCamGTFC = _mirrorGTFC.GetComponentInChildren<Camera>();
            _mirrorCam.farClipPlane             = 40;
            _mirrorCam.targetTexture.filterMode = FilterMode.Point;
            _mirrorCamGTFC.targetTexture.filterMode = FilterMode.Point;

            _quality.Value = Mathf.Clamp(_quality.Value, 1, 4);

            if (!_firstRun)
            {
                _mirrorCam.targetTexture.width      *= _quality.Value;
                _mirrorCam.targetTexture.height     *= _quality.Value;
                _mirrorCamGTFC.targetTexture.width  *= _quality.Value;
                _mirrorCamGTFC.targetTexture.height *= _quality.Value;
                _qualityCache                       =  _mirrorCam.targetTexture.height;
                _qualityCacheGTFC                   =  _mirrorCamGTFC.targetTexture.width;
                _firstRun                           =  true;
            }
            else
            {
                _mirrorCam.targetTexture.width      = _qualityCache;
                _mirrorCam.targetTexture.height     = _qualityCache;
                _mirrorCamGTFC.targetTexture.width  = _qualityCacheGTFC;
                _mirrorCamGTFC.targetTexture.height = _qualityCacheGTFC;
            }

            SetLayers(_city.transform);
        }

        private static void SetLayers(Transform t)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("NoMirror"))
                t.gameObject.layer = 0;

            foreach (Transform tr in t)
                SetLayers(tr);
        }
    } 
}