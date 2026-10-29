using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Master automation script for the upgraded audio architecture.
/// Balances sound levels, configures AudioImporters, updates SoundDataSO definitions,
/// constructs the in-game Audio Settings UI panel, and integrates buttons across all scenes.
/// </summary>
public static class AudioSystemSetup
{
    private const string AudioRoot = "Assets/_Project/Audio";
    private const string MixerPath = "Assets/_Project/Audio/Mixers/MasterMixer.mixer";
    private const string DataPath = "Assets/_Project/Audio/Data";
    private const string PrefabPath = "Assets/_Project/Prefabs/AudioManager.prefab";
    private const string FontPath = "Assets/_Project/Art/UI/LilitaOne SDF.asset";
    private const string UIPack = "Assets/_Project/Art/UI/KenneyUI/Pack/PNG";

    // [MenuItem("Audio/Execute Full Audio Setup & Integration", priority = 1)]
    public static void RunFullAudioSetup()
    {
        Debug.Log("<color=#4CAF50><b>[AudioSetup] Beginning End-to-End Audio Upgrade & Settings Integration...</b></color>");

        EnsureDirectories();
        AudioMixer mixer = SetupAudioMixer();
        ConfigureAudioImporters();
        List<SoundDataSO> soundDataList = GenerateSoundDataAssets(mixer);
        GameObject audioManagerPrefab = CreateOrUpdateAudioManagerPrefab(mixer, soundDataList);
        WireAllScenesAndPrefabs(audioManagerPrefab, soundDataList);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#4CAF50><b>[AudioSetup] Full Audio Upgrade & Integration Completed Successfully!</b></color>");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            AudioRoot,
            $"{AudioRoot}/BGM",
            $"{AudioRoot}/Ambience",
            $"{AudioRoot}/SFX",
            $"{AudioRoot}/UI",
            $"{AudioRoot}/Mixers",
            DataPath,
            "Assets/_Project/Prefabs"
        };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    public static AudioMixer SetupAudioMixer()
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if (mixer == null)
        {
            try
            {
                Type controllerType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
                if (controllerType != null)
                {
                    MethodInfo createMethod = controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (createMethod != null)
                    {
                        object controller = createMethod.Invoke(null, new object[] { MixerPath });
                        if (controller != null)
                        {
                            MethodInfo masterGroupProp = controllerType.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetGetMethod(true);
                            MethodInfo createNewGroup = controllerType.GetMethod("CreateNewGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            MethodInfo addChildToParent = controllerType.GetMethod("AddChildToParent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            object masterGroup = masterGroupProp?.Invoke(controller, null);

                            string[] subGroups = { "BGM", "Ambience", "SFX", "UI" };
                            if (masterGroup != null && createNewGroup != null && addChildToParent != null)
                            {
                                foreach (string grpName in subGroups)
                                {
                                    object newGrp = createNewGroup.Invoke(controller, new object[] { grpName, false });
                                    if (newGrp != null)
                                    {
                                        addChildToParent.Invoke(controller, new object[] { newGrp, masterGroup });
                                    }
                                }
                            }

                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AudioSetup] AudioMixerController creation exception: {ex.Message}");
            }
        }

        // Expose parameters for Master, BGM, Ambience, SFX, UI
        if (mixer != null)
        {
            ExposeMixerVolumeParameters(mixer);
        }

        return mixer;
    }

    public static void ExposeMixerVolumeParameters(AudioMixer mixer)
    {
        try
        {
            Type controllerType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            Type groupType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerGroupController");
            Type elemType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");

            if (controllerType == null || groupType == null || elemType == null) return;

            PropertyInfo masterGroupProp = controllerType.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object masterGroup = masterGroupProp?.GetValue(mixer, null);
            if (masterGroup == null) return;

            PropertyInfo childrenProp = groupType.GetProperty("children", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Array children = (Array)childrenProp?.GetValue(masterGroup, null);

            var allGroups = new List<object> { masterGroup };
            if (children != null)
            {
                foreach (var child in children) allGroups.Add(child);
            }

            MethodInfo getGuidForVol = groupType.GetMethod("GetGUIDForVolume", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo nameProp = groupType.GetProperty("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo guidField = elemType.GetField("guid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo nameField = elemType.GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var exposedList = new List<object>();
            foreach (var grp in allGroups)
            {
                string gName = (string)nameProp.GetValue(grp, null);
                var guid = getGuidForVol.Invoke(grp, null);
                string exposedName = gName == "Master" ? "MasterVolume" : gName + "Volume";

                object exposedParam = Activator.CreateInstance(elemType);
                guidField.SetValue(exposedParam, guid);
                nameField.SetValue(exposedParam, exposedName);
                exposedList.Add(exposedParam);
            }

            Array exposedArray = Array.CreateInstance(elemType, exposedList.Count);
            for (int i = 0; i < exposedList.Count; i++)
            {
                exposedArray.SetValue(exposedList[i], i);
            }

            PropertyInfo exposedParamProp = controllerType.GetProperty("exposedParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            exposedParamProp?.SetValue(mixer, exposedArray, null);

            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AudioSetup] Failed to expose mixer parameters: {ex.Message}");
        }
    }

    public static void ConfigureAudioImporters()
    {
        Debug.Log("[AudioSetup] Configuring AudioImporter memory and compression settings...");

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioRoot });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            string filename = Path.GetFileNameWithoutExtension(path);
            bool is3DSound = filename.StartsWith("SFX_Player_") || filename.StartsWith("SFX_Footstep") ||
                             filename.StartsWith("SFX_Impact") || filename.StartsWith("SFX_Platform");

            if (path.Contains("/BGM/") || path.Contains("/Ambience/"))
            {
                // BGM & Ambience Loops: Streaming, Vorbis 70%
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.70f;
                importer.forceToMono = false;
            }
            else if (filename.Contains("Victory") || filename.Contains("Death") || filename.Contains("Wind_Velocity") || filename.Contains("Fall"))
            {
                // Medium SFX: CompressedInMemory, Vorbis 70%
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.70f;
                importer.forceToMono = is3DSound && !filename.Contains("Fall");
            }
            else
            {
                // Short SFX & UI (< 2s): DecompressOnLoad, PCM
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                importer.forceToMono = is3DSound;
            }

            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            count++;
        }

        Debug.Log($"[AudioSetup] Successfully configured AudioImporter settings for {count} audio assets.");
    }

    public static List<SoundDataSO> GenerateSoundDataAssets(AudioMixer mixer)
    {
        Debug.Log("[AudioSetup] Generating SoundDataSO ScriptableObjects with balanced volume levels...");

        AudioMixerGroup masterGrp = null;
        AudioMixerGroup bgmGrp = null;
        AudioMixerGroup ambGrp = null;
        AudioMixerGroup sfxGrp = null;
        AudioMixerGroup uiGrp = null;

        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
            foreach (var g in groups)
            {
                if (g.name.Equals("Master", StringComparison.OrdinalIgnoreCase)) masterGrp = g;
                else if (g.name.Equals("BGM", StringComparison.OrdinalIgnoreCase)) bgmGrp = g;
                else if (g.name.Equals("Ambience", StringComparison.OrdinalIgnoreCase)) ambGrp = g;
                else if (g.name.Equals("SFX", StringComparison.OrdinalIgnoreCase)) sfxGrp = g;
                else if (g.name.Equals("UI", StringComparison.OrdinalIgnoreCase)) uiGrp = g;
            }
            if (masterGrp == null && groups.Length > 0) masterGrp = groups[0];
            if (bgmGrp == null) bgmGrp = masterGrp;
            if (ambGrp == null) ambGrp = masterGrp;
            if (sfxGrp == null) sfxGrp = masterGrp;
            if (uiGrp == null) uiGrp = masterGrp;
        }

        List<SoundDataSO> generatedList = new List<SoundDataSO>();

        // Optimized & balanced definitions: BGM 0.35, Ambience 0.20, SFX 0.65-0.70, UI 0.60
        var definitions = new[]
        {
            new {
                ID = "BGM_Level01_Daytime",
                Group = bgmGrp,
                Vol = 0.35f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "BGM_Level01_Daytime" }
            },
            new {
                ID = "BGM_Level02_Sunset",
                Group = bgmGrp,
                Vol = 0.35f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "BGM_Level02_Sunset" }
            },
            new {
                ID = "BGM_Level03_Cosmic",
                Group = bgmGrp,
                Vol = 0.35f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "BGM_Level03_Cosmic" }
            },
            new {
                ID = "Ambience_MeadowWind",
                Group = ambGrp,
                Vol = 0.20f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "Ambience_MeadowWind" }
            },
            new {
                ID = "Ambience_OceanBreeze",
                Group = ambGrp,
                Vol = 0.20f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "Ambience_OceanBreeze" }
            },
            new {
                ID = "Ambience_CosmicDrone",
                Group = ambGrp,
                Vol = 0.18f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "Ambience_CosmicDrone" }
            },
            new {
                ID = "SFX_Player_Jump",
                Group = sfxGrp,
                Vol = 0.65f,
                Pitch = new Vector2(0.95f, 1.05f),
                Spatial = 1.0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.05f,
                ClipSearch = new[] { "SFX_Player_Jump_01", "SFX_Player_Jump_02", "SFX_Player_Jump_03" }
            },
            new {
                ID = "SFX_Player_Land",
                Group = sfxGrp,
                Vol = 0.60f,
                Pitch = new Vector2(0.94f, 1.06f),
                Spatial = 1.0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.06f,
                ClipSearch = new[] { "SFX_Player_Land_01", "SFX_Player_Land_02", "SFX_Player_Land_03" }
            },
            new {
                ID = "SFX_Player_Footstep",
                Group = sfxGrp,
                Vol = 0.45f,
                Pitch = new Vector2(0.92f, 1.08f),
                Spatial = 1.0f,
                Dist = new Vector2(1f, 25f),
                Loop = false,
                Cooldown = 0.08f,
                ClipSearch = new[] { "SFX_Footstep_01", "SFX_Footstep_02", "SFX_Footstep_03", "SFX_Footstep_04" }
            },
            new {
                ID = "SFX_Coin",
                Group = sfxGrp,
                Vol = 0.70f,
                Pitch = new Vector2(0.96f, 1.08f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.02f,
                ClipSearch = new[] { "SFX_Coin_01", "SFX_Coin_02", "SFX_Coin_03" }
            },
            new {
                ID = "SFX_Player_Fall",
                Group = sfxGrp,
                Vol = 0.85f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.2f,
                ClipSearch = new[] { "SFX_Player_Fall" }
            },
            new {
                ID = "SFX_Player_Death",
                Group = sfxGrp,
                Vol = 0.90f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.2f,
                ClipSearch = new[] { "SFX_Player_Death" }
            },
            new {
                ID = "SFX_Level_Victory",
                Group = sfxGrp,
                Vol = 0.90f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.5f,
                ClipSearch = new[] { "SFX_Level_Victory" }
            },
            new {
                ID = "SFX_Impact",
                Group = sfxGrp,
                Vol = 0.75f,
                Pitch = new Vector2(0.92f, 1.08f),
                Spatial = 1.0f,
                Dist = new Vector2(1.5f, 30f),
                Loop = false,
                Cooldown = 0.06f,
                ClipSearch = new[] { "SFX_Impact_01", "SFX_Impact_02", "SFX_Impact_03" }
            },
            new {
                ID = "SFX_Wind_Velocity",
                Group = sfxGrp,
                Vol = 0.25f,
                Pitch = new Vector2(0.90f, 1.20f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "SFX_Wind_Velocity" }
            },
            new {
                ID = "SFX_Platform_Hum",
                Group = sfxGrp,
                Vol = 0.15f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 1.0f,
                Dist = new Vector2(1f, 6f),
                Loop = true,
                Cooldown = 0.05f,
                ClipSearch = new[] { "SFX_Platform_Hum" }
            },
            new {
                ID = "UI_Click",
                Group = uiGrp,
                Vol = 0.65f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.04f,
                ClipSearch = new[] { "UI_Click_01", "UI_Click_02" }
            },
            new {
                ID = "UI_Hover",
                Group = uiGrp,
                Vol = 0.40f,
                Pitch = new Vector2(0.98f, 1.02f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.04f,
                ClipSearch = new[] { "UI_Hover_01", "UI_Hover_02" }
            },
            new {
                ID = "UI_Pause",
                Group = uiGrp,
                Vol = 0.70f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.05f,
                ClipSearch = new[] { "UI_Pause" }
            },
            new {
                ID = "UI_Resume",
                Group = uiGrp,
                Vol = 0.70f,
                Pitch = new Vector2(1f, 1f),
                Spatial = 0f,
                Dist = new Vector2(1f, 35f),
                Loop = false,
                Cooldown = 0.05f,
                ClipSearch = new[] { "UI_Resume" }
            }
        };

        foreach (var def in definitions)
        {
            string assetPath = $"{DataPath}/{def.ID}.asset";
            SoundDataSO so = AssetDatabase.LoadAssetAtPath<SoundDataSO>(assetPath);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<SoundDataSO>();
                AssetDatabase.CreateAsset(so, assetPath);
            }

            so.SoundID = def.ID;
            so.MixerGroup = def.Group;
            so.Volume = def.Vol;
            so.PitchVariation = def.Pitch;
            so.SpatialBlend = def.Spatial;
            so.MinMaxDistance = def.Dist;
            so.Loop = def.Loop;
            so.CooldownTime = def.Cooldown;

            // Find matching clips
            List<AudioClip> foundClips = new List<AudioClip>();
            foreach (string searchPattern in def.ClipSearch)
            {
                string[] guids = AssetDatabase.FindAssets($"{searchPattern} t:AudioClip", new[] { AudioRoot });
                foreach (string guid in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(p).Equals(searchPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                        if (clip != null && !foundClips.Contains(clip))
                        {
                            foundClips.Add(clip);
                        }
                    }
                }
            }

            so.Clips = foundClips.ToArray();
            EditorUtility.SetDirty(so);
            generatedList.Add(so);
        }

        AssetDatabase.SaveAssets();
        return generatedList;
    }

    public static GameObject CreateOrUpdateAudioManagerPrefab(AudioMixer mixer, List<SoundDataSO> soundDataList)
    {
        Debug.Log("[AudioSetup] Building/Updating AudioManager prefab at " + PrefabPath);

        GameObject root = new GameObject("[AudioManager]");
        AudioManager mgr = root.AddComponent<AudioManager>();

        SerializedObject so = new SerializedObject(mgr);
        so.Update();

        if (mixer != null)
        {
            so.FindProperty("masterMixer").objectReferenceValue = mixer;

            AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
            foreach (var g in groups)
            {
                if (g.name.Equals("Master", StringComparison.OrdinalIgnoreCase))
                    so.FindProperty("masterGroup").objectReferenceValue = g;
                else if (g.name.Equals("BGM", StringComparison.OrdinalIgnoreCase))
                    so.FindProperty("bgmGroup").objectReferenceValue = g;
                else if (g.name.Equals("Ambience", StringComparison.OrdinalIgnoreCase))
                    so.FindProperty("ambienceGroup").objectReferenceValue = g;
                else if (g.name.Equals("SFX", StringComparison.OrdinalIgnoreCase))
                    so.FindProperty("sfxGroup").objectReferenceValue = g;
                else if (g.name.Equals("UI", StringComparison.OrdinalIgnoreCase))
                    so.FindProperty("uiGroup").objectReferenceValue = g;
            }
        }

        SerializedProperty dbProp = so.FindProperty("soundDatabase");
        dbProp.ClearArray();
        for (int i = 0; i < soundDataList.Count; i++)
        {
            dbProp.InsertArrayElementAtIndex(i);
            dbProp.GetArrayElementAtIndex(i).objectReferenceValue = soundDataList[i];
        }

        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        return prefab;
    }

    public static void WireAllScenesAndPrefabs(GameObject audioManagerPrefab, List<SoundDataSO> soundDataList)
    {
        Debug.Log("[AudioSetup] Wiring scenes, prefabs, and Audio Settings UI...");

        // Wire Player.prefab
        string playerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        if (playerPrefab != null)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
            {
                GameObject root = scope.prefabContentsRoot;
                EnsureComponent<PlayerAudioDriver>(root);
                EnsureComponent<ContinuousVelocityAudio>(root);
                EnsureComponent<CollisionAudioEmitter>(root);
            }
        }

        // List of project scenes
        string[] scenes = {
            "Assets/_Project/Scenes/Level01.unity",
            "Assets/_Project/Scenes/Level02_SunsetArchipelago.unity",
            "Assets/_Project/Scenes/Level03_NeonSpireway.unity"
        };

        string currentScene = SceneManager.GetActiveScene().path;

        foreach (string scenePath in scenes)
        {
            if (!File.Exists(scenePath)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1. Ensure [AudioManager] exists in scene
            AudioManager existingMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
            if (existingMgr == null && audioManagerPrefab != null)
            {
                GameObject mgrInstance = (GameObject)PrefabUtility.InstantiatePrefab(audioManagerPrefab);
                mgrInstance.name = "[AudioManager]";
                EditorSceneManager.MarkSceneDirty(scene);
            }

            // 2. Ensure Player in scene has audio components
            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                EnsureComponent<PlayerAudioDriver>(player.gameObject);
                EnsureComponent<ContinuousVelocityAudio>(player.gameObject);
                EnsureComponent<CollisionAudioEmitter>(player.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
            }

            // 3. Build & Wire Audio Settings UI Panel
            Canvas gameCanvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (gameCanvas != null)
            {
                BuildAndWireAudioSettings(gameCanvas.transform);
            }

            // 4. Ensure UI Buttons have UIButtonAudioHook
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            foreach (Button b in buttons)
            {
                EnsureComponent<UIButtonAudioHook>(b.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Re-open original scene
        if (!string.IsNullOrEmpty(currentScene) && File.Exists(currentScene))
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }
    }

    private static void BuildAndWireAudioSettings(Transform canvasRoot)
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Sprite blueBtn = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIPack}/Blue/Default/button_rectangle_depth_gradient.png");
        Sprite greyBtn = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIPack}/Grey/Default/button_rectangle_depth_gradient.png");
        Sprite yellowBtn = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIPack}/Yellow/Default/button_rectangle_depth_gradient.png");

        // 1. Find or create AudioSettingsPanel
        Transform existingPanel = canvasRoot.Find("AudioSettingsPanel");
        if (existingPanel != null)
        {
            UnityEngine.Object.DestroyImmediate(existingPanel.gameObject);
        }

        GameObject panelObj = new GameObject("AudioSettingsPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(AudioSettingsUI));
        panelObj.transform.SetParent(canvasRoot, false);
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        CanvasGroup cg = panelObj.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Dark background scrim
        GameObject scrimObj = new GameObject("Scrim", typeof(RectTransform), typeof(Image));
        scrimObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrimRt = scrimObj.GetComponent<RectTransform>();
        scrimRt.anchorMin = Vector2.zero;
        scrimRt.anchorMax = Vector2.one;
        scrimRt.sizeDelta = Vector2.zero;
        Image scrimImg = scrimObj.GetComponent<Image>();
        scrimImg.color = new Color(0.04f, 0.04f, 0.08f, 0.85f);

        // Card Modal Container
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image));
        cardObj.transform.SetParent(panelObj.transform, false);
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(620f, 540f);
        cardRt.anchoredPosition = Vector2.zero;
        Image cardImg = cardObj.GetComponent<Image>();
        cardImg.color = new Color(0.12f, 0.14f, 0.22f, 0.98f);

        // Header Ribbon
        GameObject ribbonObj = new GameObject("HeaderRibbon", typeof(RectTransform), typeof(Image));
        ribbonObj.transform.SetParent(cardObj.transform, false);
        RectTransform ribbonRt = ribbonObj.GetComponent<RectTransform>();
        ribbonRt.sizeDelta = new Vector2(380f, 52f);
        ribbonRt.anchoredPosition = new Vector2(0f, 270f);
        Image ribbonImg = ribbonObj.GetComponent<Image>();
        ribbonImg.sprite = blueBtn;
        ribbonImg.type = Image.Type.Sliced;

        TMP_Text headerText = CreateTMP(ribbonObj.transform, "Title", "AUDIO SETTINGS", font, 24f, Color.white, TextAlignmentOptions.Center);
        headerText.rectTransform.anchorMin = Vector2.zero;
        headerText.rectTransform.anchorMax = Vector2.one;
        headerText.rectTransform.sizeDelta = Vector2.zero;

        TMP_Text subText = CreateTMP(cardObj.transform, "Subtitle", "ADJUST SOUND LEVELS OR MUTE AUDIO", font, 15f, new Color(0.85f, 0.88f, 0.95f), TextAlignmentOptions.Center);
        subText.rectTransform.sizeDelta = new Vector2(560f, 26f);
        subText.rectTransform.anchoredPosition = new Vector2(0f, 215f);

        // Slider Rows
        (Slider masterSlider, TMP_Text masterValText, Button muteBtn, TMP_Text muteBtnText) = CreateSliderRowWithMute(cardObj.transform, "MasterRow", "MASTER VOLUME", 140f, font, blueBtn);
        (Slider bgmSlider, TMP_Text bgmValText) = CreateSliderRow(cardObj.transform, "BGMRow", "MUSIC (BGM)", 62f, font);
        (Slider sfxSlider, TMP_Text sfxValText) = CreateSliderRow(cardObj.transform, "SFXRow", "SOUND EFFECTS (SFX)", -16f, font);
        (Slider uiSlider, TMP_Text uiValText) = CreateSliderRow(cardObj.transform, "UIRow", "UI & INTERFACE", -94f, font);

        // Bottom Action Buttons
        Button defBtn = CreateButton(cardObj.transform, "DefaultsButton", "DEFAULTS", greyBtn, new Vector2(230f, 52f), new Vector2(-125f, -195f), font, 18f, Color.white);
        Button closeBtn = CreateButton(cardObj.transform, "CloseButton", "BACK", yellowBtn, new Vector2(230f, 52f), new Vector2(125f, -195f), font, 20f, new Color(0.15f, 0.12f, 0.05f));

        // Wire AudioSettingsUI serialized fields
        AudioSettingsUI settingsUI = panelObj.GetComponent<AudioSettingsUI>();
        SerializedObject sUI = new SerializedObject(settingsUI);
        sUI.Update();
        sUI.FindProperty("panelCanvasGroup").objectReferenceValue = cg;
        sUI.FindProperty("cardRect").objectReferenceValue = cardRt;
        sUI.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        sUI.FindProperty("masterValueText").objectReferenceValue = masterValText;
        sUI.FindProperty("muteButton").objectReferenceValue = muteBtn;
        sUI.FindProperty("muteButtonText").objectReferenceValue = muteBtnText;
        sUI.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
        sUI.FindProperty("bgmValueText").objectReferenceValue = bgmValText;
        sUI.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        sUI.FindProperty("sfxValueText").objectReferenceValue = sfxValText;
        sUI.FindProperty("uiSlider").objectReferenceValue = uiSlider;
        sUI.FindProperty("uiValueText").objectReferenceValue = uiValText;
        sUI.FindProperty("closeButton").objectReferenceValue = closeBtn;
        sUI.FindProperty("defaultsButton").objectReferenceValue = defBtn;
        sUI.ApplyModifiedProperties();

        panelObj.SetActive(false);

        // 2. Add Audio Settings button to PauseMenu Card
        Button pauseSettingsBtn = null;
        Transform pauseCard = canvasRoot.Find("PauseMenu/Card");
        if (pauseCard != null)
        {
            Transform existingBtn = pauseCard.Find("AudioSettingsButton");
            if (existingBtn != null) UnityEngine.Object.DestroyImmediate(existingBtn.gameObject);

            // Re-space PauseMenu buttons cleanly
            RectTransform cardRtPause = pauseCard.GetComponent<RectTransform>();
            cardRtPause.sizeDelta = new Vector2(600f, 550f);

            Transform resume = pauseCard.Find("ResumeButton");
            Transform restart = pauseCard.Find("RestartButton");
            Transform menu = pauseCard.Find("PauseMenuButton");

            if (resume != null) resume.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 75f);
            if (restart != null) restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 5f);
            if (menu != null) menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -135f);

            pauseSettingsBtn = CreateButton(pauseCard, "AudioSettingsButton", "AUDIO SETTINGS", blueBtn, new Vector2(380f, 56f), new Vector2(0f, -65f), font, 20f, Color.white);
        }

        // 3. Add Settings button to StartMenu Card
        Button mainSettingsBtn = null;
        Transform startCard = canvasRoot.Find("StartMenu/Card");
        if (startCard != null)
        {
            Transform existingBtn = startCard.Find("AudioSettingsButton");
            if (existingBtn != null) UnityEngine.Object.DestroyImmediate(existingBtn.gameObject);

            Transform startBtn = startCard.Find("StartButton");
            if (startBtn != null)
            {
                startBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-125f, -118f);
                startBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 64f);
            }

            mainSettingsBtn = CreateButton(startCard, "AudioSettingsButton", "SETTINGS", blueBtn, new Vector2(220f, 64f), new Vector2(145f, -118f), font, 20f, Color.white);
        }

        // 4. Wire into GameManager
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject gmSO = new SerializedObject(gm);
            gmSO.Update();
            if (pauseSettingsBtn != null) gmSO.FindProperty("pauseSettingsButton").objectReferenceValue = pauseSettingsBtn;
            if (mainSettingsBtn != null) gmSO.FindProperty("mainSettingsButton").objectReferenceValue = mainSettingsBtn;
            gmSO.FindProperty("audioSettingsPanel").objectReferenceValue = settingsUI;
            gmSO.ApplyModifiedProperties();
        }
    }

    private static (Slider, TMP_Text, Button, TMP_Text) CreateSliderRowWithMute(Transform parent, string name, string label, float posY, TMP_FontAsset font, Sprite btnSprite)
    {
        GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(530f, 66f);
        rowRt.anchoredPosition = new Vector2(0f, posY);
        Image rowImg = row.GetComponent<Image>();
        rowImg.color = new Color(0.16f, 0.18f, 0.28f, 0.90f);

        TMP_Text lbl = CreateTMP(row.transform, "Label", label, font, 16f, new Color(0.80f, 0.88f, 1f), TextAlignmentOptions.MidlineLeft);
        lbl.rectTransform.anchoredPosition = new Vector2(-120f, 16f);
        lbl.rectTransform.sizeDelta = new Vector2(240f, 24f);

        TMP_Text valTxt = CreateTMP(row.transform, "Val", "85%", font, 17f, Color.white, TextAlignmentOptions.MidlineRight);
        valTxt.rectTransform.anchoredPosition = new Vector2(85f, 16f);
        valTxt.rectTransform.sizeDelta = new Vector2(90f, 24f);

        Slider slider = CreateStyledSlider(row.transform, "Slider", new Vector2(360f, 18f), new Vector2(-55f, -14f), new Color(0.29f, 0.76f, 0.97f));

        Button muteBtn = CreateButton(row.transform, "MuteButton", "MUTE", btnSprite, new Vector2(92f, 44f), new Vector2(200f, 0f), font, 14f, Color.white);
        TMP_Text muteTxt = muteBtn.GetComponentInChildren<TMP_Text>();

        return (slider, valTxt, muteBtn, muteTxt);
    }

    private static (Slider, TMP_Text) CreateSliderRow(Transform parent, string name, string label, float posY, TMP_FontAsset font)
    {
        GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(530f, 64f);
        rowRt.anchoredPosition = new Vector2(0f, posY);
        Image rowImg = row.GetComponent<Image>();
        rowImg.color = new Color(0.16f, 0.18f, 0.28f, 0.90f);

        TMP_Text lbl = CreateTMP(row.transform, "Label", label, font, 16f, new Color(0.80f, 0.88f, 1f), TextAlignmentOptions.MidlineLeft);
        lbl.rectTransform.anchoredPosition = new Vector2(-70f, 15f);
        lbl.rectTransform.sizeDelta = new Vector2(340f, 24f);

        TMP_Text valTxt = CreateTMP(row.transform, "Val", "70%", font, 17f, Color.white, TextAlignmentOptions.MidlineRight);
        valTxt.rectTransform.anchoredPosition = new Vector2(200f, 15f);
        valTxt.rectTransform.sizeDelta = new Vector2(90f, 24f);

        Slider slider = CreateStyledSlider(row.transform, "Slider", new Vector2(470f, 18f), new Vector2(0f, -14f), new Color(0.29f, 0.76f, 0.97f));

        return (slider, valTxt);
    }

    private static Slider CreateStyledSlider(Transform parent, string name, Vector2 size, Vector2 pos, Color fillColor)
    {
        GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Background track
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.1f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.9f);
        fillAreaRt.offsetMin = new Vector2(2f, 0f);
        fillAreaRt.offsetMax = new Vector2(-2f, 0f);

        // Fill
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.color = fillColor;

        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(8f, 0f);
        handleAreaRt.offsetMax = new Vector2(-8f, 0f);

        // Handle
        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handleObj.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, size.y + 10f);
        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.color = Color.white;

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        return slider;
    }

    private static Button CreateButton(Transform parent, string name, string label, Sprite sprite, Vector2 size, Vector2 pos, TMP_FontAsset font, float fontSize, Color textColor)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAudioHook));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image img = btnObj.GetComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        img.color = Color.white;

        Button btn = btnObj.GetComponent<Button>();

        TMP_Text tmp = CreateTMP(btnObj.transform, "Text", label, font, fontSize, textColor, TextAlignmentOptions.Center);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.sizeDelta = Vector2.zero;

        return btn;
    }

    private static TMP_Text CreateTMP(Transform parent, string name, string text, TMP_FontAsset font, float fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }
}
