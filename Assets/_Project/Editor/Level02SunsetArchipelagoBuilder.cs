using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Deterministic, production-quality builder for Level02_SunsetArchipelago.
/// Generates the complete 3D stylized platformer level with vibrant sunset look-dev,
/// strict hierarchy architecture, responsive gameplay, and complete 5-state UI flow.
/// </summary>
public static class Level02SunsetArchipelagoBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/Level02_SunsetArchipelago.unity";
    private const string Root = "Assets/_Project";
    private const string NatureFBX = Root + "/Art/External/QuaterniusStylizedNature/FBX";
    private const string CloudPrefabPath = Root + "/Prefabs/Nature/CloudCluster.prefab";
    private const string NaturePrefabs = Root + "/Prefabs/Nature";
    private const string PlatformPrefabs = Root + "/Prefabs/Level01Generated";

    private static Sprite roundedSprite;
    private static TMP_FontAsset fontAsset;
    private static Sprite scorePanelSprite;
    private static Sprite scoreBarSprite;
    private static Sprite coinBadgeSprite;
    private static Sprite yellowBtnSprite;
    private static Sprite yellowBtnFlatSprite;
    private static Sprite greenBtnSprite;
    private static Sprite greenBtnFlatSprite;
    private static Sprite redBtnSprite;
    private static Sprite redBtnFlatSprite;
    private static Sprite blueBtnSprite;
    private static Sprite blueBtnFlatSprite;
    private static Sprite greyBtnSprite;
    private static Sprite greyBtnFlatSprite;
    private static Sprite starSprite;

    // [MenuItem("Tools/Platformer/Build Level02 Sunset Archipelago")]
    public static void BuildLevel02()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play Mode before building Level02.");
            return;
        }

        // Ensure target scene is active
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.path != ScenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                currentScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("Scene build canceled by user.");
                return;
            }
        }

        roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Art/UI/LilitaOne SDF.asset");
        if (fontAsset == null)
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Art/UI/KenneyUI/KenneyFuture SDF.asset");
        scorePanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/UI/ScorePanel.asset");
        scoreBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/UI/ScoreBar.asset");
        coinBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/UI/CoinBadge.asset");

        string uiPack = Root + "/Art/UI/KenneyUI/Pack/PNG";
        yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Yellow/Default/button_rectangle_depth_gradient.png");
        yellowBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Yellow/Default/button_rectangle_flat.png");
        greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Green/Default/button_rectangle_depth_gradient.png");
        greenBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Green/Default/button_rectangle_flat.png");
        redBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Red/Default/button_rectangle_depth_gradient.png");
        redBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Red/Default/button_rectangle_flat.png");
        blueBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Blue/Default/button_rectangle_depth_gradient.png");
        blueBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Blue/Default/button_rectangle_flat.png");
        greyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Grey/Default/button_rectangle_depth_gradient.png");
        greyBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Grey/Default/button_rectangle_flat.png");
        starSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPack + "/Yellow/Default/star.png");

        // Clear existing scene contents safely
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            Undo.DestroyObjectImmediate(rootObjects[i]);
        }

        // Strict containers
        Transform lightingRoot = CreateRootContainer("_Lighting").transform;
        Transform envRoot = CreateRootContainer("_Environment").transform;
        Transform platformsRoot = CreateRootContainer("_Platforms").transform;
        Transform propsRoot = CreateRootContainer("_Props").transform;
        Transform collectiblesRoot = CreateRootContainer("_Collectibles").transform;
        Transform gameplayRoot = CreateRootContainer("_Gameplay").transform;
        Transform uiRoot = CreateRootContainer("_UI").transform;

        // Phase 1: Look-Dev & Atmosphere
        SetupAtmosphereAndLighting(lightingRoot, envRoot);

        // Phase 2: Platform Architecture & Level Flow
        BuildCoursePlatforms(platformsRoot, propsRoot);

        // Phase 3: Core Gameplay (Player, Camera, Coins, DeathZone, GoalZone)
        PlayerController player = SetupGameplay(gameplayRoot, collectiblesRoot);

        // Phase 4: Complete Stylized UI Flow & Game Manager State Machine
        SetupUIAndGameManager(uiRoot, gameplayRoot, player);

        // Save & dirty scene
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=green><b>[Level02 Builder]</b> Successfully generated Level02_SunsetArchipelago with complete visuals, layout, gameplay, and 5-state UI!</color>");
    }

    private static GameObject CreateRootContainer(string name)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    #region Phase 1: Look-Dev & Atmosphere
    private static void SetupAtmosphereAndLighting(Transform lightingRoot, Transform envRoot)
    {
        // Directional Light
        GameObject sunObj = new GameObject("Directional Light", typeof(Light));
        sunObj.transform.SetParent(lightingRoot);
        sunObj.transform.position = new Vector3(0f, 40f, 0f);
        sunObj.transform.rotation = Quaternion.Euler(30f, -44f, 0f);

        Light sun = sunObj.GetComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.91f, 0.76f); // Warm golden amber
        sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.85f;

        // Sunset Procedural Skybox Material
        string matPath = Root + "/Materials/M_SunsetSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (skyMat == null)
        {
            Shader skyShader = Shader.Find("Skybox/Procedural");
            skyMat = new Material(skyShader);
            skyMat.name = "M_SunsetSkybox";
            AssetDatabase.CreateAsset(skyMat, matPath);
        }
        skyMat.SetFloat("_SunDisk", 2f); // High quality
        skyMat.SetFloat("_SunSize", 0.045f);
        skyMat.SetFloat("_SunSizeConvergence", 4.2f);
        skyMat.SetFloat("_AtmosphereThickness", 1.08f);
        skyMat.SetColor("_SkyTint", new Color(0.96f, 0.65f, 0.52f)); // Warm peach/rose
        skyMat.SetColor("_GroundColor", new Color(0.26f, 0.18f, 0.34f)); // Twilight purple-indigo
        skyMat.SetFloat("_Exposure", 1.22f);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.96f, 0.74f, 0.62f);
        RenderSettings.ambientEquatorColor = new Color(0.66f, 0.53f, 0.70f);
        RenderSettings.ambientGroundColor = new Color(0.28f, 0.22f, 0.36f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 120f;
        RenderSettings.fogColor = new Color(0.74f, 0.60f, 0.74f); // Twilight horizon haze

        // Atmospheric Floating Puffy 3D Cloud Clusters framing the skyway (clean abyss void below)
        GameObject cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CloudPrefabPath);
        if (cloudPrefab != null)
        {
            Vector3[] skyCloudPos =
            {
                new Vector3(-28f, 16f, 22f), new Vector3(32f, 18f, 38f),
                new Vector3(-26f, 22f, 65f), new Vector3(28f, 24f, 82f)
            };
            Vector3[] skyCloudScales =
            {
                new Vector3(4.5f, 3.2f, 4.5f), new Vector3(5.2f, 3.8f, 5.0f),
                new Vector3(5.8f, 4.0f, 5.5f), new Vector3(4.8f, 3.4f, 4.6f)
            };

            for (int i = 0; i < skyCloudPos.Length; i++)
            {
                GameObject cloud = (GameObject)PrefabUtility.InstantiatePrefab(cloudPrefab, envRoot);
                cloud.name = $"Cloud_Cluster_{i + 1:00}";
                cloud.transform.position = skyCloudPos[i];
                cloud.transform.rotation = Quaternion.Euler(0f, (i * 62f) + 20f, 0f);
                cloud.transform.localScale = skyCloudScales[i];
            }
        }
    }
    #endregion

    #region Phase 2: Platform Architecture & Course Flow
    private static void BuildCoursePlatforms(Transform platformsRoot, Transform propsRoot)
    {
        // Platform Prefabs
        GameObject p8x8Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefabs + "/Platform_08x08.prefab");
        GameObject p6x4Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefabs + "/Platform_06x04.prefab");
        GameObject p4x6Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefabs + "/Platform_04x06.prefab");
        GameObject p4x4Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefabs + "/Platform_04x04.prefab");

        // ---------------------------------------------------------------------------------
        // ZONE 1: Sunspire Courtyard (Starting Plaza)
        // ---------------------------------------------------------------------------------
        GameObject startPlaza = InstantiatePlatform(p8x8Prefab, "00_StartPlaza", new Vector3(0f, 0f, 0f), Quaternion.identity, platformsRoot);
        Transform startProps = CreateSubGroup("Props_00_StartPlaza", propsRoot);

        // Flanking Twisted Bonsai Pines
        PlaceNatureProp("TwistedTree_1", "Tree_Start_Left", startProps, new Vector3(-3.2f, 0f, 2.2f), Quaternion.Euler(0f, 45f, 0f), 0.28f);
        PlaceNatureProp("TwistedTree_3", "Tree_Start_Right", startProps, new Vector3(3.2f, 0f, 2.2f), Quaternion.Euler(0f, -40f, 0f), 0.26f);

        // Stone Pavers entrance path & garden borders
        PlaceNatureProp("RockPath_Square_Wide", "Paver_Center", startProps, new Vector3(0f, 0.02f, 0f), Quaternion.identity, 0.35f);
        PlaceNatureProp("Flower_3_Group", "Flowers_Start_Left", startProps, new Vector3(-2.8f, 0f, -1.8f), Quaternion.Euler(0f, 25f, 0f), 0.32f);
        PlaceNatureProp("Flower_4_Group", "Flowers_Start_Right", startProps, new Vector3(2.8f, 0f, -1.8f), Quaternion.Euler(0f, -30f, 0f), 0.32f);
        PlaceNatureProp("Rock_Medium_1", "Rock_Start_Left", startProps, new Vector3(-3.6f, 0f, -3.2f), Quaternion.Euler(0f, 70f, 0f), 0.25f);
        PlaceNatureProp("Rock_Medium_2", "Rock_Start_Right", startProps, new Vector3(3.6f, 0f, -3.2f), Quaternion.Euler(0f, -65f, 0f), 0.25f);

        // ---------------------------------------------------------------------------------
        // ZONE 2: Stepping-Stone Rapids (Ascending Curving Archipelago Eastward +X)
        // ---------------------------------------------------------------------------------
        // Platform 01: First hop (Y = 1.2m)
        GameObject step01 = InstantiatePlatform(p4x4Prefab, "01_SteppingStone_01", new Vector3(3.8f, 1.2f, 7.5f), Quaternion.Euler(0f, 15f, 0f), platformsRoot);
        Transform step01Props = CreateSubGroup("Props_01_Step01", propsRoot);
        PlaceNatureProp("Grass_Common_Tall", "Grass_Step01", step01Props, new Vector3(2.6f, 1.2f, 7.8f), Quaternion.Euler(0f, 15f, 0f), 0.45f);
        PlaceNatureProp("Flower_3_Single", "Flower_Step01", step01Props, new Vector3(4.8f, 1.2f, 6.8f), Quaternion.Euler(0f, -20f, 0f), 0.35f);

        // Platform 02: Second hop (Y = 2.4f)
        GameObject step02 = InstantiatePlatform(p4x4Prefab, "02_SteppingStone_02", new Vector3(8.5f, 2.4f, 14.2f), Quaternion.Euler(0f, 30f, 0f), platformsRoot);
        Transform step02Props = CreateSubGroup("Props_02_Step02", propsRoot);
        PlaceNatureProp("Rock_Medium_3", "Rock_Step02", step02Props, new Vector3(9.8f, 2.4f, 15.2f), Quaternion.Euler(0f, 110f, 0f), 0.22f);
        PlaceNatureProp("Clover_2", "Clover_Step02", step02Props, new Vector3(7.4f, 2.4f, 13.5f), Quaternion.Euler(0f, 40f, 0f), 0.45f);

        // Platform 03: Ascending Bridge (Y = 3.8f)
        GameObject step03 = InstantiatePlatform(p6x4Prefab, "03_SteppingStone_03", new Vector3(12.8f, 3.8f, 21.5f), Quaternion.Euler(0f, 20f, 0f), platformsRoot);
        Transform step03Props = CreateSubGroup("Props_03_Step03", propsRoot);
        PlaceNatureProp("PF_StylizedPine", "Pine_Step03", step03Props, new Vector3(15.2f, 3.8f, 22.8f), Quaternion.Euler(0f, -35f, 0f), 0.85f);
        PlaceNatureProp("Bush_Common_Flowers", "Bush_Step03", step03Props, new Vector3(10.8f, 3.8f, 20.2f), Quaternion.Euler(0f, 55f, 0f), 0.32f);
        PlaceNatureProp("Grass_Wispy_Tall", "Grass_Step03", step03Props, new Vector3(14.2f, 3.8f, 20.4f), Quaternion.Euler(0f, 10f, 0f), 0.42f);

        // ---------------------------------------------------------------------------------
        // ZONE 3: Midpoint Plateau (Scenic Canopy Overlook)
        // ---------------------------------------------------------------------------------
        GameObject plateau = InstantiatePlatform(p8x8Prefab, "04_MidpointPlateau", new Vector3(14.5f, 5.5f, 32.5f), Quaternion.Euler(0f, -10f, 0f), platformsRoot);
        Transform plateauProps = CreateSubGroup("Props_04_MidpointPlateau", propsRoot);

        // Grand pine and rock outcrop on the eastern cliff
        PlaceNatureProp("Pine_1", "Pine_Plateau_Back", plateauProps, new Vector3(17.8f, 5.5f, 35.8f), Quaternion.Euler(0f, -15f, 0f), 0.38f);
        PlaceNatureProp("Rock_Medium_1", "Rock_Plateau_Edge", plateauProps, new Vector3(18.2f, 5.5f, 30.5f), Quaternion.Euler(0f, 45f, 0f), 0.28f);
        PlaceNatureProp("Rock_Medium_2", "Rock_Plateau_Inner", plateauProps, new Vector3(11.2f, 5.5f, 35.5f), Quaternion.Euler(0f, -80f, 0f), 0.24f);

        // Scenic rest circle
        PlaceNatureProp("RockPath_Round_Wide", "StoneRestRing", plateauProps, new Vector3(14.5f, 5.52f, 32.5f), Quaternion.Euler(0f, 25f, 0f), 0.38f);
        PlaceNatureProp("Flower_4_Group", "Flowers_Plateau", plateauProps, new Vector3(11.8f, 5.5f, 30.2f), Quaternion.Euler(0f, 60f, 0f), 0.35f);
        PlaceNatureProp("Mushroom_Common", "Mushrooms_Plateau", plateauProps, new Vector3(17.2f, 5.5f, 32.8f), Quaternion.Euler(0f, 0f, 0f), 0.45f);

        // ---------------------------------------------------------------------------------
        // ZONE 4: Skyway Spire Crossing (Zigzag Ascent Westward -X)
        // ---------------------------------------------------------------------------------
        // Spire 01: Turn towards center (Y = 7.0m)
        GameObject spire01 = InstantiatePlatform(p4x4Prefab, "05_SkywaySpire_01", new Vector3(8.2f, 7.0f, 39.8f), Quaternion.Euler(0f, -25f, 0f), platformsRoot);
        Transform spire01Props = CreateSubGroup("Props_05_Spire01", propsRoot);
        PlaceNatureProp("Grass_Wispy_Tall", "Grass_Spire01", spire01Props, new Vector3(9.2f, 7.0f, 40.8f), Quaternion.Euler(0f, -25f, 0f), 0.42f);
        PlaceNatureProp("Fern_1", "Fern_Spire01", spire01Props, new Vector3(7.2f, 7.0f, 38.8f), Quaternion.Euler(0f, 50f, 0f), 0.55f);

        // Spire 02: Center void crossing (Y = 8.5m)
        GameObject spire02 = InstantiatePlatform(p4x4Prefab, "06_SkywaySpire_02", new Vector3(2.2f, 8.5f, 47.0f), Quaternion.Euler(0f, -15f, 0f), platformsRoot);
        Transform spire02Props = CreateSubGroup("Props_06_Spire02", propsRoot);
        PlaceNatureProp("Rock_Medium_3", "Rock_Spire02", spire02Props, new Vector3(3.2f, 8.5f, 47.8f), Quaternion.Euler(0f, 85f, 0f), 0.22f);
        PlaceNatureProp("Flower_3_Single", "Flower_Spire02", spire02Props, new Vector3(1.2f, 8.5f, 46.2f), Quaternion.Euler(0f, -15f, 0f), 0.35f);

        // Spire 03: High westward shelf (Y = 10.0m)
        GameObject spire03 = InstantiatePlatform(p6x4Prefab, "07_SkywaySpire_03", new Vector3(-3.5f, 10.0f, 54.5f), Quaternion.Euler(0f, 10f, 0f), platformsRoot);
        Transform spire03Props = CreateSubGroup("Props_07_Spire03", propsRoot);
        PlaceNatureProp("TwistedTree_2", "Tree_Spire03", spire03Props, new Vector3(-5.8f, 10.0f, 56.0f), Quaternion.Euler(0f, -70f, 0f), 0.25f);
        PlaceNatureProp("Bush_Common", "Bush_Spire03", spire03Props, new Vector3(-1.4f, 10.0f, 53.5f), Quaternion.Euler(0f, 35f, 0f), 0.32f);

        // Spire 04: Final launch platform towards summit (Y = 11.4m)
        GameObject spire04 = InstantiatePlatform(p4x4Prefab, "08_SkywaySpire_04", new Vector3(-1.8f, 11.4f, 62.5f), Quaternion.Euler(0f, 25f, 0f), platformsRoot);
        Transform spire04Props = CreateSubGroup("Props_08_Spire04", propsRoot);
        PlaceNatureProp("Grass_Wispy_Short", "Grass_Spire04", spire04Props, new Vector3(-2.8f, 11.4f, 63.2f), Quaternion.Euler(0f, 25f, 0f), 0.45f);
        PlaceNatureProp("Pebble_Square_3", "Pebble_Spire04", spire04Props, new Vector3(-0.8f, 11.4f, 61.8f), Quaternion.Euler(0f, -40f, 0f), 0.4f);

        // ---------------------------------------------------------------------------------
        // ZONE 5: The Grand Sky Sanctuary (Goal Summit)
        // ---------------------------------------------------------------------------------
        GameObject goalSanctuary = InstantiatePlatform(p8x8Prefab, "09_GoalSanctuary", new Vector3(0f, 12.8f, 73.0f), Quaternion.identity, platformsRoot);
        Transform sanctuaryProps = CreateSubGroup("Props_09_Sanctuary", propsRoot);

        // Flanking Grand Pines
        PlaceNatureProp("Pine_2", "Pine_Sanctuary_Left", sanctuaryProps, new Vector3(-3.2f, 12.8f, 75.8f), Quaternion.Euler(0f, 30f, 0f), 0.36f);
        PlaceNatureProp("Pine_4", "Pine_Sanctuary_Right", sanctuaryProps, new Vector3(3.2f, 12.8f, 75.8f), Quaternion.Euler(0f, -25f, 0f), 0.34f);

        // Corner stylized boulders
        PlaceNatureProp("Rock_Medium_1", "Rock_Sanctuary_L", sanctuaryProps, new Vector3(-3.6f, 12.8f, 69.8f), Quaternion.Euler(0f, 15f, 0f), 0.28f);
        PlaceNatureProp("Rock_Medium_2", "Rock_Sanctuary_R", sanctuaryProps, new Vector3(3.6f, 12.8f, 69.8f), Quaternion.Euler(0f, -45f, 0f), 0.28f);

        // Flower garden surrounding the sanctuary
        PlaceNatureProp("Flower_3_Group", "Flowers_Sanctuary_L", sanctuaryProps, new Vector3(-2.2f, 12.8f, 71.5f), Quaternion.Euler(0f, 45f, 0f), 0.32f);
        PlaceNatureProp("Flower_4_Group", "Flowers_Sanctuary_R", sanctuaryProps, new Vector3(2.2f, 12.8f, 71.5f), Quaternion.Euler(0f, -45f, 0f), 0.32f);

        // Ceremonial Stone Pathway leading to the Archway
        PlaceNatureProp("RockPath_Square_Wide", "Sanctuary_Path_1", sanctuaryProps, new Vector3(0f, 12.82f, 70.5f), Quaternion.identity, 0.35f);
        PlaceNatureProp("RockPath_Square_Wide", "Sanctuary_Path_2", sanctuaryProps, new Vector3(0f, 12.82f, 73.0f), Quaternion.identity, 0.35f);

        // The Grand Golden Sun Archway
        BuildGoldenSunArchway(sanctuaryProps, new Vector3(0f, 12.8f, 75.5f));
    }

    private static GameObject InstantiatePlatform(GameObject prefab, string name, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.position = pos;
        instance.transform.rotation = rot;
        return instance;
    }

    private static Transform CreateSubGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void PlaceNatureProp(string modelName, string instanceName, Transform parent, Vector3 pos, Quaternion rot, float scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NatureFBX + "/" + modelName + ".fbx");
        if (prefab == null)
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/" + modelName + ".prefab");
        if (prefab == null)
            return;

        GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        prop.name = instanceName;
        prop.transform.position = pos;
        prop.transform.rotation = rot;
        prop.transform.localScale = Vector3.one * scale;

        // Ensure props cast and receive soft shadows cleanly
        foreach (Renderer r in prop.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        // Remove extraneous colliders from nature dressing so the player never gets snagged
        foreach (Collider col in prop.GetComponentsInChildren<Collider>())
        {
            Undo.DestroyObjectImmediate(col);
        }
    }

    private static void BuildGoldenSunArchway(Transform parent, Vector3 archPosition)
    {
        GameObject archRoot = new GameObject("Ceremonial_Sun_Archway");
        Undo.RegisterCreatedObjectUndo(archRoot, "Create Golden Sun Archway");
        archRoot.transform.SetParent(parent, false);
        archRoot.transform.position = archPosition;

        Material goldMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalGold.mat");
        Material tealMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalTeal.mat");
        Material coralMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalCoral.mat");

        float columnDist = 2.2f;
        float columnHeight = 4.2f;

        // Twin Pillars
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            Vector3 colPos = new Vector3(side * columnDist, 0f, 0f);

            // Base plinth
            CreateArchPrimitive("Pillar_Base_" + i, PrimitiveType.Cube, archRoot.transform, colPos + new Vector3(0f, 0.25f, 0f), new Vector3(0.9f, 0.5f, 0.9f), goldMat);
            // Fluted Shaft
            CreateArchPrimitive("Pillar_Shaft_" + i, PrimitiveType.Cylinder, archRoot.transform, colPos + new Vector3(0f, columnHeight * 0.5f, 0f), new Vector3(0.55f, columnHeight * 0.5f, 0.55f), tealMat);
            // Capital
            CreateArchPrimitive("Pillar_Capital_" + i, PrimitiveType.Cube, archRoot.transform, colPos + new Vector3(0f, columnHeight + 0.15f, 0f), new Vector3(0.85f, 0.3f, 0.85f), goldMat);
            // Finial Sphere
            CreateArchPrimitive("Pillar_Orb_" + i, PrimitiveType.Sphere, archRoot.transform, colPos + new Vector3(0f, columnHeight + 0.5f, 0f), new Vector3(0.45f, 0.45f, 0.45f), coralMat);
        }

        // Cross Lintel Beam
        CreateArchPrimitive("Lintel_Main", PrimitiveType.Cube, archRoot.transform, new Vector3(0f, columnHeight + 0.15f, 0f), new Vector3(columnDist * 2f + 1.2f, 0.38f, 0.7f), tealMat);
        CreateArchPrimitive("Lintel_Crown", PrimitiveType.Cube, archRoot.transform, new Vector3(0f, columnHeight + 0.45f, 0f), new Vector3(columnDist * 2f + 0.8f, 0.22f, 0.8f), goldMat);

        // Center Sun Disc Emblem
        CreateArchPrimitive("Sun_Disc", PrimitiveType.Cylinder, archRoot.transform, new Vector3(0f, columnHeight + 0.9f, -0.05f), new Vector3(1.1f, 0.12f, 1.1f), goldMat, Quaternion.Euler(90f, 0f, 0f));
        CreateArchPrimitive("Sun_Core", PrimitiveType.Sphere, archRoot.transform, new Vector3(0f, columnHeight + 0.9f, -0.15f), new Vector3(0.5f, 0.5f, 0.5f), coralMat);

        // Glowing Portal Gateway Curtain & Beacon Light
        CreateArchPrimitive("Portal_Curtain", PrimitiveType.Cube, archRoot.transform, new Vector3(0f, columnHeight * 0.45f, 0f), new Vector3(columnDist * 1.6f, columnHeight * 0.85f, 0.08f), tealMat);

        GameObject beaconLightObj = new GameObject("Portal_Beacon_Light", typeof(Light));
        beaconLightObj.transform.SetParent(archRoot.transform, false);
        beaconLightObj.transform.localPosition = new Vector3(0f, columnHeight * 0.5f, -0.4f);
        Light beacon = beaconLightObj.GetComponent<Light>();
        beacon.type = LightType.Point;
        beacon.color = new Color(0.35f, 0.95f, 0.85f);
        beacon.range = 8.5f;
        beacon.intensity = 1.8f;
        beacon.shadows = LightShadows.None;
    }

    private static void CreateArchPrimitive(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Material mat, Quaternion? rot = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        if (rot.HasValue) go.transform.localRotation = rot.Value;

        if (mat != null)
        {
            MeshRenderer r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null) Undo.DestroyObjectImmediate(col);
    }
    #endregion

    #region Phase 3: Core Gameplay & Mechanics
    private static PlayerController SetupGameplay(Transform gameplayRoot, Transform collectiblesRoot)
    {
        // 1. Player Spawn Point (Feet flush on the ground platform at Y = 0.0)
        GameObject spawnPoint = new GameObject("Player_SpawnPoint");
        Undo.RegisterCreatedObjectUndo(spawnPoint, "Create Player_SpawnPoint");
        spawnPoint.transform.SetParent(gameplayRoot);
        spawnPoint.transform.position = new Vector3(0f, 0.0f, -1.8f);
        spawnPoint.transform.rotation = Quaternion.identity;

        // 2. Instantiate Player Prefab
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Player.prefab");
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, gameplayRoot);
        playerInstance.name = "Player";
        playerInstance.transform.position = spawnPoint.transform.position;
        playerInstance.transform.rotation = Quaternion.identity;

        Transform vis = playerInstance.transform.Find("CharacterVisual");
        if (vis != null) vis.localPosition = new Vector3(0f, 0.085f, 0f);

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        // 3. Main Camera & CameraFollow
        GameObject cameraObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        Undo.RegisterCreatedObjectUndo(cameraObj, "Create Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.SetParent(gameplayRoot);
        cameraObj.transform.position = new Vector3(0f, 4f, -7.5f);
        cameraObj.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        Camera cam = cameraObj.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 58f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;

        CameraFollow camFollow = cameraObj.GetComponent<CameraFollow>();
        camFollow.Target = playerInstance.transform;

        SerializedObject camSerialized = new SerializedObject(camFollow);
        camSerialized.FindProperty("target").objectReferenceValue = playerInstance.transform;
        camSerialized.FindProperty("followDistance").floatValue = 6.2f;
        camSerialized.FindProperty("lookAtHeight").floatValue = 1.15f;
        camSerialized.FindProperty("smoothTime").floatValue = 0.12f;
        camSerialized.FindProperty("startingPitch").floatValue = 18f;
        camSerialized.ApplyModifiedPropertiesWithoutUndo();

        // Wire player's camera reference
        if (playerController != null)
        {
            SerializedObject pSerialized = new SerializedObject(playerController);
            pSerialized.FindProperty("cameraTransform").objectReferenceValue = cameraObj.transform;
            pSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // 4. Fall Detection (KillZone Trigger beneath the course)
        GameObject killZoneObj = new GameObject("KillZone", typeof(BoxCollider), typeof(KillZone));
        Undo.RegisterCreatedObjectUndo(killZoneObj, "Create KillZone");
        killZoneObj.transform.SetParent(gameplayRoot);
        killZoneObj.transform.position = new Vector3(5f, -12f, 40f);

        BoxCollider killCollider = killZoneObj.GetComponent<BoxCollider>();
        killCollider.isTrigger = true;
        killCollider.size = new Vector3(140f, 4f, 160f);

        // 5. Level Complete Goal Zone (Inside Golden Sun Archway)
        GameObject goalObj = new GameObject("FinishZone", typeof(BoxCollider), typeof(FinishZone));
        Undo.RegisterCreatedObjectUndo(goalObj, "Create FinishZone");
        goalObj.transform.SetParent(gameplayRoot);
        goalObj.transform.position = new Vector3(0f, 14.8f, 75.5f);

        BoxCollider goalCollider = goalObj.GetComponent<BoxCollider>();
        goalCollider.isTrigger = true;
        goalCollider.size = new Vector3(3.6f, 3.8f, 1.2f);

        // 6. Collectible Coins (16 Coins Placed Along Course Jump Arcs)
        SpawnCourseCoins(collectiblesRoot);

        return playerController;
    }

    private static void SpawnCourseCoins(Transform collectiblesRoot)
    {
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Pickups/Coin.prefab");
        if (coinPrefab == null)
        {
            Debug.LogError("Coin prefab missing at " + Root + "/Prefabs/Pickups/Coin.prefab");
            return;
        }

        // 16 Intentional Coin Positions across the 5 zones, calibrated to player jump trajectories:
        Vector3[] coinPositions =
        {
            // Zone 1: Starting Plaza coins (ground level, collectible while running)
            new Vector3(-1.4f, 1.2f, 1.5f),
            new Vector3(1.4f, 1.2f, 1.5f),

            // Zone 2: Stepping-Stone Rapids (Arc jumps)
            new Vector3(2.0f, 2.5f, 5.8f),    // Jump arc from Plaza to Step 01
            new Vector3(3.8f, 2.4f, 7.5f),    // Over Step 01 surface
            new Vector3(6.2f, 3.6f, 11.5f),   // Jump arc from Step 01 to Step 02
            new Vector3(8.5f, 3.6f, 14.2f),   // Over Step 02 surface
            new Vector3(10.8f, 4.8f, 18.2f),  // Jump arc from Step 02 to Step 03
            new Vector3(12.8f, 5.0f, 21.5f),  // Over Step 03 bridge surface

            // Zone 3: Midpoint Plateau (Y = 5.5f)
            new Vector3(13.8f, 6.7f, 27.5f),  // Jump arc onto Plateau
            new Vector3(14.5f, 6.7f, 32.5f),  // Plateau rest area

            // Zone 4: Skyway Spires (Zigzag void leaps calibrated to jump apex)
            new Vector3(11.4f, 8.0f, 36.2f),  // Leap to Spire 01
            new Vector3(5.2f, 9.5f, 43.4f),   // Leap to Spire 02
            new Vector3(-0.6f, 11.0f, 50.8f), // Leap to Spire 03
            new Vector3(-2.6f, 12.3f, 58.5f), // Leap to Spire 04

            // Zone 5: Grand Sky Sanctuary (Goal Summit)
            new Vector3(-0.9f, 14.0f, 68.0f), // Apex leap onto Sanctuary plaza
            new Vector3(0.0f, 14.0f, 74.2f)   // In front of the Golden Sun Portal
        };

        for (int i = 0; i < coinPositions.Length; i++)
        {
            GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, collectiblesRoot);
            coin.name = $"Coin_{i + 1:00}";
            coin.transform.position = coinPositions[i];
            coin.transform.rotation = Quaternion.identity;
        }
    }
    #endregion

    #region Phase 4: Stylized UI Flow & Game Manager State Machine
    private static void SetupUIAndGameManager(Transform uiRoot, Transform gameplayRoot, PlayerController player)
    {
        // 1. EventSystem
        GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        esObj.transform.SetParent(uiRoot, false);
        InputSystemUIInputModule inputModule = esObj.GetComponent<InputSystemUIInputModule>();
        if (inputModule != null) inputModule.AssignDefaultActions();

        // 2. Canvas
        GameObject canvasObj = new GameObject("Canvas_GameUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas_GameUI");
        canvasObj.transform.SetParent(uiRoot, false);

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // 3. UI Screens
        // A) HUD
        CanvasGroup hudGroup = CreateCanvasGroup("GameplayHUD", canvasObj.transform);
        CoinTrackerRefs tracker = BuildHUD(hudGroup.transform);

        // B) Start Menu
        CanvasGroup startGroup = CreateCanvasGroup("StartMenu", canvasObj.transform);
        (Button startButton, TMP_Text startTotalHint) = BuildStartScreen(startGroup.transform);

        // C) Pause Menu
        CanvasGroup pauseGroup = CreateCanvasGroup("PauseMenu", canvasObj.transform);
        (Button resumeBtn, Button pauseRestartBtn, Button pauseMenuBtn) = BuildPauseScreen(pauseGroup.transform);

        // D) Game Over / Fall Menu
        CanvasGroup deathGroup = CreateCanvasGroup("DeathMenu", canvasObj.transform);
        (Button retryBtn, Button deathMenuBtn, TMP_Text deathScoreText) = BuildDeathScreen(deathGroup.transform);

        // E) Level Complete Menu
        CanvasGroup winGroup = CreateCanvasGroup("WinMenu", canvasObj.transform);
        (Button nextLevelBtn, Button playAgainBtn, Button winMenuBtn, TMP_Text winScoreText) = BuildWinScreen(winGroup.transform);

        // Initial Visibility
        SetCanvasState(hudGroup, false);
        SetCanvasState(startGroup, true);
        SetCanvasState(pauseGroup, false);
        SetCanvasState(deathGroup, false);
        SetCanvasState(winGroup, false);

        // 4. Central GameManager State Machine
        GameObject gmObj = new GameObject("GameManager", typeof(GameManager));
        Undo.RegisterCreatedObjectUndo(gmObj, "Create GameManager");
        gmObj.transform.SetParent(gameplayRoot, false);

        GameManager gm = gmObj.GetComponent<GameManager>();
        SerializedObject s = new SerializedObject(gm);

        s.FindProperty("player").objectReferenceValue = player;
        s.FindProperty("cameraFollow").objectReferenceValue = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;

        // Coin Tracker references
        s.FindProperty("scoreText").objectReferenceValue = tracker.Score;
        s.FindProperty("totalText").objectReferenceValue = tracker.Total;
        s.FindProperty("progressFill").objectReferenceValue = tracker.Progress;
        s.FindProperty("countPulseTarget").objectReferenceValue = tracker.Score.rectTransform;

        // CanvasGroup Screens
        s.FindProperty("gameplayHud").objectReferenceValue = hudGroup;
        s.FindProperty("mainMenu").objectReferenceValue = startGroup;
        s.FindProperty("pauseMenu").objectReferenceValue = pauseGroup;
        s.FindProperty("deathMenu").objectReferenceValue = deathGroup;
        s.FindProperty("winMenu").objectReferenceValue = winGroup;

        // Texts
        s.FindProperty("mainMenuTotalText").objectReferenceValue = startTotalHint;
        s.FindProperty("deathScoreText").objectReferenceValue = deathScoreText;
        s.FindProperty("winScoreText").objectReferenceValue = winScoreText;

        // Buttons
        s.FindProperty("startButton").objectReferenceValue = startButton;
        s.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
        s.FindProperty("pauseRestartButton").objectReferenceValue = pauseRestartBtn;
        s.FindProperty("pauseMenuButton").objectReferenceValue = pauseMenuBtn;
        s.FindProperty("hudPauseButton").objectReferenceValue = tracker.PauseBtn;
        s.FindProperty("retryButton").objectReferenceValue = retryBtn;
        s.FindProperty("deathMenuButton").objectReferenceValue = deathMenuBtn;
        s.FindProperty("nextLevelButton").objectReferenceValue = nextLevelBtn;
        s.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn;
        s.FindProperty("winMenuButton").objectReferenceValue = winMenuBtn;
        s.FindProperty("nextSceneName").stringValue = "Level03_NeonSpireway";

        s.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(gm);
    }

    private struct CoinTrackerRefs
    {
        public TMP_Text Score;
        public TMP_Text Total;
        public Image Progress;
        public Button PauseBtn;
        public TMP_Text MenuTotalHint;
    }

    private static CoinTrackerRefs BuildHUD(Transform parent)
    {
        // Top-Right Floating Coin Badge with Coin Icon placed on the RIGHT side and snug capsule width
        RectTransform card = CreateRect("CoinTracker", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(182f, 54f), new Vector2(-28f, -24f));
        Image cardBg = card.gameObject.AddComponent<Image>();
        cardBg.sprite = scoreBarSprite != null ? scoreBarSprite : roundedSprite;
        cardBg.type = Image.Type.Sliced;
        cardBg.color = new Color(0.12f, 0.15f, 0.24f, 0.95f);

        // 1. Score Text (Left side of capsule)
        TMP_Text scoreText = CreateText(card, "ScoreText", "00", 30f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(18f, 0f), new Vector2(48f, 44f), true);
        scoreText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        scoreText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        scoreText.rectTransform.pivot = new Vector2(0f, 0.5f);

        // 2. Total Text (Middle of capsule)
        TMP_Text totalText = CreateText(card, "TotalText", "/ 16", 21f, new Color(1f, 0.85f, 0.45f), TextAlignmentOptions.MidlineLeft, new Vector2(66f, -2f), new Vector2(58f, 38f), false);
        totalText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        totalText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        totalText.rectTransform.pivot = new Vector2(0f, 0.5f);

        // 3. Gold Coin Icon (Right side of capsule)
        GameObject iconObj = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(card, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(128f, 0f);
        iconRect.sizeDelta = new Vector2(38f, 38f);
        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = coinBadgeSprite;
        icon.preserveAspect = true;

        // Top-Left Floating Pause Keycap Indicator Pill
        Button hudPauseBtn = CreateJuicyButton(parent as RectTransform, "HUD_PauseButton", "[ESC] PAUSE", greyBtnSprite, greyBtnFlatSprite, new Vector2(150f, 48f), new Vector2(28f, -24f), 16f, new Color(0.18f, 0.20f, 0.28f));
        hudPauseBtn.image.rectTransform.anchorMin = new Vector2(0f, 1f);
        hudPauseBtn.image.rectTransform.anchorMax = new Vector2(0f, 1f);
        hudPauseBtn.image.rectTransform.pivot = new Vector2(0f, 1f);
        hudPauseBtn.image.rectTransform.anchoredPosition = new Vector2(28f, -24f);

        return new CoinTrackerRefs { Score = scoreText, Total = totalText, Progress = null, PauseBtn = hudPauseBtn, MenuTotalHint = null };
    }

    private static (Button, TMP_Text) BuildStartScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(720f, 540f));

        // Top Ribbon Header
        CreateHeaderRibbon(card, "SUNSET ARCHIPELAGO", yellowBtnSprite, new Color(0.14f, 0.11f, 0.05f), new Vector2(460f, 54f), 270f);

        CreateText(card, "Eyebrow", "A CELESTIAL SKYWAY RUN", 15f, new Color(1f, 0.82f, 0.45f), TextAlignmentOptions.Center, new Vector2(0f, 195f), new Vector2(660f, 26f), true);
        CreateText(card, "Title", "ASCEND THE SPIRES", 34f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 148f), new Vector2(660f, 46f), true);
        CreateText(card, "Subtitle", "Leap across floating islands to reach the Sun Sanctuary.", 15f, new Color(0.82f, 0.86f, 0.94f), TextAlignmentOptions.Center, new Vector2(0f, 105f), new Vector2(620f, 30f), false);

        // 4 Sleek Control Chips in a 2x2 grid (PC Keyboard bindings)
        CreateControlPill(card, "MovePill", "WASD / ARROWS", "MOVE", new Vector2(-160f, 32f));
        CreateControlPill(card, "JumpPill", "SPACE", "JUMP", new Vector2(160f, 32f));
        CreateControlPill(card, "LookPill", "MOUSE", "LOOK", new Vector2(-160f, -32f));
        CreateControlPill(card, "PausePill", "ESC / P", "PAUSE", new Vector2(160f, -32f));

        // Start Button (tactile yellow depth button)
        Button startBtn = CreateJuicyButton(card, "StartButton", "START RUN", yellowBtnSprite, yellowBtnFlatSprite, new Vector2(420f, 68f), new Vector2(0f, -118f), 28f, new Color(0.14f, 0.11f, 0.05f));

        // Bottom Coin Count Plaque
        TMP_Text totalCoinsHint = CreateText(card, "TotalCoinsHint", "16 SUNSET COINS HIDDEN ALONG THE HIGHWAY", 14f, new Color(0.95f, 0.85f, 0.65f, 0.88f), TextAlignmentOptions.Center, new Vector2(0f, -195f), new Vector2(600f, 28f), false);

        return (startBtn, totalCoinsHint);
    }

    private static (Button, Button, Button) BuildPauseScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(580f, 470f));

        // Top Ribbon Header
        CreateHeaderRibbon(card, "GAME PAUSED", blueBtnSprite, Color.white, new Vector2(360f, 52f), 235f);

        CreateText(card, "PauseSubtitle", "The archipelago rests with you. Catch your breath.", 16f, new Color(0.85f, 0.90f, 0.96f), TextAlignmentOptions.Center, new Vector2(0f, 145f), new Vector2(520f, 32f), false);

        // Buttons
        Button resumeBtn = CreateJuicyButton(card, "ResumeButton", "RESUME", greenBtnSprite, greenBtnFlatSprite, new Vector2(380f, 66f), new Vector2(0f, 45f), 26f, Color.white);
        Button restartBtn = CreateJuicyButton(card, "PauseRestartButton", "RESTART LEVEL", yellowBtnSprite, yellowBtnFlatSprite, new Vector2(380f, 60f), new Vector2(0f, -38f), 22f, new Color(0.14f, 0.11f, 0.05f));
        Button menuBtn = CreateJuicyButton(card, "PauseMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(380f, 56f), new Vector2(0f, -118f), 20f, new Color(0.18f, 0.20f, 0.28f));

        return (resumeBtn, restartBtn, menuBtn);
    }

    private static (Button, Button, TMP_Text) BuildDeathScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(620f, 510f));

        // Top Ribbon Header
        CreateHeaderRibbon(card, "YOU FELL!", redBtnSprite, Color.white, new Vector2(340f, 52f), 255f);

        CreateText(card, "DeathSubtitle", "The clouds broke your fall. The spires are still waiting!", 16f, new Color(0.94f, 0.88f, 0.90f), TextAlignmentOptions.Center, new Vector2(0f, 168f), new Vector2(540f, 32f), false);

        // Stats Plaque Container
        RectTransform statsPlaque = CreateRect("StatsBox", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(390f, 74f), new Vector2(0f, 75f));
        Image plaqueBg = statsPlaque.gameObject.AddComponent<Image>();
        plaqueBg.sprite = scoreBarSprite != null ? scoreBarSprite : roundedSprite;
        plaqueBg.type = Image.Type.Sliced;
        plaqueBg.color = new Color(0.16f, 0.18f, 0.28f, 0.95f);

        // Coin Icon inside stats
        GameObject iconObj = new GameObject("StatsCoin", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(statsPlaque, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(16f, 0f);
        iconRect.sizeDelta = new Vector2(44f, 44f);
        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = coinBadgeSprite;
        icon.preserveAspect = true;

        CreateText(statsPlaque, "ScoreLabel", "COINS THIS RUN", 14f, new Color(1f, 0.84f, 0.55f), TextAlignmentOptions.MidlineLeft, new Vector2(72f, 12f), new Vector2(200f, 24f), true);
        TMP_Text deathScore = CreateText(statsPlaque, "DeathScoreText", "00 / 16", 28f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(72f, -12f), new Vector2(280f, 32f), true);

        // Buttons
        Button retryBtn = CreateJuicyButton(card, "RetryButton", "TRY AGAIN", redBtnSprite, redBtnFlatSprite, new Vector2(400f, 68f), new Vector2(0f, -35f), 26f, Color.white);
        Button menuBtn = CreateJuicyButton(card, "DeathMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(370f, 56f), new Vector2(0f, -120f), 20f, new Color(0.18f, 0.20f, 0.28f));

        return (retryBtn, menuBtn, deathScore);
    }

    private static (Button, Button, Button, TMP_Text) BuildWinScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(660f, 620f));

        // Top Ribbon Header
        CreateHeaderRibbon(card, "SUMMIT REACHED!", yellowBtnSprite, new Color(0.14f, 0.11f, 0.05f), new Vector2(440f, 54f), 310f);

        CreateText(card, "WinSubtitle", "You mastered the archipelago and reached the Sun Sanctuary!", 16f, new Color(0.88f, 0.95f, 0.92f), TextAlignmentOptions.Center, new Vector2(0f, 220f), new Vector2(600f, 32f), false);

        // 3 Victory Stars row
        if (starSprite != null)
        {
            float[] starOffsets = { -70f, 0f, 70f };
            float[] starScales = { 44f, 54f, 44f };
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject("Star_" + i, typeof(RectTransform), typeof(Image));
                starObj.transform.SetParent(card, false);
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                starRect.anchoredPosition = new Vector2(starOffsets[i], 155f);
                starRect.sizeDelta = new Vector2(starScales[i], starScales[i]);
                Image starImg = starObj.GetComponent<Image>();
                starImg.sprite = starSprite;
                starImg.color = Color.white;
                starImg.preserveAspect = true;
            }
        }

        // Stats Box
        RectTransform statsPlaque = CreateRect("StatsBox", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(410f, 74f), new Vector2(0f, 70f));
        Image plaqueBg = statsPlaque.gameObject.AddComponent<Image>();
        plaqueBg.sprite = scoreBarSprite != null ? scoreBarSprite : roundedSprite;
        plaqueBg.type = Image.Type.Sliced;
        plaqueBg.color = new Color(0.16f, 0.18f, 0.28f, 0.95f);

        GameObject iconObj = new GameObject("StatsCoin", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(statsPlaque, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(16f, 0f);
        iconRect.sizeDelta = new Vector2(44f, 44f);
        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = coinBadgeSprite;
        icon.preserveAspect = true;

        CreateText(statsPlaque, "WinScoreLabel", "FINAL COINS COLLECTED", 14f, new Color(0.48f, 0.94f, 0.78f), TextAlignmentOptions.MidlineLeft, new Vector2(72f, 12f), new Vector2(250f, 24f), true);
        TMP_Text winScore = CreateText(statsPlaque, "WinScoreText", "16 / 16", 28f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(72f, -12f), new Vector2(280f, 32f), true);

        // 3 Action Buttons: NEXT LEVEL (emerald), PLAY AGAIN (yellow depth), MAIN MENU (grey)
        Button nextLevelBtn = CreateJuicyButton(card, "NextLevelButton", "NEXT LEVEL", greenBtnSprite, greenBtnFlatSprite, new Vector2(430f, 66f), new Vector2(0f, -38f), 26f, Color.white);
        Button playAgainBtn = CreateJuicyButton(card, "PlayAgainButton", "PLAY AGAIN", yellowBtnSprite, yellowBtnFlatSprite, new Vector2(400f, 56f), new Vector2(0f, -114f), 22f, new Color(0.14f, 0.11f, 0.05f));
        Button menuBtn = CreateJuicyButton(card, "WinMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(360f, 50f), new Vector2(0f, -182f), 19f, new Color(0.18f, 0.20f, 0.28f));

        return (nextLevelBtn, playAgainBtn, menuBtn, winScore);
    }

    private static RectTransform BuildModalCard(Transform screenRoot, Vector2 cardSize)
    {
        // Dark Vignette Backdrop Scrim
        CreateScreenScrim(screenRoot, new Color(0.04f, 0.04f, 0.08f, 0.78f));

        // Main Modal Card
        RectTransform card = CreateRect("Card", screenRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cardSize, Vector2.zero);

        // Soft Drop Shadow Underlay (as child of Card so it scales with it)
        RectTransform shadow = CreateRect("Shadow", card, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        shadow.offsetMin = new Vector2(-12f, -18f);
        shadow.offsetMax = new Vector2(12f, 6f);
        Image shadowImg = shadow.gameObject.AddComponent<Image>();
        shadowImg.sprite = scorePanelSprite != null ? scorePanelSprite : roundedSprite;
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0.01f, 0.01f, 0.03f, 0.70f);
        shadow.SetAsFirstSibling();

        // Card Body Image
        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.sprite = scorePanelSprite != null ? scorePanelSprite : roundedSprite;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.12f, 0.14f, 0.22f, 0.98f);

        return card;
    }

    private static void CreateHeaderRibbon(RectTransform card, string text, Sprite ribbonSprite, Color textColor, Vector2 size, float posY)
    {
        RectTransform ribbon = CreateRect("HeaderRibbon", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, new Vector2(0f, posY));
        Image ribbonImg = ribbon.gameObject.AddComponent<Image>();
        ribbonImg.sprite = ribbonSprite != null ? ribbonSprite : roundedSprite;
        ribbonImg.type = Image.Type.Sliced;
        ribbonImg.color = Color.white;

        TMP_Text tmp = CreateText(ribbon, "Text", text, 22f, textColor, TextAlignmentOptions.Center, new Vector2(0f, 2f), size, true);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.sizeDelta = Vector2.zero;
    }

    private static void CreateControlPill(RectTransform card, string name, string keys, string action, Vector2 pos)
    {
        RectTransform pill = CreateRect(name, card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 52f), pos);
        Image bg = pill.gameObject.AddComponent<Image>();
        bg.sprite = scoreBarSprite != null ? scoreBarSprite : roundedSprite;
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.18f, 0.21f, 0.32f, 0.95f);

        CreateText(pill, "Keys", keys, 17f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 7f), new Vector2(280f, 22f), true);
        CreateText(pill, "Action", action, 12f, new Color(1f, 0.82f, 0.45f), TextAlignmentOptions.Center, new Vector2(0f, -11f), new Vector2(280f, 16f), false);
    }

    private static Button CreateJuicyButton(RectTransform parent, string name, string label, Sprite normalSprite, Sprite pressedSprite, Vector2 size, Vector2 pos, float fontSize = 24f, Color? textColor = null)
    {
        RectTransform btnRect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos);
        Image img = btnRect.gameObject.AddComponent<Image>();
        img.sprite = normalSprite != null ? normalSprite : roundedSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = btnRect.gameObject.AddComponent<Button>();
        if (pressedSprite != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState states = btn.spriteState;
            states.highlightedSprite = normalSprite;
            states.pressedSprite = pressedSprite;
            states.selectedSprite = normalSprite;
            btn.spriteState = states;
        }
        else
        {
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;
        }

        Color txtCol = textColor ?? new Color(0.12f, 0.12f, 0.16f);
        // Slightly offset text upwards by 2px because of button's 3D bottom bevel
        TMP_Text tmp = CreateText(btnRect, "Text", label, fontSize, txtCol, TextAlignmentOptions.Center, new Vector2(0f, 3f), size, true);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.sizeDelta = Vector2.zero;

        return btn;
    }

    private static void CreateScreenScrim(Transform parent, Color color)
    {
        RectTransform scrim = CreateRect("Scrim", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        scrim.offsetMin = Vector2.zero;
        scrim.offsetMax = Vector2.zero;
        Image img = scrim.gameObject.AddComponent<Image>();
        if (roundedSprite != null) img.sprite = roundedSprite;
        img.color = new Color(color.r, color.g, color.b, 0.45f);
    }

    private static TMP_Text CreateText(Transform parent, string name, string textContent, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 pos, Vector2 size, bool bold)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = textContent;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        return rect;
    }

    private static CanvasGroup CreateCanvasGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return go.GetComponent<CanvasGroup>();
    }

    private static void SetCanvasState(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.gameObject.SetActive(visible);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
    #endregion
}
