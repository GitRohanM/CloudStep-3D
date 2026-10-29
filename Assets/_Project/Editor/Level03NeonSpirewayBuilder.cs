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
/// Level 03 Builder: "Neon Spireway"
/// A nocturnal cosmic skyway featuring glowing crystal spires, moving ferry rafts,
/// precision vertical climbing, mystical twisted trees, starlight particle atmosphere,
/// and a complete 5-state UI flow with Lilita One SDF typography.
/// </summary>
public static class Level03NeonSpirewayBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/Level03_NeonSpireway.unity";
    private const string Root = "Assets/_Project";
    private const string NatureFBX = Root + "/Art/External/QuaterniusStylizedNature/FBX";
    private const string CloudPrefabPath = Root + "/Prefabs/Nature/CloudCluster.prefab";

    // UI Sprites & Font
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
    private static Sprite greyBtnSprite;
    private static Sprite greyBtnFlatSprite;
    private static Sprite blueBtnSprite;
    private static Sprite blueBtnFlatSprite;
    private static Sprite starSprite;

    // Materials
    private static Material obsidianMat;
    private static Material slateDeckMat;
    private static Material cyanTopMat;
    private static Material violetTopMat;
    private static Material raftCoreMat;
    private static Material neonRuneGlowMat;
    private static Material neonGemMat;
    private static Material neonBarkMat;
    private static Material neonFoliageMat;
    private static Material neonFernMat;
    private static Material neonMushroomMat;

    // [MenuItem("Tools/Platformer/Build Level03 Neon Spireway")]
    public static void BuildLevel03()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot build Level03 while in Play Mode. Stop playback first.");
            return;
        }

        // 1. Create or open scene
        Scene scene;
        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // Clean slate in the scene to prevent any duplicate or lingering objects
        foreach (GameObject oldRoot in scene.GetRootGameObjects())
        {
            Undo.DestroyObjectImmediate(oldRoot);
        }

        LoadVisualAssets();
        SetupMaterials();
        EnsurePlayerMaterials();

        // 2. Hierarchy Root Containers
        GameObject envRoot = CreateRootContainer("_Environment");
        GameObject platformsRoot = CreateRootContainer("_Platforms");
        GameObject propsRoot = CreateRootContainer("_Props");
        GameObject collectiblesRoot = CreateRootContainer("_Collectibles");
        GameObject gameplayRoot = CreateRootContainer("_Gameplay");
        GameObject uiRoot = CreateRootContainer("_UI");
        GameObject lightingRoot = CreateRootContainer("_Lighting");

        // 3. Look-Dev: Atmosphere, Lighting & Particles
        SetupAtmosphereAndLighting(lightingRoot.transform, envRoot.transform);

        // 4. Platforms & Moving Rafts
        BuildPlatformCourse(platformsRoot.transform);

        // 5. Props, Twisted Trees & Crystals
        DressEnvironment(propsRoot.transform);

        // 6. KillZone
        CreateKillZone(gameplayRoot.transform);

        // 7. Collectibles (18 Cosmic Coins)
        BuildCoins(collectiblesRoot.transform);

        // 8. Player & Camera
        PlayerController player = SetupGameplay(gameplayRoot.transform);

        // 9. UI Canvas & GameManager State Machine
        SetupUIAndGameManager(uiRoot.transform, gameplayRoot.transform, player);

        // 10. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=cyan><b>[Level03 Builder]</b> Successfully generated Level03_NeonSpireway with moving platforms, vertical ascension, and 5-state UI!</color>");
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

    #region Phase 1: Look-Dev, Lighting & Atmospheric Particles
    private static void LoadVisualAssets()
    {
        string pack = Root + "/Art/UI/KenneyUI/Pack/PNG";
        roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/panel_round.png");
        scorePanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/panel_inset.png");
        scoreBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/bar_round.png");
        coinBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/UI/CoinBadge.asset");

        yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Yellow/Default/button_rectangle_depth_gradient.png");
        yellowBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Yellow/Default/button_rectangle_flat.png");
        greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Green/Default/button_rectangle_depth_gradient.png");
        greenBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Green/Default/button_rectangle_flat.png");
        redBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Red/Default/button_rectangle_depth_gradient.png");
        redBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Red/Default/button_rectangle_flat.png");
        greyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/button_rectangle_depth_gradient.png");
        greyBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/button_rectangle_flat.png");
        blueBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Blue/Default/button_rectangle_depth_gradient.png");
        blueBtnFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Blue/Default/button_rectangle_flat.png");

        starSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Yellow/Default/star.png");
        fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Art/UI/LilitaOne SDF.asset");
    }

    private static void SetupMaterials()
    {
        obsidianMat = CreateOrLoadMaterial(Root + "/Materials/M_ObsidianBase.mat", new Color(0.10f, 0.12f, 0.18f), 0.35f, 0.15f);
        slateDeckMat = CreateOrLoadMaterial(Root + "/Materials/M_SlateDeck.mat", new Color(0.12f, 0.15f, 0.22f), 0.30f, 0.10f);
        cyanTopMat = CreateOrLoadMaterial(Root + "/Materials/M_CyanCrystalTop.mat", new Color(0.10f, 0.82f, 0.94f), 0.45f, 0.20f, new Color(0.08f, 0.45f, 0.55f));
        violetTopMat = CreateOrLoadMaterial(Root + "/Materials/M_VioletCrystalTop.mat", new Color(0.65f, 0.25f, 0.92f), 0.45f, 0.20f, new Color(0.35f, 0.12f, 0.50f));
        raftCoreMat = CreateOrLoadMaterial(Root + "/Materials/M_TealRaftCore.mat", new Color(0.14f, 0.18f, 0.26f), 0.60f, 0.35f);
        neonRuneGlowMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonRuneGlow.mat", new Color(0.12f, 0.98f, 0.85f), 0.85f, 0.1f, new Color(0.25f, 1.6f, 1.3f));
        neonGemMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonGemCrystal.mat", new Color(0.48f, 0.22f, 0.98f), 0.90f, 0.25f, new Color(0.75f, 0.35f, 1.8f));
        neonBarkMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonBark.mat", new Color(0.11f, 0.10f, 0.15f), 0.2f, 0.0f);
        neonFoliageMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonFoliage.mat", new Color(0.65f, 0.22f, 0.92f), 0.35f, 0.0f, new Color(0.20f, 0.05f, 0.35f));
        neonFernMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonFern.mat", new Color(0.06f, 0.90f, 0.75f), 0.45f, 0.0f);
        neonMushroomMat = CreateOrLoadMaterial(Root + "/Materials/M_NeonMushroom.mat", new Color(1.0f, 0.55f, 0.15f), 0.7f, 0.0f, new Color(0.9f, 0.45f, 0.1f));
    }

    private static void EnsurePlayerMaterials()
    {
        Shader simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (simpleLit == null) return;
        string[] matPaths = new string[] {
            Root + "/Materials/M_Player_Blue.mat",
            Root + "/Materials/M_Player_Peach.mat",
            Root + "/Materials/M_Player_Tummy.mat",
            Root + "/Materials/M_Player_Eye.mat",
            Root + "/Materials/M_Player_White.mat",
            Root + "/Materials/M_Player_Black.mat"
        };
        foreach (var p in matPaths)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null && m.shader != simpleLit)
            {
                Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;
                m.shader = simpleLit;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                EditorUtility.SetDirty(m);
            }
        }
    }

    private static Material CreateOrLoadMaterial(string path, Color albedo, float smoothness, float metallic, Color? emission = null)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(litShader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", albedo);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);

        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission.Value);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void SetupAtmosphereAndLighting(Transform lightingRoot, Transform envRoot)
    {
        // 1. Cool Luminous Celestial Moon / Starlight Directional Light
        GameObject moonObj = new GameObject("Celestial_Moon_Light", typeof(Light));
        moonObj.transform.SetParent(lightingRoot);
        moonObj.transform.position = new Vector3(0f, 40f, 0f);
        moonObj.transform.rotation = Quaternion.Euler(45f, -60f, 0f);

        Light moon = moonObj.GetComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.75f, 0.90f, 1.0f);
        moon.intensity = 1.15f;
        moon.shadows = LightShadows.Soft;
        moon.shadowStrength = 0.88f;

        // 2. Cosmic Midnight Procedural Skybox
        string skyPath = Root + "/Materials/M_NeonSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
        if (skyMat == null)
        {
            Shader skyShader = Shader.Find("Skybox/Procedural");
            skyMat = new Material(skyShader);
            skyMat.name = "M_NeonSkybox";
            AssetDatabase.CreateAsset(skyMat, skyPath);
        }
        skyMat.SetFloat("_SunDisk", 2f);
        skyMat.SetFloat("_SunSize", 0.035f);
        skyMat.SetFloat("_SunSizeConvergence", 5.5f);
        skyMat.SetFloat("_AtmosphereThickness", 0.52f);
        skyMat.SetColor("_SkyTint", new Color(0.08f, 0.11f, 0.24f)); // Deep nocturnal cosmic indigo
        skyMat.SetColor("_GroundColor", new Color(0.04f, 0.03f, 0.10f)); // Nocturnal purple abyss
        skyMat.SetFloat("_Exposure", 1.15f);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;
        RenderSettings.sun = moon;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.24f, 0.48f, 0.78f);
        RenderSettings.ambientEquatorColor = new Color(0.44f, 0.22f, 0.65f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.06f, 0.14f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.08f, 0.06f, 0.16f);
        RenderSettings.fogDensity = 0.0035f; // Ultra-subtle so celestial bodies and starfield shine brilliantly

        // 3. Spectacular Giant Celestial Moon with Planetary Rings
        BuildCelestialMoonAndRings(envRoot);

        // 4. Twinkling Cosmic Starfield (1200 Stars)
        BuildStarfield(envRoot);

        // 5. Shooting Stars / Comets Streaking Across the Sky
        BuildShootingStars(envRoot);

        // 6. Volumetric Puffy 3D Clouds in the Skyway
        GameObject cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CloudPrefabPath);
        if (cloudPrefab != null)
        {
            Vector3[] skyCloudPos =
            {
                new Vector3(-35f, 16f, 25f), new Vector3(38f, 18f, 40f),
                new Vector3(-32f, 24f, 75f), new Vector3(34f, 26f, 92f),
                new Vector3(-22f, -6f, 48f), new Vector3(26f, -8f, 62f)
            };
            Vector3[] skyCloudScales =
            {
                new Vector3(5.2f, 3.6f, 5.2f), new Vector3(6.0f, 4.0f, 5.6f),
                new Vector3(6.4f, 4.4f, 6.0f), new Vector3(5.5f, 3.8f, 5.2f),
                new Vector3(7.5f, 4.2f, 7.5f), new Vector3(7.0f, 4.0f, 7.0f)
            };

            for (int i = 0; i < skyCloudPos.Length; i++)
            {
                GameObject cloud = (GameObject)PrefabUtility.InstantiatePrefab(cloudPrefab, envRoot);
                cloud.name = $"Sky_Cloud_{i + 1:00}";
                cloud.transform.position = skyCloudPos[i];
                cloud.transform.rotation = Quaternion.Euler(0f, (i * 58f) + 15f, 0f);
                cloud.transform.localScale = skyCloudScales[i];
            }
        }

        // 7. Ambient Starlight Mote / Firefly Particle System
        GameObject fxObj = new GameObject("FX_StarlightMotes", typeof(ParticleSystem));
        fxObj.transform.SetParent(envRoot, false);
        fxObj.transform.position = new Vector3(0f, 6f, 45f);

        ParticleSystem ps = fxObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 6f;
        main.startSpeed = 0.4f;
        main.startSize = 0.22f;
        main.maxParticles = 80;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 0.95f, 0.85f, 0.85f), new Color(0.75f, 0.35f, 1f, 0.85f));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(35f, 18f, 90f);

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        Material particleMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_NeonParticle.mat");
        if (particleMat == null)
        {
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            particleMat = new Material(pShader);
            AssetDatabase.CreateAsset(particleMat, Root + "/Materials/M_NeonParticle.mat");
        }
        particleMat.SetColor("_BaseColor", new Color(0.25f, 0.98f, 0.88f, 0.85f));
        EditorUtility.SetDirty(particleMat);

        ParticleSystemRenderer psRenderer = fxObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.material = particleMat;
    }

    private static void BuildCelestialMoonAndRings(Transform envRoot)
    {
        GameObject moonRoot = new GameObject("Celestial_Moon_System");
        moonRoot.transform.SetParent(envRoot, false);
        // Positioned along line of sight directly behind and above the zenith summit
        moonRoot.transform.position = new Vector3(0f, 28f, 130f);

        // A) Glowing Moon Sphere (Colossal Celestial Orb)
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Moon_Sphere";
        sphere.transform.SetParent(moonRoot.transform, false);
        sphere.transform.localScale = Vector3.one * 36f;
        Object.DestroyImmediate(sphere.GetComponent<Collider>());

        Material moonMat = CreateOrLoadMaterial(Root + "/Materials/M_CelestialMoon.mat", new Color(0.94f, 0.97f, 1.0f), 0.95f, 0.1f, new Color(0.90f, 1.50f, 2.20f));
        sphere.GetComponent<MeshRenderer>().sharedMaterial = moonMat;

        // B) Inner Cosmic Planetary Ring (Radiant Cyan Glow)
        GameObject innerRing = new GameObject("Ring_Inner", typeof(MeshFilter), typeof(MeshRenderer));
        innerRing.transform.SetParent(moonRoot.transform, false);
        innerRing.transform.localRotation = Quaternion.Euler(22f, -32f, 16f);
        innerRing.GetComponent<MeshFilter>().sharedMesh = CreateRingMesh(24f, 36f, 96);
        Material innerRingMat = CreateOrLoadMaterial(Root + "/Materials/M_CosmicRing_Cyan.mat", new Color(0.20f, 0.90f, 1.0f, 0.85f), 0.1f, 0.0f, new Color(0.40f, 1.50f, 2.20f));
        innerRingMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        innerRing.GetComponent<MeshRenderer>().sharedMaterial = innerRingMat;

        // C) Outer Cosmic Planetary Ring (Ethereal Violet Glow)
        GameObject outerRing = new GameObject("Ring_Outer", typeof(MeshFilter), typeof(MeshRenderer));
        outerRing.transform.SetParent(moonRoot.transform, false);
        outerRing.transform.localRotation = Quaternion.Euler(22f, -32f, 16f);
        outerRing.GetComponent<MeshFilter>().sharedMesh = CreateRingMesh(39f, 52f, 96);
        Material outerRingMat = CreateOrLoadMaterial(Root + "/Materials/M_CosmicRing_Violet.mat", new Color(0.75f, 0.35f, 1.0f, 0.75f), 0.1f, 0.0f, new Color(1.20f, 0.50f, 2.00f));
        outerRingMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        outerRing.GetComponent<MeshRenderer>().sharedMaterial = outerRingMat;

        // D) Ambient Moonlight Aura (Soft Point Light)
        GameObject auraLight = new GameObject("Moon_AuraLight", typeof(Light));
        auraLight.transform.SetParent(moonRoot.transform, false);
        Light l = auraLight.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.55f, 0.85f, 1.0f);
        l.range = 140f;
        l.intensity = 2.0f;
    }

    private static void BuildStarfield(Transform envRoot)
    {
        GameObject starsObj = new GameObject("FX_CosmicStarfield", typeof(ParticleSystem));
        starsObj.transform.SetParent(envRoot, false);
        starsObj.transform.position = new Vector3(0f, 20f, 60f);

        ParticleSystem ps = starsObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 10000f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 1.0f);
        main.maxParticles = 1600;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.95f), new Color(0.45f, 0.88f, 1f, 0.95f));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 260f;
        shape.radiusThickness = 0f; // Only outer celestial shell - zero stars near platforms!

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1600) });

        Material pMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_NeonParticle.mat");
        ParticleSystemRenderer rend = starsObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.material = pMat;
    }

    private static void BuildShootingStars(Transform envRoot)
    {
        GameObject cometObj = new GameObject("FX_ShootingStars", typeof(ParticleSystem));
        cometObj.transform.SetParent(envRoot, false);
        cometObj.transform.position = new Vector3(45f, 48f, 95f);

        ParticleSystem ps = cometObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 1.4f;
        main.startSpeed = 46f;
        main.startSize = 0.5f;
        main.maxParticles = 8;
        main.startColor = new Color(0.92f, 0.98f, 1f, 1f);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 10f, 20f);
        shape.rotation = new Vector3(35f, -120f, 0f);

        var emission = ps.emission;
        emission.rateOverTime = 0.5f;

        var trails = ps.trails;
        trails.enabled = true;
        trails.lifetime = 0.35f;
        trails.minVertexDistance = 0.2f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, 0f);

        Material pMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_NeonParticle.mat");
        ParticleSystemRenderer rend = cometObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.material = pMat;
        rend.trailMaterial = pMat;
    }

    private static Mesh CreateRingMesh(float innerRadius, float outerRadius, int segments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "CosmicRingMesh";
        // 2 sets of vertices (front & back) so normals do not cancel each other out
        Vector3[] vertices = new Vector3[segments * 4];
        Vector3[] normals = new Vector3[segments * 4];
        Vector2[] uvs = new Vector2[segments * 4];
        int[] triangles = new int[segments * 12];

        int vOffset = segments * 2;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 inner = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            Vector3 outer = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
            float u = (float)i / segments;

            // Front face vertices & normals
            vertices[i * 2] = inner;
            vertices[i * 2 + 1] = outer;
            normals[i * 2] = Vector3.up;
            normals[i * 2 + 1] = Vector3.up;
            uvs[i * 2] = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);

            // Back face vertices & normals
            vertices[vOffset + i * 2] = inner;
            vertices[vOffset + i * 2 + 1] = outer;
            normals[vOffset + i * 2] = Vector3.down;
            normals[vOffset + i * 2 + 1] = Vector3.down;
            uvs[vOffset + i * 2] = new Vector2(u, 0f);
            uvs[vOffset + i * 2 + 1] = new Vector2(u, 1f);

            int next = (i + 1) % segments;
            int t = i * 6;
            // Front face triangles
            triangles[t + 0] = i * 2;
            triangles[t + 1] = i * 2 + 1;
            triangles[t + 2] = next * 2;

            triangles[t + 3] = next * 2;
            triangles[t + 4] = i * 2 + 1;
            triangles[t + 5] = next * 2 + 1;

            // Back face triangles
            int tb = segments * 6 + i * 6;
            triangles[tb + 0] = vOffset + next * 2;
            triangles[tb + 1] = vOffset + i * 2 + 1;
            triangles[tb + 2] = vOffset + i * 2;

            triangles[tb + 3] = vOffset + next * 2 + 1;
            triangles[tb + 4] = vOffset + i * 2 + 1;
            triangles[tb + 5] = vOffset + next * 2;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
    #endregion

    #region Phase 2: Platform Course & Moving Rafts
    private static void BuildPlatformCourse(Transform parent)
    {
        // Platform 00: Starting Plaza ("Sanctuary of Awakening") - Dark slate walking surface with glowing cyan rim
        BuildFacetedPlatform(parent, "00_StartPlaza", new Vector3(0f, 0f, 0f), new Vector3(8f, 1.2f, 8f), slateDeckMat, obsidianMat, true);

        // Platform 01: Low Stepping Stones ("Starlight Steppers")
        BuildFacetedPlatform(parent, "01_Step_A", new Vector3(0f, 0.6f, 6.5f), new Vector3(2.5f, 1.0f, 2.5f), cyanTopMat, obsidianMat);
        BuildFacetedPlatform(parent, "01_Step_B", new Vector3(2.4f, 1.4f, 11f), new Vector3(2.4f, 1.0f, 2.4f), cyanTopMat, obsidianMat);
        BuildFacetedPlatform(parent, "01_Step_C", new Vector3(0f, 2.2f, 15.5f), new Vector3(2.5f, 1.0f, 2.5f), cyanTopMat, obsidianMat);

        // Platform 02: Launch Deck for Moving Raft 1
        BuildFacetedPlatform(parent, "02_RaftDeck_01", new Vector3(-1.2f, 3.0f, 21f), new Vector3(4.8f, 1.2f, 4.8f), slateDeckMat, obsidianMat, true);

        // Moving Raft 01: "The Void Ferry" (Travels along Z: 26 -> 36)
        BuildMovingRaft(parent, "03_MovingRaft_VoidFerry", new Vector3(0f, 3.0f, 26f), new Vector3(3.2f, 0.6f, 3.2f), new Vector3(0f, 0f, 10f), 3.2f, 0.4f);

        // Platform 03: Midpoint Observation Spire ("Violet Terrace")
        BuildFacetedPlatform(parent, "04_VioletTerrace", new Vector3(0f, 4.0f, 41f), new Vector3(6.5f, 1.4f, 6.5f), slateDeckMat, obsidianMat, true);

        // Moving Raft 02: "The Lateral Glider" (Travels along X: -5 -> +5 at Z = 49)
        BuildMovingRaft(parent, "05_MovingRaft_LateralGlider", new Vector3(-5f, 5.2f, 49f), new Vector3(3.2f, 0.6f, 3.2f), new Vector3(10f, 0f, 0f), 3.4f, 0.4f);

        // Platform 04: The Narrow Spire Stairway ("Pillars of Ascension")
        BuildFacetedPlatform(parent, "06_AscentSpire_01", new Vector3(6.0f, 6.4f, 56f), new Vector3(2.2f, 3.0f, 2.2f), violetTopMat, obsidianMat);
        BuildFacetedPlatform(parent, "06_AscentSpire_02", new Vector3(3.5f, 7.8f, 61f), new Vector3(2.0f, 4.5f, 2.0f), violetTopMat, obsidianMat);
        BuildFacetedPlatform(parent, "06_AscentSpire_03", new Vector3(-0.5f, 9.2f, 66f), new Vector3(2.0f, 5.8f, 2.0f), violetTopMat, obsidianMat);
        BuildFacetedPlatform(parent, "06_AscentSpire_04", new Vector3(-4.0f, 10.6f, 71f), new Vector3(2.2f, 7.2f, 2.2f), violetTopMat, obsidianMat);

        // Moving Raft 03: "The Vertical Lift" (Lifts player diagonally from 10.6m to 13.4m)
        BuildMovingRaft(parent, "07_MovingRaft_VerticalLift", new Vector3(-1.5f, 10.6f, 77f), new Vector3(3.2f, 0.6f, 3.2f), new Vector3(0f, 2.8f, 7f), 3.0f, 0.4f);

        // Platform 05: Zenith Citadel & Celestial Gateway ("The Astral Sanctuary")
        BuildFacetedPlatform(parent, "08_ZenithCitadel", new Vector3(0f, 14.0f, 95f), new Vector3(9.5f, 1.8f, 9.5f), slateDeckMat, obsidianMat, true);
    }

    private static GameObject BuildFacetedPlatform(Transform parent, string name, Vector3 pos, Vector3 size, Material topMat, Material baseMat, bool runeBorder = false)
    {
        GameObject root = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(root, "Create " + name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Base Rock Body
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "Obsidian_Base";
        baseObj.transform.SetParent(root.transform, false);
        baseObj.transform.localPosition = new Vector3(0f, -size.y * 0.5f, 0f);
        baseObj.transform.localScale = size;
        MeshRenderer baseR = baseObj.GetComponent<MeshRenderer>();
        baseR.sharedMaterial = baseMat;
        baseR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        baseR.receiveShadows = true;

        // Crystalline Deck Cap (top walking surface slightly elevated above the border)
        GameObject capObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        capObj.name = "Crystal_Deck";
        capObj.transform.SetParent(root.transform, false);
        capObj.transform.localPosition = new Vector3(0f, 0.010f, 0f);
        capObj.transform.localScale = new Vector3(size.x * 0.94f, 0.040f, size.z * 0.94f);
        MeshRenderer capR = capObj.GetComponent<MeshRenderer>();
        capR.sharedMaterial = topMat;
        capR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        capR.receiveShadows = true;

        // Remove redundant collider from cap so the main base box collider is sole ground
        Collider capCol = capObj.GetComponent<Collider>();
        if (capCol != null) Object.DestroyImmediate(capCol);

        // Box collider adjusted so the walking surface is exactly at Y = +0.030m
        BoxCollider boxCol = baseObj.GetComponent<BoxCollider>();
        boxCol.center = new Vector3(0f, 0.015f / size.y, 0f);
        boxCol.size = new Vector3(1f, 1f + (0.030f / size.y), 1f);

        // Glowing Rune Perimeter Trim (steps down slightly as a glowing border bevel)
        if (runeBorder)
        {
            GameObject borderObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            borderObj.name = "Rune_Border";
            borderObj.transform.SetParent(root.transform, false);
            borderObj.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            borderObj.transform.localScale = new Vector3(size.x * 0.985f, 0.030f, size.z * 0.985f);
            MeshRenderer borderR = borderObj.GetComponent<MeshRenderer>();
            borderR.sharedMaterial = neonRuneGlowMat;
            borderR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Collider bCol = borderObj.GetComponent<Collider>();
            if (bCol != null) Object.DestroyImmediate(bCol);
        }

        return root;
    }

    private static GameObject BuildMovingRaft(Transform parent, string name, Vector3 startPos, Vector3 size, Vector3 moveDelta, float duration, float wait)
    {
        GameObject raft = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(raft, "Create " + name);
        raft.transform.SetParent(parent, false);
        raft.transform.position = startPos;

        // MovingPlatform Controller Component
        MovingPlatform mp = raft.AddComponent<MovingPlatform>();
        mp.MoveDelta = moveDelta;
        mp.TravelDuration = duration;
        mp.WaitDuration = wait;

        // Solid Raft Base
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Raft_Body";
        body.transform.SetParent(raft.transform, false);
        body.transform.localPosition = new Vector3(0f, -size.y * 0.5f, 0f);
        body.transform.localScale = size;
        MeshRenderer bodyR = body.GetComponent<MeshRenderer>();
        bodyR.sharedMaterial = raftCoreMat;

        // Crystalline Floor Pad
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "Raft_Deck";
        pad.transform.SetParent(raft.transform, false);
        pad.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        pad.transform.localScale = new Vector3(size.x * 0.92f, 0.08f, size.z * 0.92f);
        MeshRenderer padR = pad.GetComponent<MeshRenderer>();
        padR.sharedMaterial = cyanTopMat;
        Collider padCol = pad.GetComponent<Collider>();
        if (padCol != null) Object.DestroyImmediate(padCol);

        // Glowing Neon Edge Indicator
        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rim.name = "Neon_Rim";
        rim.transform.SetParent(raft.transform, false);
        rim.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        rim.transform.localScale = new Vector3(size.x * 0.98f, 0.05f, size.z * 0.98f);
        MeshRenderer rimR = rim.GetComponent<MeshRenderer>();
        rimR.sharedMaterial = neonRuneGlowMat;
        Collider rimCol = rim.GetComponent<Collider>();
        if (rimCol != null) Object.DestroyImmediate(rimCol);

        // Player Carry Trigger volume sitting right above the deck
        BoxCollider trigger = raft.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.6f, 0f);
        trigger.size = new Vector3(size.x * 0.98f, 1.2f, size.z * 0.98f);

        // Point Light Underglow
        GameObject underglow = new GameObject("Raft_Underglow", typeof(Light));
        underglow.transform.SetParent(raft.transform, false);
        underglow.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        Light l = underglow.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.15f, 0.95f, 0.85f);
        l.range = 5f;
        l.intensity = 1.2f;
        l.shadows = LightShadows.None;

        return raft;
    }
    #endregion

    #region Phase 3: Props, Twisted Trees & Radiant Crystals
    private static void DressEnvironment(Transform parent)
    {
        // 1. Starting Plaza Architecture - 4 Symmetrical Celestial Crystal Pylons & Glowing Runic Sanctum
        PlaceCelestialPylon(parent, "Pylon_FL", new Vector3(-3.6f, 0f, 2.6f));
        PlaceCelestialPylon(parent, "Pylon_FR", new Vector3(3.6f, 0f, 2.6f));
        PlaceCelestialPylon(parent, "Pylon_BL", new Vector3(-3.6f, 0f, -3.2f));
        PlaceCelestialPylon(parent, "Pylon_BR", new Vector3(3.6f, 0f, -3.2f));

        // Central Glowing Runic Sanctum Pattern flush on the dark slate floor
        GameObject outerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        outerRing.name = "Sanctum_RunicCircle_Outer";
        outerRing.transform.SetParent(parent, false);
        outerRing.transform.position = new Vector3(0f, 0.033f, -2.0f);
        outerRing.transform.localScale = new Vector3(3.0f, 0.003f, 3.0f);
        outerRing.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;
        Collider oCol = outerRing.GetComponent<Collider>();
        if (oCol != null) Object.DestroyImmediate(oCol);

        GameObject innerDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerDisc.name = "Sanctum_RunicCircle_Inner";
        innerDisc.transform.SetParent(parent, false);
        innerDisc.transform.position = new Vector3(0f, 0.035f, -2.0f);
        innerDisc.transform.localScale = new Vector3(2.5f, 0.003f, 2.5f);
        innerDisc.GetComponent<MeshRenderer>().sharedMaterial = slateDeckMat;
        Collider iCol = innerDisc.GetComponent<Collider>();
        if (iCol != null) Object.DestroyImmediate(iCol);

        GameObject coreRune = GameObject.CreatePrimitive(PrimitiveType.Cube);
        coreRune.name = "Sanctum_CoreRune";
        coreRune.transform.SetParent(parent, false);
        coreRune.transform.position = new Vector3(0f, 0.037f, -2.0f);
        coreRune.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        coreRune.transform.localScale = new Vector3(1.2f, 0.003f, 1.2f);
        coreRune.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;
        Collider crCol = coreRune.GetComponent<Collider>();
        if (crCol != null) Object.DestroyImmediate(crCol);

        // Soft corner crystal clusters at the back corners of the start platform (safely out of view/path)
        PlaceCrystalCluster(parent, "Crystal_Start_BL", new Vector3(-3.4f, 0f, -3.2f), 0.75f);
        PlaceCrystalCluster(parent, "Crystal_Start_BR", new Vector3(3.4f, 0f, -3.2f), 0.75f);

        // 2. Launch Deck 01 Props
        PlaceCrystalCluster(parent, "Crystal_Deck1", new Vector3(-2.8f, 3.0f, 21.5f), 1.0f);

        // 3. Midpoint Violet Terrace Props - Elegant Obelisks
        PlaceCrystalObelisk(parent, "Midpoint_Obelisk_L", new Vector3(-3.6f, 4.0f, 42.5f));
        PlaceCrystalObelisk(parent, "Midpoint_Obelisk_R", new Vector3(3.6f, 4.0f, 42.5f));
        PlaceCrystalCluster(parent, "Crystal_Mid_A", new Vector3(3.4f, 4.0f, 39.5f), 1.0f);

        // 4. Zenith Citadel Props (Astral Portal & Obelisks)
        BuildAstralPortal(parent, new Vector3(0f, 14.0f, 98.5f));
        PlaceCrystalObelisk(parent, "Obelisk_FL", new Vector3(-4.2f, 14.0f, 91.5f));
        PlaceCrystalObelisk(parent, "Obelisk_FR", new Vector3(4.2f, 14.0f, 91.5f));
        PlaceCrystalObelisk(parent, "Obelisk_BL", new Vector3(-4.2f, 14.0f, 97.5f));
        PlaceCrystalObelisk(parent, "Obelisk_BR", new Vector3(4.2f, 14.0f, 97.5f));

        // Distant background framing trees at the far Summit behind the portal only
        PlaceTwistedTree(parent, "TwistedTree_4", new Vector3(-5.5f, 14.0f, 98f), 0.90f, -40f);
        PlaceTwistedTree(parent, "TwistedTree_5", new Vector3(5.5f, 14.0f, 98f), 0.85f, 50f);
    }

    private static void PlaceCelestialPylon(Transform parent, string name, Vector3 pos)
    {
        GameObject pylon = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(pylon, "Create " + name);
        pylon.transform.SetParent(parent, false);
        pylon.transform.position = pos;

        // Plinth Base
        GameObject baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseBox.name = "Plinth";
        baseBox.transform.SetParent(pylon.transform, false);
        baseBox.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        baseBox.transform.localScale = new Vector3(0.60f, 1.0f, 0.60f);
        baseBox.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;
        Collider bCol = baseBox.GetComponent<Collider>();
        if (bCol != null) Object.DestroyImmediate(bCol);

        // Decorative Rune Collar
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        collar.name = "RuneCollar";
        collar.transform.SetParent(pylon.transform, false);
        collar.transform.localPosition = new Vector3(0f, 1.04f, 0f);
        collar.transform.localScale = new Vector3(0.68f, 0.08f, 0.68f);
        collar.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;
        Collider cCol = collar.GetComponent<Collider>();
        if (cCol != null) Object.DestroyImmediate(cCol);

        // Floating Crystal Spire
        GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crystal.name = "FloatingCrystal";
        crystal.transform.SetParent(pylon.transform, false);
        crystal.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        crystal.transform.localScale = new Vector3(0.28f, 0.50f, 0.28f);
        crystal.transform.localRotation = Quaternion.Euler(12f, 25f, 8f);
        crystal.GetComponent<MeshRenderer>().sharedMaterial = neonGemMat;
        Collider crCol = crystal.GetComponent<Collider>();
        if (crCol != null) Object.DestroyImmediate(crCol);

        // Soft Cyan/Violet Point light
        GameObject lightObj = new GameObject("PylonGlow", typeof(Light));
        lightObj.transform.SetParent(pylon.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Light l = lightObj.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.40f, 0.90f, 1.0f);
        l.range = 3.5f;
        l.intensity = 0.75f;
    }

    private static void PlaceMushroomProp(Transform parent, Vector3 pos, float scale)
    {
        string fbxPath = $"{NatureFBX}/Mushroom_Common.fbx";
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx == null) return;

        GameObject shroom = (GameObject)PrefabUtility.InstantiatePrefab(fbx, parent);
        shroom.name = "NeonMushroom";
        shroom.transform.position = pos;
        shroom.transform.localScale = Vector3.one * scale;
        foreach (Renderer r in shroom.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = neonGemMat;
    }

    private static void PlaceTwistedTree(Transform parent, string modelName, Vector3 pos, float scale, float rotY)
    {
        string fbxPath = $"{NatureFBX}/{modelName}.fbx";
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx == null) return;

        GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(fbx, parent);
        tree.name = modelName + "_Instance";
        tree.transform.position = pos;
        tree.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        tree.transform.localScale = Vector3.one * scale;

        foreach (Renderer r in tree.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Contains("Leaves") || r.name.Contains("Foliage"))
                r.sharedMaterial = neonFoliageMat;
            else
                r.sharedMaterial = neonBarkMat;
        }
    }

    private static void PlaceFernProp(Transform parent, Vector3 pos, float scale)
    {
        string fbxPath = $"{NatureFBX}/Fern_1.fbx";
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx == null) return;

        GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(fbx, parent);
        fern.name = "NeonFern";
        fern.transform.position = pos;
        fern.transform.localScale = Vector3.one * scale;
        foreach (Renderer r in fern.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = neonFernMat;
    }

    private static void PlaceCrystalCluster(Transform parent, string name, Vector3 pos, float scale)
    {
        GameObject root = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(root, "Create " + name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        // Central Tall Crystal Spire
        CreateCrystalFacet(root.transform, "MainSpire", PrimitiveType.Cylinder, new Vector3(0f, 1.1f * scale, 0f), new Vector3(0.45f * scale, 1.1f * scale, 0.45f * scale), Quaternion.Euler(0f, 15f, 6f));
        // Flanking Shards
        CreateCrystalFacet(root.transform, "Shard_A", PrimitiveType.Cylinder, new Vector3(-0.35f * scale, 0.7f * scale, 0.2f * scale), new Vector3(0.35f * scale, 0.7f * scale, 0.35f * scale), Quaternion.Euler(-18f, 40f, -12f));
        CreateCrystalFacet(root.transform, "Shard_B", PrimitiveType.Cylinder, new Vector3(0.32f * scale, 0.6f * scale, -0.15f * scale), new Vector3(0.32f * scale, 0.6f * scale, 0.32f * scale), Quaternion.Euler(14f, -30f, 20f));

        // Soft Crystal Point Light
        GameObject lightObj = new GameObject("CrystalGlow", typeof(Light));
        lightObj.transform.SetParent(root.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 1.2f * scale, 0f);
        Light l = lightObj.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.65f, 0.35f, 1f);
        l.range = 4.5f * scale;
        l.intensity = 1.4f;
    }

    private static void CreateCrystalFacet(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 scale, Quaternion rot)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.transform.localRotation = rot;

        MeshRenderer r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = neonGemMat;
        Collider c = go.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);
    }

    private static void PlaceCrystalObelisk(Transform parent, string name, Vector3 pos)
    {
        GameObject ob = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(ob, "Create " + name);
        ob.transform.SetParent(parent, false);
        ob.transform.position = pos;

        // Base Plinth
        GameObject baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseBox.transform.SetParent(ob.transform, false);
        baseBox.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        baseBox.transform.localScale = new Vector3(0.9f, 0.8f, 0.9f);
        baseBox.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;

        // Pillar Shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(ob.transform, false);
        shaft.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        shaft.transform.localScale = new Vector3(0.55f, 1.4f, 0.55f);
        shaft.GetComponent<MeshRenderer>().sharedMaterial = cyanTopMat;

        // Floating Gem Finial
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.transform.SetParent(ob.transform, false);
        gem.transform.localPosition = new Vector3(0f, 4.0f, 0f);
        gem.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
        gem.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;

        GameObject lObj = new GameObject("Obelisk_Light", typeof(Light));
        lObj.transform.SetParent(ob.transform, false);
        lObj.transform.localPosition = new Vector3(0f, 4.2f, 0f);
        Light l = lObj.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.15f, 0.95f, 0.85f);
        l.range = 6f;
        l.intensity = 1.6f;
    }

    private static void BuildAstralPortal(Transform parent, Vector3 pos)
    {
        GameObject portalRoot = new GameObject("Celestial_Astral_Gateway");
        Undo.RegisterCreatedObjectUndo(portalRoot, "Create Astral Gateway");
        portalRoot.transform.SetParent(parent, false);
        portalRoot.transform.position = pos;

        // Gateway Pillars & Arch
        float dist = 2.4f;
        float height = 4.8f;
        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? -1f : 1f;
            Vector3 p = new Vector3(side * dist, height * 0.5f, 0f);
            GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
            col.transform.SetParent(portalRoot.transform, false);
            col.transform.localPosition = p;
            col.transform.localScale = new Vector3(0.85f, height, 0.85f);
            col.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;

            // Glowing Rune Stripe on Pillar
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.transform.SetParent(portalRoot.transform, false);
            stripe.transform.localPosition = p + new Vector3(0f, 0f, -0.44f);
            stripe.transform.localScale = new Vector3(0.25f, height * 0.9f, 0.05f);
            stripe.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
        }

        // Lintel Header Beam
        GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel.transform.SetParent(portalRoot.transform, false);
        lintel.transform.localPosition = new Vector3(0f, height + 0.35f, 0f);
        lintel.transform.localScale = new Vector3(dist * 2f + 1.6f, 0.7f, 1.1f);
        lintel.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;

        // Radiant Portal Curtain
        GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curtain.name = "Portal_EnergyCurtain";
        curtain.transform.SetParent(portalRoot.transform, false);
        curtain.transform.localPosition = new Vector3(0f, height * 0.48f, 0f);
        curtain.transform.localScale = new Vector3(dist * 1.8f, height * 0.92f, 0.12f);
        curtain.GetComponent<MeshRenderer>().sharedMaterial = neonRuneGlowMat;
        Collider curCol = curtain.GetComponent<Collider>();
        if (curCol != null) Object.DestroyImmediate(curCol);

        // FinishZone Trigger Box
        GameObject finishObj = new GameObject("FinishZone", typeof(BoxCollider), typeof(FinishZone));
        finishObj.transform.SetParent(portalRoot.transform, false);
        finishObj.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        BoxCollider finishCol = finishObj.GetComponent<BoxCollider>();
        finishCol.isTrigger = true;
        finishCol.size = new Vector3(dist * 1.9f, height, 2.0f);

        // Portal Beacon Light
        GameObject beacon = new GameObject("Portal_Beacon", typeof(Light));
        beacon.transform.SetParent(portalRoot.transform, false);
        beacon.transform.localPosition = new Vector3(0f, height * 0.5f, -0.6f);
        Light l = beacon.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.2f, 0.95f, 0.88f);
        l.range = 10f;
        l.intensity = 2.2f;
    }
    #endregion

    #region Phase 4: KillZone & Collectibles
    private static void CreateKillZone(Transform gameplayRoot)
    {
        GameObject killZoneObj = new GameObject("KillZone", typeof(BoxCollider), typeof(KillZone));
        Undo.RegisterCreatedObjectUndo(killZoneObj, "Create KillZone");
        killZoneObj.transform.SetParent(gameplayRoot, false);
        killZoneObj.transform.position = new Vector3(0f, -8f, 48f);

        BoxCollider col = killZoneObj.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(90f, 3.5f, 130f);
    }

    private static void BuildCoins(Transform parent)
    {
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Pickups/Coin.prefab");
        if (coinPrefab == null) return;

        // 18 Cosmic Coins placed with intentional jumping heights
        Vector3[] coinPositions =
        {
            // Section 1: Low Stepping Stones (3 coins)
            new Vector3(0f, 1.8f, 6.5f),
            new Vector3(2.4f, 2.6f, 11f),
            new Vector3(0f, 3.4f, 15.5f),

            // Section 2: Launch Deck to Void Ferry (3 coins along ferry track)
            new Vector3(-1.2f, 4.2f, 21f),
            new Vector3(0f, 4.2f, 28f),
            new Vector3(0f, 4.2f, 34f),

            // Section 3: Midpoint Violet Terrace (3 coins in circle)
            new Vector3(-1.4f, 5.2f, 41f),
            new Vector3(0f, 5.2f, 42.5f),
            new Vector3(1.4f, 5.2f, 41f),

            // Section 4: Lateral Glider Track (2 coins)
            new Vector3(-2.5f, 6.4f, 49f),
            new Vector3(2.5f, 6.4f, 49f),

            // Section 5: Ascension Spires (4 precision jump coins)
            new Vector3(6.0f, 7.8f, 56f),
            new Vector3(3.5f, 9.2f, 61f),
            new Vector3(-0.5f, 10.6f, 66f),
            new Vector3(-4.0f, 12.0f, 71f),

            // Section 6: Vertical Lift (1 coin along lift climb)
            new Vector3(-1.5f, 12.8f, 80f),

            // Section 7: Zenith Citadel Celebration Threshold (2 coins)
            new Vector3(-1.2f, 15.2f, 94.5f),
            new Vector3(1.2f, 15.2f, 94.5f)
        };

        for (int i = 0; i < coinPositions.Length; i++)
        {
            GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, parent);
            coin.name = $"Coin_{i + 1:00}";
            coin.transform.position = coinPositions[i];
            SphereCollider sc = coin.GetComponent<SphereCollider>();
            if (sc != null) sc.radius = 0.95f;
        }
    }
    #endregion

    #region Phase 5: Player, Camera & UI Flow
    private static PlayerController SetupGameplay(Transform gameplayRoot)
    {
        // 1. Player Spawn Point: Exact contact with platform floor at Y = +0.030
        GameObject spawnPoint = new GameObject("Player_SpawnPoint");
        Undo.RegisterCreatedObjectUndo(spawnPoint, "Create Player_SpawnPoint");
        spawnPoint.transform.SetParent(gameplayRoot, false);
        spawnPoint.transform.position = new Vector3(0f, 0.030f, -2.0f);
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

        // 3. Main Camera & Smooth Follow
        GameObject cameraObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        Undo.RegisterCreatedObjectUndo(cameraObj, "Create Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.SetParent(gameplayRoot, false);
        cameraObj.transform.position = new Vector3(0f, 3.8f, -7.5f);
        cameraObj.transform.rotation = Quaternion.Euler(13.5f, 0f, 0f);

        Camera cam = cameraObj.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 600f;

        CameraFollow camFollow = cameraObj.GetComponent<CameraFollow>();
        camFollow.Target = playerInstance.transform;

        SerializedObject camSerialized = new SerializedObject(camFollow);
        camSerialized.FindProperty("target").objectReferenceValue = playerInstance.transform;
        camSerialized.FindProperty("followDistance").floatValue = 6.4f;
        camSerialized.FindProperty("lookAtHeight").floatValue = 1.15f;
        camSerialized.FindProperty("smoothTime").floatValue = 0.12f;
        camSerialized.FindProperty("startingPitch").floatValue = 13.5f;
        camSerialized.ApplyModifiedPropertiesWithoutUndo();

        if (playerController != null)
        {
            SerializedObject pSerialized = new SerializedObject(playerController);
            pSerialized.FindProperty("cameraTransform").objectReferenceValue = cameraObj.transform;
            pSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        return playerController;
    }

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

        // E) Game Finished / Campaign Complete Menu
        CanvasGroup winGroup = CreateCanvasGroup("WinMenu", canvasObj.transform);
        (Button playAgainBtn, Button winMenuBtn, TMP_Text winScoreText) = BuildWinScreen(winGroup.transform);

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
        s.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn;
        s.FindProperty("nextLevelButton").objectReferenceValue = winMenuBtn;
        s.FindProperty("winMenuButton").objectReferenceValue = winMenuBtn;
        s.FindProperty("nextSceneName").stringValue = "Level01";

        s.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(gm);
    }

    private struct CoinTrackerRefs
    {
        public TMP_Text Score;
        public TMP_Text Total;
        public Image Progress;
        public Button PauseBtn;
    }

    private static CoinTrackerRefs BuildHUD(Transform parent)
    {
        // Top-Right Snug Coin Badge with Coin Icon placed on the RIGHT (100% consistent with Level 01 & 02)
        RectTransform card = CreateRect("CoinTracker", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(182f, 54f), new Vector2(-28f, -24f));
        Image cardBg = card.gameObject.AddComponent<Image>();
        cardBg.sprite = scoreBarSprite != null ? scoreBarSprite : roundedSprite;
        cardBg.type = Image.Type.Sliced;
        cardBg.color = new Color(0.12f, 0.16f, 0.25f, 0.95f);

        // 1. Score Text (Left)
        TMP_Text scoreText = CreateText(card, "ScoreText", "00", 30f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(18f, 0f), new Vector2(48f, 44f), true);
        scoreText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        scoreText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        scoreText.rectTransform.pivot = new Vector2(0f, 0.5f);

        // 2. Total Text (Middle) - Warm gold delimiter matching Level 01 & Level 02
        TMP_Text totalText = CreateText(card, "TotalText", "/ 18", 21f, new Color(1f, 0.85f, 0.45f), TextAlignmentOptions.MidlineLeft, new Vector2(66f, -2f), new Vector2(58f, 38f), false);
        totalText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        totalText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        totalText.rectTransform.pivot = new Vector2(0f, 0.5f);

        // 3. Gold Coin Icon (Right)
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

        return new CoinTrackerRefs { Score = scoreText, Total = totalText, Progress = null, PauseBtn = hudPauseBtn };
    }

    private static (Button, TMP_Text) BuildStartScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(720f, 540f));

        CreateHeaderRibbon(card, "NEON SPIREWAY", blueBtnSprite, Color.white, new Vector2(460f, 54f), 270f);

        CreateText(card, "Eyebrow", "LEVEL 03 - A CELESTIAL ASCENT", 15f, new Color(0.40f, 0.95f, 0.88f), TextAlignmentOptions.Center, new Vector2(0f, 195f), new Vector2(660f, 26f), true);
        CreateText(card, "Title", "SCALE THE CRYSTAL SPIRES", 33f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 148f), new Vector2(660f, 46f), true);
        CreateText(card, "Subtitle", "Ride the moving rafts and conquer the ascending spires to reach the Astral Gateway.", 15f, new Color(0.82f, 0.88f, 0.95f), TextAlignmentOptions.Center, new Vector2(0f, 105f), new Vector2(620f, 30f), false);

        CreateControlPill(card, "MovePill", "WASD / ARROWS", "MOVE", new Vector2(-160f, 32f));
        CreateControlPill(card, "JumpPill", "SPACE", "JUMP", new Vector2(160f, 32f));
        CreateControlPill(card, "LookPill", "MOUSE", "LOOK", new Vector2(-160f, -32f));
        CreateControlPill(card, "PausePill", "ESC / P", "PAUSE", new Vector2(160f, -32f));

        Button startBtn = CreateJuicyButton(card, "StartButton", "START RUN", greenBtnSprite, greenBtnFlatSprite, new Vector2(420f, 68f), new Vector2(0f, -118f), 28f, Color.white);
        TMP_Text totalCoinsHint = CreateText(card, "TotalCoinsHint", "18 COSMIC CRYSTAL COINS HIDDEN ALONG THE SPIRES", 14f, new Color(0.70f, 0.85f, 1f, 0.9f), TextAlignmentOptions.Center, new Vector2(0f, -195f), new Vector2(600f, 28f), false);

        return (startBtn, totalCoinsHint);
    }

    private static (Button, Button, Button) BuildPauseScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(580f, 470f));
        CreateHeaderRibbon(card, "GAME PAUSED", blueBtnSprite, Color.white, new Vector2(360f, 52f), 235f);

        CreateText(card, "PauseSubtitle", "The cosmic spires await your next leap.", 16f, new Color(0.85f, 0.90f, 0.98f), TextAlignmentOptions.Center, new Vector2(0f, 150f), new Vector2(500f, 32f), false);

        Button resumeBtn = CreateJuicyButton(card, "ResumeButton", "RESUME", greenBtnSprite, greenBtnFlatSprite, new Vector2(380f, 66f), new Vector2(0f, 60f), 26f, Color.white);
        Button restartBtn = CreateJuicyButton(card, "RestartButton", "RESTART LEVEL", yellowBtnSprite, yellowBtnFlatSprite, new Vector2(380f, 58f), new Vector2(0f, -22f), 22f, new Color(0.14f, 0.11f, 0.05f));
        Button menuBtn = CreateJuicyButton(card, "MenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(340f, 50f), new Vector2(0f, -98f), 19f, new Color(0.18f, 0.20f, 0.28f));

        return (resumeBtn, restartBtn, menuBtn);
    }

    private static (Button, Button, TMP_Text) BuildDeathScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(600f, 500f));
        CreateHeaderRibbon(card, "VOID FALL!", redBtnSprite, Color.white, new Vector2(340f, 52f), 250f);

        CreateText(card, "DeathSubtitle", "The abyss claimed you. Rise again and master the timing!", 16f, new Color(0.95f, 0.88f, 0.92f), TextAlignmentOptions.Center, new Vector2(0f, 165f), new Vector2(540f, 32f), false);

        RectTransform statsPlaque = CreateRect("StatsBox", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(390f, 74f), new Vector2(0f, 72f));
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

        CreateText(statsPlaque, "DeathScoreLabel", "COINS RECOVERED", 14f, new Color(1f, 0.72f, 0.76f), TextAlignmentOptions.MidlineLeft, new Vector2(72f, 12f), new Vector2(250f, 24f), true);
        TMP_Text deathScore = CreateText(statsPlaque, "DeathScoreText", "00 / 18", 28f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(72f, -12f), new Vector2(280f, 32f), true);

        Button retryBtn = CreateJuicyButton(card, "RetryButton", "TRY AGAIN", redBtnSprite, redBtnFlatSprite, new Vector2(400f, 66f), new Vector2(0f, -38f), 26f, Color.white);
        Button menuBtn = CreateJuicyButton(card, "DeathMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(360f, 52f), new Vector2(0f, -120f), 20f, new Color(0.18f, 0.20f, 0.28f));

        return (retryBtn, menuBtn, deathScore);
    }

    private static (Button, Button, TMP_Text) BuildWinScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(680f, 620f));
        CreateHeaderRibbon(card, "YOU'VE FINISHED THE GAME!", yellowBtnSprite, new Color(0.14f, 0.11f, 0.05f), new Vector2(500f, 54f), 310f);

        CreateText(card, "WinEyebrow", "THE CELESTIAL ODYSSEY IS COMPLETE", 15f, new Color(0.48f, 0.95f, 0.98f), TextAlignmentOptions.Center, new Vector2(0f, 235f), new Vector2(620f, 24f), true);
        CreateText(card, "WinTitle", "CONGRATULATIONS!", 36f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 195f), new Vector2(620f, 44f), true);
        CreateText(card, "WinSubtitle", "You conquered all 3 levels and mastered the skies of Cloudstep!", 15f, new Color(0.85f, 0.92f, 0.98f), TextAlignmentOptions.Center, new Vector2(0f, 150f), new Vector2(620f, 32f), false);

        if (starSprite != null)
        {
            float[] starOffsets = { -75f, 0f, 75f };
            float[] starScales = { 46f, 58f, 46f };
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject("Star_" + i, typeof(RectTransform), typeof(Image));
                starObj.transform.SetParent(card, false);
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                starRect.anchoredPosition = new Vector2(starOffsets[i], 95f);
                starRect.sizeDelta = new Vector2(starScales[i], starScales[i]);
                Image starImg = starObj.GetComponent<Image>();
                starImg.sprite = starSprite;
                starImg.color = Color.white;
                starImg.preserveAspect = true;
            }
        }

        RectTransform statsPlaque = CreateRect("StatsBox", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 74f), new Vector2(0f, 15f));
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

        CreateText(statsPlaque, "WinScoreLabel", "FINAL LEVEL CRYSTAL COINS", 13f, new Color(0.48f, 0.94f, 0.78f), TextAlignmentOptions.MidlineLeft, new Vector2(72f, 12f), new Vector2(280f, 24f), true);
        TMP_Text winScore = CreateText(statsPlaque, "WinScoreText", "18 / 18", 28f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(72f, -12f), new Vector2(280f, 32f), true);

        // Action Buttons:
        // 1. Primary Action: MAIN MENU (Juicy Emerald Green, loads Level 01 Title Screen)
        Button menuBtn = CreateJuicyButton(card, "WinMenuButton", "MAIN MENU", greenBtnSprite, greenBtnFlatSprite, new Vector2(430f, 66f), new Vector2(0f, -80f), 26f, Color.white);

        // 2. Secondary Action: PLAY AGAIN (Sleek Slate Grey, restarts Level 03)
        Button playAgainBtn = CreateJuicyButton(card, "PlayAgainButton", "PLAY AGAIN", greyBtnSprite, greyBtnFlatSprite, new Vector2(360f, 52f), new Vector2(0f, -155f), 19f, new Color(0.18f, 0.20f, 0.28f));

        return (playAgainBtn, menuBtn, winScore);
    }

    private static RectTransform BuildModalCard(Transform screenRoot, Vector2 cardSize)
    {
        CreateScreenScrim(screenRoot, new Color(0.04f, 0.04f, 0.08f, 0.80f));
        RectTransform card = CreateRect("Card", screenRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cardSize, Vector2.zero);

        RectTransform shadow = CreateRect("Shadow", card, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        shadow.offsetMin = new Vector2(-12f, -18f);
        shadow.offsetMax = new Vector2(12f, 6f);
        Image shadowImg = shadow.gameObject.AddComponent<Image>();
        shadowImg.sprite = scorePanelSprite != null ? scorePanelSprite : roundedSprite;
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0.01f, 0.01f, 0.03f, 0.70f);
        shadow.SetAsFirstSibling();

        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.sprite = scorePanelSprite != null ? scorePanelSprite : roundedSprite;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.10f, 0.12f, 0.20f, 0.98f);

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
        bg.color = new Color(0.16f, 0.20f, 0.30f, 0.95f);

        CreateText(pill, "Keys", keys, 17f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 7f), new Vector2(280f, 22f), true);
        CreateText(pill, "Action", action, 12f, new Color(0.40f, 0.95f, 0.88f), TextAlignmentOptions.Center, new Vector2(0f, -11f), new Vector2(280f, 16f), false);
    }

    private static Button CreateJuicyButton(RectTransform parent, string name, string label, Sprite normalSprite, Sprite pressedSprite, Vector2 size, Vector2 pos, float fontSize = 24f, Color? textColor = null)
    {
        RectTransform btnRect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos);
        Image btnImg = btnRect.gameObject.AddComponent<Image>();
        btnImg.sprite = normalSprite != null ? normalSprite : roundedSprite;
        btnImg.type = Image.Type.Sliced;
        btnImg.color = Color.white;

        Button btn = btnRect.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        if (pressedSprite != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = new SpriteState();
            ss.highlightedSprite = normalSprite;
            ss.pressedSprite = pressedSprite;
            ss.selectedSprite = normalSprite;
            btn.spriteState = ss;
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
        img.color = new Color(color.r, color.g, color.b, 0.50f);
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
