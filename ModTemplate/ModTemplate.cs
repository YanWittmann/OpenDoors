using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {
        private bool _ready = false;
        private bool _filterObjects = true;
        private float _maxDistance = 30f;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            ModHelper.Console.WriteLine($"Loaded Open Doors mod", MessageType.Success);
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem || _ready) return;
                CreateDoorObjectNames();
                ModHelper.Console.WriteLine($"Open Doors mod is now ready", MessageType.Success);
                _ready = true;
            };
        }

        private void InteractionNearbyObjects(bool closeDoors)
        {
            var playerBody = FindObjectOfType<PlayerBody>();
            var playerPos = playerBody.transform.position;

            var allGameObjects = GetAllGameObjectsAroundPosition(playerPos, _maxDistance);
            var numObjects = 0;
            var affectedObjects = 0;
            var saveString = "";

            var logString = "";

            foreach (var obj in allGameObjects)
            {
                numObjects++;
                saveString += $"{obj.name}\t\t{obj.activeSelf}\t\t{obj.transform.childCount}\n";

                if (IsHideableObject(obj))
                {
                    SetGameObjectVisibility(obj, closeDoors);

                    affectedObjects++;
                    if (obj.transform.childCount > 0)
                    {
                        logString += $" {obj.name} ({obj.transform.childCount})";
                    }
                    else
                    {
                        logString += $" {obj.name}";
                    }
                }
            }

            affectedObjects += SetFullPathObjectsVisibility(closeDoors);

            if (affectedObjects > 0)
            {
                logString = $"Applied door state to ({affectedObjects}/{numObjects}) objects: " + logString;
                ModHelper.Console.WriteLine(logString, MessageType.Success);
            }
            else
            {
                ModHelper.Console.WriteLine("No objects affected", MessageType.Warning);
            }

            File.WriteAllText("objectCounts.txt", saveString);
            ModHelper.Console.WriteLine($"Saved {numObjects} objects to file {Path.GetFullPath("objectCounts.txt")}",
                MessageType.Info);
        }

        private HashSet<GameObject> GetAllGameObjectsAroundPosition(Vector3 referencePosition, float maxDistance)
        {
            var queryAllGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var allGameObjects = new HashSet<GameObject>();

            foreach (var obj in queryAllGameObjects)
            {
                var objectPos = obj.transform.position;
                var distance = Vector3.Distance(objectPos, referencePosition);
                var objPosMag = objectPos.magnitude;

                if (objPosMag == 0 || distance > maxDistance || distance == 0) continue;

                allGameObjects.Add(obj);

                ExtractAllParents(obj, allGameObjects);
            }

            if (_filterObjects)
            {
                allGameObjects.RemoveWhere(ShouldObjectNameBeSkipped);
            }

            return allGameObjects;
        }

        private static void ExtractAllParents(GameObject obj, HashSet<GameObject> allParents)
        {
            while (true)
            {
                if (obj.transform.parent == null) return;
                var parent = obj.transform.parent;
                if (allParents.Add(parent.gameObject))
                {
                    obj = parent.gameObject;
                    continue;
                }

                break;
            }
        }

        private static string GetClipboardText()
        {
            return GUIUtility.systemCopyBuffer;
        }

        private int SetFullPathObjectsVisibility(bool visible)
        {
            var count = 0;

            foreach (var (key, value) in _hideDoorObjectsByFullPath)
            {
                var obj = GameObject.Find(key);
                if (obj == null) continue;
                SetGameObjectVisibility(obj, visible);
                count++;
            }

            return count;
        }

        private void SetGameObjectVisibility(GameObject obj, bool visible)
        {
            if (visible)
            {
                ShowGameObject(obj);
            }
            else
            {
                HideGameObject(obj);
            }
        }

        private void HideGameObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void ShowGameObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        private Dictionary<string, string> _hideDoorObjectsByFullPath = new();
        private Dictionary<string, string> _hideDoorObjectsEquals = new();
        private Dictionary<string, string> _hideDoorObjectsConatins = new();

        private void CreateDoorObjectNames()
        {
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_SouthUnderground/Sector_FossilCave/Interactables_FossilCave/ProbePrompt_PodFossilWindow", "anglerfish fossil overview pod collision");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_SouthUnderground/Sector_FossilCave/Geometry_FossilCave/OtherComponentsGroup/Rocks_FossilOverlook/BatchedGroup/BatchedMeshRenderers_1", "anglerfish fossil overview pod stalagmites");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthUnderground/Sector_LakebedCaves/Geometry_LakebedCaves/Rocks", "lakebed stalagmites stalactites");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Interactables_CaveTwin/Structure_NOM_EyeSymbol", "sunless city eye symbol outside");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Interactables_CaveTwin/Structure_NOM_EyeSymbol (1)", "sunless city eye symbol inside");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_CannonPath/Geometry_CannonPath/OtherComponentsGroup/Rocks", "sunless city cannon path stones");
            _hideDoorObjectsByFullPath.Add("CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_CannonPath/Geometry_CannonPath/BatchedGroup/BatchedMeshColliders_0", "sunless city cannon path stones colliders");

            _hideDoorObjectsEquals.Add("slabs_door", "large orb doors");
            _hideDoorObjectsEquals.Add("Structure_NOM_RotatingDoor_Broken_Panels", "single sided rotating orb door");
            _hideDoorObjectsEquals.Add("PointLight_NOM_OrbSmall", "general door orb");
            _hideDoorObjectsEquals.Add("Props_NOM_TractorBeam", "tractor beam (ring)");
            _hideDoorObjectsEquals.Add("BeamVolume", "tractor beam (beam)");
            _hideDoorObjectsEquals.Add("HazardVolume", "removes all hazards");
            _hideDoorObjectsEquals.Add("Cacti", "cactus parent object");
            _hideDoorObjectsEquals.Add("DarkMatter", "ghost matter");
            _hideDoorObjectsEquals.Add("DarkMatterVolume", "ghost matter");
            _hideDoorObjectsEquals.Add("GhostMatter_Clutter", "ghost matter");
            _hideDoorObjectsEquals.Add("Props_GM_Clutter", "ghost matter");

            _hideDoorObjectsConatins.Add("Cactus", "all variants and plants on cacti");
            _hideDoorObjectsConatins.Add("Structure_NOM_RotatingDoor_Panel", "both sided rotating orb door");

        }

        private bool IsHideableObject(GameObject obj)
        {
            var objectName = GetObjectNameOnly(obj);

            if (_hideDoorObjectsEquals.ContainsKey(objectName))
            {
                return true;
            }

            foreach (var (key, value) in _hideDoorObjectsConatins)
            {
                if (objectName.Contains(key))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetObjectNameOnly(GameObject obj)
        {
            return obj.name.Split(' ')[0];
        }

        private static bool ShouldObjectNameBeSkipped(GameObject obj)
        {
            return ExcludeIfContainedInName.Any(excludeIfContained => obj.name.Contains(excludeIfContained));
        }

        [SerializeField] private static readonly string[] ExcludeIfContainedInName =
            "LeftArrow,LabelBG,Top,ScreenPrompt,CommandImage,Text,Scroll View,Viewport,HorizontalLayoutGroup,Checkbox,Box,ScreenPromptListBottomLeft,Arm_M_pivot,Root,HelmetElectricalArc_3,Props_HEA_Probe_Prelaunch,ThrusterWash,DownImage,GlassBorder,ScanLightVolume4,Effects_NOM_OrbitHologram_Large,OPC_WingPiece_Tip_02_SunkenModule_Hologram,Reticule1,NomaiTranslatorProp,RightBubbles,Effects_HEA_MarshmallowFlames,Arrow1Pivot,UnderwaterEffectBubble,ScanProjector1,QuantumFogEffectBubble,Effects_NOM_OrbitalProbeCannon_Hologram,DataParticles,Effects_IP_Z4RaftHouseSplash3,SafetyCollider,GiantsDeepRoot,Sliding,EyeCoords,PlayerFootstep_Dirt,FlashlightRoot,HUD_HelmetCracks,giantsDeep,Flashlight_BasePivot,OPC_Cannon_Mid_Hologram,LeftImage,Props_HEA_ProbeLauncher_ProbeCamera,Props_HEA_Translator_RotatingPart,ToolHoldTransform,ScrollSocket,Slides_Front,Effects_IP_Z4RaftHouseSplash2,Scrollbar,Effects_HEA_ThrusterFlame,Props_HEA_Translator_Button_R,TextWarningBlock,ScaleAndRotate,Effects_NOM_HologramDrips,ScreenPromptList,RecallEffect,LockOnGUI,HelmetVisorMaskRenderer,CommandImage,FogWarpEffectBubble,Props_NOM_SmallTractorBeam_Geo,ToolModeUI,HelmetUVRenderer,HelmetFrame,Reticule2,Bottom,RotBuildingSplash_8,Props_HEA_ProbeLauncher,CanvasMarker,OPC_WingPiece_Mid_02_SunkenModule_Hologram,PlayerFootstep_None,Props_HEA_Translator_Prepass,ProbeLauncher,Scarf,HelmetRoot,HorizontalLayoutGroup,Props_HEA_Flashlight_FrontHeadlight,BackwardRightThrust,DreamEyeMask,Mallow_Root,TranslatorGroup,LaunchParticleEffect_Underwater,OPC_Module_Sunken_Hologram,ThrusterLight,OPC_WingPiece_Mid_01_SunkenModule_Hologram,ProbeLauncherChassis,VesselCoreSocket,ForwardRightBubbles,ItemSocket,ScreenPrompt,Scroll,Props_NOM_SmallTractorBeam_Anchor,DownThrust,HelmetRainDroplets,Content,ScanLightVolume5,Effects_NOM_ProbeHologram,BackwardLeftBubbles,LockOnCircle,Stick_Tip,OffScreenIndicator,HelmetVisorUVRenderer,HelmetRainStreaks,Frame_Whole,ImageBlock,OPC_Cannon_Tip_Hologram,Frame_8,Props_HEA_Translator_Button_L,CameraDetector,Effects_HEA_AirLeak,Probe,HelmetVisorEffects,SimpleLanternSocket,ThrusterWash_Default,ScanLightVolume3,Traveller_Mesh_v01,WarpCoreSocket,HelmetElectricalArc_2,BakedTerrain_Proxy_QPolePath_4_Baked,ToolStowTransform,Stick_Pivot,ProbeLauncherTransform,TextInfoBlock,Arm_S_pivot,FogWarpMarker,PointLight_HEA_TranslatorBulb,Props_HEA_Signalscope,Props_HEA_Probe_Prelaunch_Prepass,Hologram_AllProbeTrajectories,TranslatorBeams,HelmetOffLockOn,LineX,PlayerFootstep_Snow,DataStream,ScreenEffects,HUD_CurvedSurface,PageNumberText,RightThrust,VisionTorchSocket,BakedTerrain_VM_Proxy_Base,Top,Canvas,ForwardLeftThrust,ScanProjector2,LaunchParticleEffect,Props_HEA_Translator,ScanProjector4,player_mesh_noSuit,PressureGauge_Arrow,CanvasMarkerManager,ScanProjector5,RingCircle,Ring,UniverseLibCanvas,ScanLightVolume2,ForwardLeftBubbles,TranslatorScanBeam3,ShadowProjector,OPC_Base_Hologram,AttachPointWarningBlock,Props_HEA_Translator_Screen,PointLight_HEA_TranslatorButtonLeft,LineY,Flashlight_WobblePivot,Props_HEA_Translator_Pivot_RotatingPart,RoastingStick_Arm_NoSuit,Lines,ScanLightVolume1,LeftThrust,ScaleRoot,Handle,Bracket,TranslatorScanBeam1,Arm_L_pivot,LighthouseSplash_2,Props_HEA_RoastingStick,MallowSmoke,Slides_Back,preLaunchCamera,BackwardRightBubbles,Traveller_HEA_Player_v2,VesselCoreStowTransform,Stick_Root,LightFlickerEffectBubble,Flashlight_WobblePivot_OldTransforms,Effects_IP_LighthouseSplash,PointLight_HEA_TranslatorBulb2,TextScaleRoot,SingularityEffectAmbientAudio,RoastingStick_Stick,PointLight_HEA_TranslatorButtonRight,ArrowPivot,Arrow,HelmetMesh,Traveller_Rig_v01,ItemCarryTool,HUD_Helmet_v2,NebulaParticles,Frame_7,Background,RightImage,Exclamation,Text,SharedStoneSocket,Flashlight_SpotLight,Frame_6,ImageWarningBlock,RotBuildingSplash_9,CloudsEffectBubble,TranslatorScanBeam4,ScanProjector3,SlideReelSocket,FullTextBlock,SingularityEffectOneShotAudio,Arrows,ForwardRightThrust,Props_HEA_Flashlight_Geo,Props_HEA_Signalscope_Prepass,TranslatorText,HelmetElectricalArc_1,LeftBubbles,UpThrust,AttachPointInfoBlock,UpBubbles,Props_HEA_Flashlight,DreamLanternSocket,WarningBlock,TranslatorScanBeam2,CenteringPivot,Lighting,Cannon_Pivot,Viewport,LighthouseSplash_4,Props_HEA_Translator_Geo,Effects_IP_Z4RaftHouseSplash4,Props_HEA_Marshmallow,HighlightBracket,DarkMatterBubble,DownBubbles,Props_HEA_Translator_RotatingPart_Prepass,AmbientLight_EyeHologram,Signalscope,Flashlight_FillLight,SandEffectBubble,BakedTerrain_Proxy_Frag_23_Baked,GlassScreen,Helmet,TranslatorScanBeam5,HUDController,UpImage,RoastingSystem,PlayerCamera,BackwardLeftThrust,RoastingStick_Arm,Props_HEA_ProbeLauncher_Prepass"
                .Split(',');

        public void Update()
        {
            if (!_ready) return;

            if (Keyboard.current[Key.O].wasPressedThisFrame)
            {
                InteractionNearbyObjects(false);
            }

            if (Keyboard.current[Key.P].wasPressedThisFrame)
            {
                InteractionNearbyObjects(true);
            }

            if (Keyboard.current[Key.L].wasPressedThisFrame)
            {
                _filterObjects = !_filterObjects;
            }

            if (Keyboard.current[Key.K].wasPressedThisFrame)
            {
                _maxDistance = _maxDistance + 10;
                ModHelper.Console.WriteLine($"Max distance: {_maxDistance}");
            }

            if (Keyboard.current[Key.J].wasPressedThisFrame)
            {
                _maxDistance = _maxDistance - 10;
                ModHelper.Console.WriteLine($"Max distance: {_maxDistance}");
            }

            if (Keyboard.current[Key.I].wasPressedThisFrame)
            {
                var clipboardText = GetClipboardText();
                if (clipboardText.Length > 0)
                {
                    ModHelper.Console.WriteLine($"Adding {clipboardText} to hide-able objects");
                    _hideDoorObjectsEquals.Add(clipboardText, clipboardText + " (from clipboard)");
                    _hideDoorObjectsConatins.Add(clipboardText, clipboardText + " (from clipboard)");
                    _hideDoorObjectsByFullPath.Add(clipboardText, clipboardText + " (from clipboard)");
                }
            }
        }
    }
}