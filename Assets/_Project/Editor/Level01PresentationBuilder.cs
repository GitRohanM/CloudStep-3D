using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Editor-only, repeatable presentation pass for Level01.</summary>
public static class Level01PresentationBuilder
{
    private const string Root = "Assets/_Project";
    private const string MeshFolder = Root + "/Art/Generated/Meshes";
    private const string NatureFolder = Root + "/Prefabs/Nature";
    private const string QuaterniusNatureFolder = Root + "/Art/External/QuaterniusStylizedNature/FBX";
    private const string QuaterniusCloudPath = Root + "/Art/External/QuaterniusCloud/Cloud4.fbx";
    private static Sprite roundedSprite;

    private struct CoinTrackerRefs
    {
        public TMP_Text Score;
        public TMP_Text Total;
        public Image Progress;
    }

    // [MenuItem("Tools/Platformer/Rebuild Level01 Presentation")]
    public static void RebuildPresentation()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play Mode before rebuilding Level01 presentation.");
            return;
        }

        RebuildArt();
        RebuildUI();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Level01 presentation rebuilt: Quaternius nature set, floating-island details, cloud layers, and menus/HUD.");
    }

    // [MenuItem("Tools/Platformer/Rebuild Level01 Environment Art")]
    public static void RebuildEnvironmentArt()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play Mode before rebuilding Level01 environment art.");
            return;
        }

        RebuildArt();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Level01 environment rebuilt with imported Quaternius trees, plants, rocks, and clouds.");
    }

    private static void RebuildArt()
    {
        GameObject dressing = GameObject.Find("Environment_Dressing");
        if (dressing == null)
        {
            dressing = new GameObject("Environment_Dressing");
            Undo.RegisterCreatedObjectUndo(dressing, "Create environment dressing root");
        }

        // Remove only generated dressing objects; gameplay islands, pickups and route stay untouched.
        for (int i = dressing.transform.childCount - 1; i >= 0; i--)
        {
            string childName = dressing.transform.GetChild(i).name;
            if (childName.StartsWith("Pine_") || childName.StartsWith("Broadleaf_") ||
                childName.StartsWith("Blossom_") || childName.StartsWith("FlowerPatch_") ||
                childName.StartsWith("Quaternius_"))
                Undo.DestroyObjectImmediate(dressing.transform.GetChild(i).gameObject);
        }

        PlaceNatureModel(dressing.transform, "CommonTree_1", "Quaternius_Tree_Start_Maple", "Start_Island", -3.05f, 1.75f, -10f, 0.34f, true);
        PlaceNatureModel(dressing.transform, "Pine_1", "Quaternius_Tree_Start_Pine", "Start_Island", 3.05f, 1.75f, 14f, 0.35f, true);
        PlaceNatureModel(dressing.transform, "CommonTree_3", "Quaternius_Tree_Island02", "Island_02", -1.35f, 1.95f, 20f, 0.27f, true);
        PlaceNatureModel(dressing.transform, "Pine_4", "Quaternius_Tree_Island04", "Island_04", 2.15f, -0.55f, -18f, 0.25f, true);
        PlaceNatureModel(dressing.transform, "TwistedTree_2", "Quaternius_Tree_Island06", "Island_06", -1.3f, 1.55f, 18f, 0.16f, true);
        PlaceNatureModel(dressing.transform, "CommonTree_5", "Quaternius_Tree_Island09", "Island_09", 2.05f, -1.0f, -24f, 0.29f, true);
        PlaceNatureModel(dressing.transform, "Pine_2", "Quaternius_Tree_Finish_Pine", "End_Island", -2.85f, 2.8f, -12f, 0.34f, true);
        PlaceNatureModel(dressing.transform, "CommonTree_2", "Quaternius_Tree_Finish_Maple", "End_Island", 2.85f, 2.8f, 20f, 0.33f, true);

        // Low, collider-free plants sit at the island margins, leaving the coin and landing lanes open.
        PlaceNatureModel(dressing.transform, "Grass_Common_Tall", "Quaternius_Grass_Island01_Left", "Island_01", -1.3f, -1.15f, 18f, 0.46f, false);
        PlaceNatureModel(dressing.transform, "Plant_7_Big", "Quaternius_Plant_Island01_Right", "Island_01", 1.25f, 1.15f, -16f, 0.7f, false);
        PlaceNatureModel(dressing.transform, "Flower_3_Group", "Quaternius_Flowers_Island02_Right", "Island_02", 1.25f, -2.05f, 25f, 0.3f, false);
        PlaceNatureModel(dressing.transform, "Bush_Common_Flowers", "Quaternius_Bush_Island02_Right", "Island_02", 1.2f, 2.1f, -12f, 0.3f, false);
        PlaceNatureModel(dressing.transform, "Flower_4_Group", "Quaternius_Flowers_Island03_Left", "Island_03", -1.35f, -1.3f, 34f, 0.27f, false);
        PlaceNatureModel(dressing.transform, "Grass_Wispy_Tall", "Quaternius_Grass_Island04_Left", "Island_04", -2.35f, 1.15f, -20f, 0.43f, false);
        PlaceNatureModel(dressing.transform, "Clover_2", "Quaternius_Clover_Island05_Left", "Island_05", -1.35f, 1.35f, 26f, 0.45f, false);
        PlaceNatureModel(dressing.transform, "Mushroom_Common", "Quaternius_Mushroom_Island05_Right", "Island_05", 1.35f, -1.35f, 0f, 0.5f, false);
        PlaceNatureModel(dressing.transform, "Plant_7_Big", "Quaternius_Plant_Island06_Right", "Island_06", 1.25f, -2.1f, -18f, 0.72f, false);
        PlaceNatureModel(dressing.transform, "Flower_3_Group", "Quaternius_Flowers_Island07_Right", "Island_07", 1.3f, -1.3f, 20f, 0.28f, false);
        PlaceNatureModel(dressing.transform, "Grass_Common_Tall", "Quaternius_Grass_Island08_Left", "Island_08", -1.3f, -1.25f, -12f, 0.42f, false);
        PlaceNatureModel(dressing.transform, "Flower_4_Group", "Quaternius_Flowers_Island08_Right", "Island_08", 1.3f, 1.25f, 24f, 0.25f, false);
        PlaceNatureModel(dressing.transform, "Bush_Common_Flowers", "Quaternius_Bush_Island09_Left", "Island_09", -2.25f, 1.15f, -18f, 0.3f, false);
        PlaceNatureModel(dressing.transform, "Grass_Wispy_Tall", "Quaternius_Grass_Finish_Left", "End_Island", -3.25f, -2.9f, 12f, 0.42f, false);
        PlaceNatureModel(dressing.transform, "Flower_3_Group", "Quaternius_Flowers_Finish_Right", "End_Island", 3.2f, -2.8f, -24f, 0.28f, false);
        PlaceNatureModel(dressing.transform, "Fern_1", "Quaternius_Fern_Finish_Left", "End_Island", -3.35f, 0.35f, -20f, 0.3f, false);
        PlaceNatureModel(dressing.transform, "Bush_Common", "Quaternius_Bush_Finish_Right", "End_Island", 3.35f, 0.35f, 24f, 0.28f, false);

        RebuildFinishCelebration(dressing.transform);
        RebuildFloatingIslandRocks();
        RebuildQuaterniusClouds();
        CreateFinishTrigger();
    }

    private static void PlaceNatureModel(Transform parent, string modelName, string instanceName, string platformName,
        float xOffset, float zOffset, float yaw, float scale, bool castShadows)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuaterniusNatureFolder + "/" + modelName + ".fbx");
        GameObject instance = CreateImportedInstance(prefab, instanceName, parent);
        if (instance == null) return;

        Vector3 uniformScale = Vector3.one * scale;
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = uniformScale;
        instance.transform.rotation = rotation;
        Bounds bounds = GetImportedModelBounds(prefab);
        Vector3 surface = PlatformPoint(platformName, xOffset, zOffset);
        Vector3 scaledCenter = Vector3.Scale(bounds.center, uniformScale);
        Vector3 horizontalCenter = rotation * new Vector3(scaledCenter.x, 0f, scaledCenter.z);
        instance.transform.position = new Vector3(surface.x - horizontalCenter.x,
            surface.y - bounds.min.y * scale,
            surface.z - horizontalCenter.z);

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = castShadows;
        }
    }

    private static void RebuildFinishCelebration(Transform parent)
    {
        const string rootName = "Finish_Celebration";
        GameObject oldRoot = GameObject.Find(rootName);
        if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot);

        GameObject island = GameObject.Find("End_Island");
        BoxCollider platform = island != null ? island.GetComponent<BoxCollider>() : null;
        if (platform == null)
        {
            Debug.LogWarning("Finish celebration skipped because End_Island has no BoxCollider.");
            return;
        }

        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create finish celebration");
        root.transform.SetParent(parent, false);

        float surfaceY = platform.bounds.max.y + 0.035f;
        GameObject finishTrigger = GameObject.Find("Finish_Trigger");
        float gateZ = finishTrigger != null ? finishTrigger.transform.position.z : platform.bounds.center.z - 0.5f;
        Material teal = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalTeal.mat");
        Material gold = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalGold.mat");
        Material coral = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_GoalCoral.mat");
        Material stone = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_IslandStoneTop.mat");

        // The gate frames the existing finish trigger but leaves a generous, unobstructed running lane.
        const float postX = 2.18f;
        const float postHeight = 2.82f;
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * postX;
            CreateFinishPrimitive(root.transform, PrimitiveType.Cube, "Portal_Base_" + side,
                new Vector3(x, surfaceY + 0.13f, gateZ), new Vector3(0.82f, 0.26f, 0.78f), gold, Vector3.zero);
            CreateFinishPrimitive(root.transform, PrimitiveType.Cylinder, "Portal_Pillar_" + side,
                new Vector3(x, surfaceY + postHeight * 0.5f, gateZ), new Vector3(0.48f, postHeight, 0.48f), teal, Vector3.zero);
            CreateFinishPrimitive(root.transform, PrimitiveType.Cylinder, "Portal_Capital_" + side,
                new Vector3(x, surfaceY + postHeight + 0.12f, gateZ), new Vector3(0.76f, 0.24f, 0.76f), gold, Vector3.zero);
            CreateFinishPrimitive(root.transform, PrimitiveType.Sphere, "Portal_Light_" + side,
                new Vector3(x, surfaceY + postHeight + 0.38f, gateZ), new Vector3(0.3f, 0.3f, 0.3f), coral, Vector3.zero);
        }

        float beamY = surfaceY + postHeight + 0.29f;
        CreateFinishPrimitive(root.transform, PrimitiveType.Cube, "Portal_Crossbeam",
            new Vector3(0f, beamY, gateZ), new Vector3(4.65f, 0.42f, 0.58f), teal, Vector3.zero);
        CreateFinishPrimitive(root.transform, PrimitiveType.Cube, "Portal_Crossbeam_GoldTrim",
            new Vector3(0f, beamY + 0.25f, gateZ), new Vector3(4.35f, 0.12f, 0.62f), gold, Vector3.zero);

        // Four small stepping-stone inlays guide the eye along the final approach without changing collision.
        for (int i = 0; i < 4; i++)
        {
            float z = gateZ - 2.25f + i * 0.63f;
            Material tileMaterial = i % 2 == 0 ? stone : gold;
            CreateFinishPrimitive(root.transform, PrimitiveType.Cube, "Approach_Inlay_" + (i + 1),
                new Vector3(0f, surfaceY + 0.018f, z), new Vector3(1.3f, 0.035f, 0.38f), tileMaterial, Vector3.zero);
        }

        // A gold medallion and a low-poly star make the goal readable from the course camera.
        float emblemY = surfaceY + postHeight + 0.82f;
        CreateFinishPrimitive(root.transform, PrimitiveType.Cylinder, "Portal_Emblem",
            new Vector3(0f, emblemY, gateZ - 0.12f), new Vector3(0.78f, 0.16f, 0.78f), gold, new Vector3(90f, 0f, 0f));
        CreateFinishStar(root.transform, new Vector3(0f, emblemY, gateZ - 0.225f),
            AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_CoinGold_Dark.mat"));
    }

    private static GameObject CreateFinishPrimitive(Transform parent, PrimitiveType type, string name,
        Vector3 position, Vector3 scale, Material material, Vector3 euler)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        Undo.RegisterCreatedObjectUndo(part, "Create finish detail");
        part.name = name;
        part.transform.SetParent(parent, true);
        part.transform.position = position;
        part.transform.rotation = Quaternion.Euler(euler);
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Undo.DestroyObjectImmediate(collider);
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
        return part;
    }

    private static void CreateFinishStar(Transform parent, Vector3 position, Material material)
    {
        const string meshPath = "Assets/_Project/Art/Generated/Meshes/SM_FinishStar.asset";
        EnsureFolder("Assets/_Project/Art/Generated/Meshes");
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null)
        {
            mesh = new Mesh { name = "SM_FinishStar" };
            Vector3[] vertices = new Vector3[11];
            int[] triangles = new int[30];
            vertices[0] = Vector3.zero;
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? 0.38f : 0.17f;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                int tri = i * 3;
                triangles[tri] = 0;
                triangles[tri + 1] = (i + 1) % 10 + 1;
                triangles[tri + 2] = i + 1;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.back;
            mesh.normals = normals;
            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        GameObject star = new GameObject("Portal_Star_Emblem", typeof(MeshFilter), typeof(MeshRenderer));
        Undo.RegisterCreatedObjectUndo(star, "Create finish star emblem");
        star.transform.SetParent(parent, true);
        star.transform.position = position;
        star.GetComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = star.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void RebuildFloatingIslandRocks()
    {
        const string rootName = "Quaternius_FloatingIslandRocks";
        GameObject oldRoot = GameObject.Find(rootName);
        if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot);

        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create imported floating island details");
        string[] platforms = { "Start_Island", "Island_01", "Island_02", "Island_03", "Island_04", "Island_05", "Island_06", "Island_07", "Island_08", "Island_09", "End_Island" };
        string[] rockModels = { "Rock_Medium_1", "Rock_Medium_2", "Rock_Medium_3" };
        for (int i = 0; i < platforms.Length; i++)
        {
            GameObject platform = GameObject.Find(platforms[i]);
            BoxCollider box = platform == null ? null : platform.GetComponent<BoxCollider>();
            if (box == null) continue;

            Vector3 surface = PlatformPoint(platforms[i], 0f, 0f);
            float halfWidth = box.bounds.extents.x;
            float halfDepth = box.bounds.extents.z;
            float scale = (i == 0 || i == platforms.Length - 1) ? 0.21f : 0.18f;
            for (int side = 0; side < 2; side++)
            {
                float direction = side == 0 ? -1f : 1f;
                float zOffset = halfDepth * (side == 0 ? 0.32f : -0.32f);
                string modelName = rockModels[(i + side) % rockModels.Length];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuaterniusNatureFolder + "/" + modelName + ".fbx");
                GameObject rock = CreateImportedInstance(prefab, "Quaternius_Rock_" + platforms[i] + "_" + side, root.transform);
                if (rock == null) continue;

                Vector3 rockScale = Vector3.one * scale;
                Quaternion rotation = Quaternion.Euler(0f, (i * 37 + side * 53) % 360, 0f);
                rock.transform.localScale = rockScale;
                rock.transform.rotation = rotation;
                Bounds bounds = GetImportedModelBounds(prefab);
                Vector3 scaledCenter = Vector3.Scale(bounds.center, rockScale);
                Vector3 worldCenterOffset = rotation * scaledCenter;
                Vector3 targetCenter = new Vector3(
                    platform.transform.position.x + direction * (halfWidth - 0.04f),
                    surface.y - 0.27f,
                    platform.transform.position.z + zOffset);
                rock.transform.position = targetCenter - worldCenterOffset;

                foreach (Renderer renderer in rock.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
            }
        }
    }

    private static void RebuildQuaterniusClouds()
    {
        const string rootName = "Quaternius_CloudLayers";
        GameObject oldRoot = GameObject.Find(rootName);
        if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot);

        GameObject cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuaterniusCloudPath);
        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create imported cloud layers");
        Material cloudMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_SoftCloud.mat");
        Vector3[] positions =
        {
            new Vector3(-11f, 10.5f, 9f), new Vector3(11f, 9f, 23f),
            new Vector3(-12f, 11.5f, 40f), new Vector3(11.5f, 10f, 57f),
            new Vector3(-10.5f, 12f, 75f), new Vector3(10.5f, 11f, 91f)
        };
        Vector3[] scales =
        {
            new Vector3(80f, 58f, 88f), new Vector3(68f, 64f, 82f),
            new Vector3(92f, 70f, 96f), new Vector3(74f, 60f, 90f),
            new Vector3(84f, 68f, 82f), new Vector3(70f, 62f, 94f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject cloud = CreateImportedInstance(cloudPrefab, "Quaternius_Cloud_" + (i + 1).ToString("00"), root.transform);
            if (cloud == null) continue;
            cloud.transform.position = positions[i];
            cloud.transform.rotation = Quaternion.Euler(0f, i * 31f, 0f);
            cloud.transform.localScale = scales[i];
            foreach (Renderer renderer in cloud.GetComponentsInChildren<Renderer>(true))
            {
                if (cloudMaterial != null) renderer.sharedMaterial = cloudMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }
    }

    private static GameObject CreateImportedInstance(GameObject prefab, string instanceName, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Missing imported Quaternius model for " + instanceName + ".");
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null) return null;
        Undo.RegisterCreatedObjectUndo(instance, "Place imported environment model");
        instance.name = instanceName;
        instance.transform.SetParent(parent, false);
        return instance;
    }

    private static Bounds GetImportedModelBounds(GameObject prefab)
    {
        Bounds combined = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null) continue;
            Bounds meshBounds = filter.sharedMesh.bounds;
            Vector3 center = filter.transform.localPosition + filter.transform.localRotation *
                Vector3.Scale(meshBounds.center, filter.transform.localScale);
            Vector3 size = Vector3.Scale(meshBounds.size, new Vector3(
                Mathf.Abs(filter.transform.localScale.x),
                Mathf.Abs(filter.transform.localScale.y),
                Mathf.Abs(filter.transform.localScale.z)));
            Bounds part = new Bounds(center, size);
            if (!hasBounds)
            {
                combined = part;
                hasBounds = true;
            }
            else combined.Encapsulate(part);
        }

        return combined;
    }

    private static void RepositionFlowers(Transform dressing)
    {
        string[] names =
        {
            "FlowerPatch_01", "FlowerPatch_02", "FlowerPatch_03", "FlowerPatch_04",
            "FlowerPatch_05", "FlowerPatch_06", "FlowerPatch_07", "FlowerPatch_08",
            "FlowerPatch_09", "FlowerPatch_10", "FlowerPatch_11", "FlowerPatch_12",
            "FlowerPatch_13", "FlowerPatch_14", "FlowerPatch_15", "FlowerPatch_16"
        };
        string[] platforms =
        {
            "Start_Island", "Start_Island", "Start_Island", "Start_Island",
            "Island_01", "Island_01", "Island_02", "Island_02", "Island_03",
            "Island_04", "Island_05", "Island_06", "Island_07", "Island_08",
            "Island_09", "End_Island"
        };
        Vector2[] offsets =
        {
            new Vector2(-3.05f, -2.7f), new Vector2(3.05f, -2.7f), new Vector2(-3.05f, 2.7f), new Vector2(3.05f, 2.7f),
            new Vector2(1.4f, -1.25f), new Vector2(-1.4f, 1.25f), new Vector2(-1.35f, -1.9f), new Vector2(1.35f, 1.9f),
            new Vector2(-1.35f, -1.3f), new Vector2(-2.15f, 1.25f), new Vector2(1.35f, -1.3f), new Vector2(1.35f, 1.9f),
            new Vector2(-1.35f, -1.25f), new Vector2(1.35f, 1.25f), new Vector2(-2.15f, -1.2f), new Vector2(3.05f, -2.7f)
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject flower = GameObject.Find(names[i]);
            if (flower == null) continue;
            Undo.RecordObject(flower.transform, "Reposition flower patch");
            flower.transform.SetParent(dressing, true);
            float scale = 0.82f + (i % 4) * 0.055f;
            flower.transform.localScale = Vector3.one * scale;
            flower.transform.rotation = Quaternion.Euler(0f, (i * 53) % 360, 0f);
            Vector3 surface = PlatformPoint(platforms[i], offsets[i].x, offsets[i].y);
            Renderer renderer = flower.GetComponent<Renderer>();
            float baseOffset = renderer != null ? renderer.bounds.min.y - flower.transform.position.y : 0f;
            flower.transform.position = new Vector3(surface.x, surface.y - baseOffset, surface.z);
        }
    }

    private static void CreateFinishTrigger()
    {
        GameObject previous = GameObject.Find("Finish_Trigger");
        if (previous != null) Undo.DestroyObjectImmediate(previous);
        GameObject finish = new GameObject("Finish_Trigger");
        Undo.RegisterCreatedObjectUndo(finish, "Create finish trigger");
        finish.transform.position = new Vector3(0f, 5.25f, 84.55f);
        BoxCollider trigger = finish.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(4.7f, 4.1f, 0.9f);
        Rigidbody body = finish.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        finish.AddComponent<FinishZone>();
    }

    private static void PlaceTree(GameObject prefab, Transform parent, string name, string platform, float dx, float dz, float yaw, float scale)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null) return;
        Undo.RegisterCreatedObjectUndo(instance, "Place stylized tree");
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.position = PlatformPoint(platform, dx, dz);
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = Vector3.one * scale;
    }

    private static Vector3 PlatformPoint(string platformName, float dx, float dz)
    {
        GameObject platform = GameObject.Find(platformName);
        if (platform == null) return new Vector3(dx, 0f, dz);
        Collider collider = platform.GetComponent<Collider>();
        float top = collider != null ? collider.bounds.max.y : platform.transform.position.y;
        return new Vector3(platform.transform.position.x + dx, top, platform.transform.position.z + dz);
    }

    private static GameObject MakeTreePrefab(string name, string kind, Mesh ico, Mesh cone, Material trunk, Material leafA, Material leafB, Material blossom)
    {
        string path = NatureFolder + "/" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            foreach (MeshRenderer renderer in contents.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.gameObject.name != "Warm trunk")
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        GameObject root = new GameObject(name);
        AddPrimitive(root.transform, "Warm trunk", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.28f, 0.78f, 0.28f), trunk);
        if (kind == "pine")
        {
            AddMeshPart(root.transform, "Lower evergreen crown", cone, new Vector3(0f, 0.88f, 0f), new Vector3(2f, 1.35f, 2f), leafA);
            AddMeshPart(root.transform, "Middle evergreen crown", cone, new Vector3(0f, 1.65f, 0f), new Vector3(1.55f, 1.2f, 1.55f), leafB);
            AddMeshPart(root.transform, "Top evergreen crown", cone, new Vector3(0f, 2.3f, 0f), new Vector3(1.15f, 1.15f, 1.15f), leafA);
        }
        else
        {
            AddMeshPart(root.transform, "Canopy core", ico, new Vector3(0f, 2f, 0f), new Vector3(1.7f, 1.35f, 1.55f), leafA);
            AddMeshPart(root.transform, "Canopy left", ico, new Vector3(-0.62f, 2.35f, 0.03f), Vector3.one * 1.05f, leafB);
            AddMeshPart(root.transform, "Canopy right", ico, new Vector3(0.62f, 2.3f, -0.02f), Vector3.one, leafA);
            AddMeshPart(root.transform, "Canopy crown", ico, new Vector3(0f, 2.85f, 0.05f), Vector3.one * 0.95f, leafB);
            if (kind == "blossom")
            {
                AddMeshPart(root.transform, "Blossom 01", ico, new Vector3(-0.45f, 2.65f, -0.62f), Vector3.one * 0.48f, blossom);
                AddMeshPart(root.transform, "Blossom 02", ico, new Vector3(0.46f, 2.56f, 0.55f), Vector3.one * 0.43f, blossom);
                AddMeshPart(root.transform, "Blossom 03", ico, new Vector3(0.7f, 2.65f, -0.35f), Vector3.one * 0.38f, blossom);
            }
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void AddPrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        part.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void AddMeshPart(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = part.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        // Keep the bright course readable instead of casting large canopy shadows over the jump lane.
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private static Mesh LoadOrCreateMesh(string path, System.Func<Mesh> create)
    {
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;
        mesh = create();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader) { name = name };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Mesh BuildIcosphere()
    {
        float phi = (1f + Mathf.Sqrt(5f)) * 0.5f;
        Vector3[] points =
        {
            new Vector3(-1,phi,0), new Vector3(1,phi,0), new Vector3(-1,-phi,0), new Vector3(1,-phi,0),
            new Vector3(0,-1,phi), new Vector3(0,1,phi), new Vector3(0,-1,-phi), new Vector3(0,1,-phi),
            new Vector3(phi,0,-1), new Vector3(phi,0,1), new Vector3(-phi,0,-1), new Vector3(-phi,0,1)
        };
        for (int i = 0; i < points.Length; i++) points[i].Normalize();
        int[] faces =
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        for (int i = 0; i < faces.Length; i += 3)
        {
            Vector3 a = points[faces[i]], b = points[faces[i + 1]], c = points[faces[i + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(normal, a + b + c) < 0f) { Vector3 temp = b; b = c; c = temp; normal = -normal; }
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            normals.Add(normal); normals.Add(normal); normals.Add(normal);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        }
        Mesh mesh = new Mesh { name = "SM_LowPolyIcosphere" };
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    private static Mesh BuildCone()
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        const int sides = 7;
        for (int i = 0; i < sides; i++)
        {
            float angle0 = i * Mathf.PI * 2f / sides;
            float angle1 = (i + 1) * Mathf.PI * 2f / sides;
            Vector3 p0 = new Vector3(Mathf.Cos(angle0) * 0.5f, 0f, Mathf.Sin(angle0) * 0.5f);
            Vector3 p1 = new Vector3(Mathf.Cos(angle1) * 0.5f, 0f, Mathf.Sin(angle1) * 0.5f);
            Vector3 normal = Vector3.Cross(Vector3.up - p0, p1 - p0).normalized;
            int start = vertices.Count;
            vertices.Add(p0); vertices.Add(Vector3.up); vertices.Add(p1);
            normals.Add(normal); normals.Add(normal); normals.Add(normal);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
            start = vertices.Count;
            vertices.Add(Vector3.zero); vertices.Add(p1); vertices.Add(p0);
            normals.Add(Vector3.down); normals.Add(Vector3.down); normals.Add(Vector3.down);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        }
        Mesh mesh = new Mesh { name = "SM_LowPolyCone" };
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        EnsureFolder(path.Substring(0, slash));
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    private static void RebuildUI()
    {
        GameObject hudObject = GameObject.Find("HUD");
        if (hudObject == null) hudObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = hudObject.GetComponent<Canvas>() ?? hudObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = hudObject.GetComponent<CanvasScaler>() ?? hudObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        if (hudObject.GetComponent<GraphicRaycaster>() == null) hudObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = hudObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero; canvasRect.anchorMax = Vector2.one; canvasRect.sizeDelta = Vector2.zero;
        for (int i = hudObject.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = hudObject.transform.GetChild(i);
            if (child.name != "EventSystem") Undo.DestroyObjectImmediate(child.gameObject);
        }

        Sprite skin = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        roundedSprite = skin;
        CanvasGroup hudGroup = MakeGroup("GameplayHUD", hudObject.transform, false);
        CoinTrackerRefs tracker = BuildCoinTracker(hudGroup.transform);
        TMP_Text score = tracker.Score;
        TMP_Text total = tracker.Total;
        Image progress = tracker.Progress;

        CanvasGroup main = BuildMenu("MainMenu", hudObject.transform, new Color(0.015f, 0.055f, 0.085f, 0.45f), new Color32(41, 206, 173, 255));
        AddText(main.transform, "MenuEyebrow", "A FLOATING ISLANDS ADVENTURE", 23f, new Color32(139, 240, 216, 255), TextAlignmentOptions.Center, new Vector2(0f, 286f), new Vector2(760f, 38f), true);
        AddText(main.transform, "MenuTitle", "CLOUDSTEP", 104f, new Color32(255, 250, 220, 255), TextAlignmentOptions.Center, new Vector2(0f, 170f), new Vector2(1000f, 132f), true);
        AddText(main.transform, "MenuSubtitle", "Find the golden trail. Reach the last isle.", 30f, new Color32(224, 239, 239, 255), TextAlignmentOptions.Center, new Vector2(0f, 78f), new Vector2(850f, 52f), false);
        BuildControlChip(main.transform, "Move", "WASD  /  ARROWS", new Vector2(-162f, -32f));
        BuildControlChip(main.transform, "Jump", "SPACE  /  A", new Vector2(162f, -32f));
        AddText(main.transform, "LookHint", "MOUSE  /  RIGHT STICK TO LOOK", 18f, new Color32(201, 222, 227, 255), TextAlignmentOptions.Center, new Vector2(0f, -112f), new Vector2(560f, 32f), false);
        Button start = AddButton(main.transform, "StartButton", "START RUN", new Vector2(0f, -208f), new Vector2(470f, 86f), new Color32(246, 190, 72, 255), new Color32(35, 44, 57, 255));
        TMP_Text menuCount = AddText(main.transform, "CoinHint", "16 COINS HIDDEN THROUGHOUT THE ISLES", 20f, new Color32(211, 226, 228, 255), TextAlignmentOptions.Center, new Vector2(0f, -302f), new Vector2(720f, 38f), false);

        CanvasGroup death = BuildMenu("DeathMenu", hudObject.transform, new Color(0.045f, 0.018f, 0.035f, 0.48f), new Color32(244, 111, 92, 255));
        AddText(death.transform, "DeathEyebrow", "THE WIND TOOK YOU", 23f, new Color32(255, 177, 157, 255), TextAlignmentOptions.Center, new Vector2(0f, 206f), new Vector2(700f, 38f), true);
        AddText(death.transform, "DeathTitle", "YOU FELL", 88f, new Color32(255, 245, 234, 255), TextAlignmentOptions.Center, new Vector2(0f, 112f), new Vector2(900f, 108f), true);
        AddText(death.transform, "DeathSubtitle", "The islands are still there. Give it another try.", 29f, new Color32(236, 224, 226, 255), TextAlignmentOptions.Center, new Vector2(0f, 28f), new Vector2(900f, 52f), false);
        AddText(death.transform, "DeathScoreLabel", "COINS THIS RUN", 21f, new Color32(255, 202, 158, 255), TextAlignmentOptions.Center, new Vector2(0f, -52f), new Vector2(600f, 34f), true);
        TMP_Text deathScore = AddText(death.transform, "DeathScore", "00 / 16", 44f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, -100f), new Vector2(600f, 68f), true);
        AddText(death.transform, "DeathRestartHint", "PRESS R TO TRY AGAIN", 18f, new Color32(232, 217, 218, 255), TextAlignmentOptions.Center, new Vector2(0f, -152f), new Vector2(600f, 34f), false);
        Button retry = AddButton(death.transform, "RetryButton", "RESTART RUN", new Vector2(0f, -230f), new Vector2(440f, 78f), new Color32(242, 117, 92, 255), Color.white);
        Button deathMenu = AddButton(death.transform, "DeathMenuButton", "MAIN MENU", new Vector2(0f, -324f), new Vector2(360f, 66f), new Color32(44, 54, 72, 255), new Color32(220, 228, 235, 255));

        CanvasGroup win = BuildMenu("WinMenu", hudObject.transform, new Color(0.015f, 0.065f, 0.09f, 0.38f), new Color32(96, 225, 189, 255));
        AddText(win.transform, "WinEyebrow", "YOU FOUND THE WAY", 23f, new Color32(151, 250, 215, 255), TextAlignmentOptions.Center, new Vector2(0f, 224f), new Vector2(700f, 38f), true);
        AddText(win.transform, "WinTitle", "LEVEL COMPLETE!", 82f, new Color32(255, 214, 74, 255), TextAlignmentOptions.Center, new Vector2(0f, 128f), new Vector2(1160f, 108f), true);
        AddText(win.transform, "WinSubtitle", "A beautiful run, from first jump to final landing.", 28f, new Color32(248, 251, 244, 255), TextAlignmentOptions.Center, new Vector2(0f, 43f), new Vector2(900f, 52f), false);
        AddText(win.transform, "WinScoreLabel", "COINS", 20f, new Color32(177, 245, 218, 255), TextAlignmentOptions.Center, new Vector2(0f, -25f), new Vector2(520f, 32f), true);
        TMP_Text winScore = AddText(win.transform, "WinScore", "00 / 16", 46f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, -78f), new Vector2(600f, 68f), true);
        AddText(win.transform, "WinRestartHint", "PRESS R TO PLAY AGAIN", 18f, new Color32(218, 235, 235, 255), TextAlignmentOptions.Center, new Vector2(0f, -136f), new Vector2(600f, 34f), false);
        Button playAgain = AddButton(win.transform, "PlayAgainButton", "PLAY AGAIN", new Vector2(0f, -220f), new Vector2(440f, 78f), new Color32(98, 224, 188, 255), new Color32(22, 49, 55, 255));
        Button winMenu = AddButton(win.transform, "WinMenuButton", "MAIN MENU", new Vector2(0f, -314f), new Vector2(360f, 66f), new Color32(38, 67, 79, 255), new Color32(226, 241, 240, 255));

        EnsureEventSystem();
        GameManager manager = Object.FindAnyObjectByType<GameManager>();
        if (manager != null)
        {
            SerializedObject serialized = new SerializedObject(manager);
            SetReference(serialized, "scoreText", score);
            SetReference(serialized, "totalText", total);
            SetReference(serialized, "progressFill", progress);
            SetReference(serialized, "countPulseTarget", score.rectTransform);
            SetReference(serialized, "gameplayHud", hudGroup);
            SetReference(serialized, "mainMenu", main);
            SetReference(serialized, "deathMenu", death);
            SetReference(serialized, "winMenu", win);
            SetReference(serialized, "mainMenuTotalText", menuCount);
            SetReference(serialized, "deathScoreText", deathScore);
            SetReference(serialized, "winScoreText", winScore);
            SetReference(serialized, "startButton", start);
            SetReference(serialized, "retryButton", retry);
            SetReference(serialized, "deathMenuButton", deathMenu);
            SetReference(serialized, "playAgainButton", playAgain);
            SetReference(serialized, "winMenuButton", winMenu);
            SetReference(serialized, "player", Object.FindAnyObjectByType<PlayerController>());
            Camera mainCamera = Camera.main;
            SetReference(serialized, "cameraFollow", mainCamera != null ? mainCamera.GetComponent<CameraFollow>() : null);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
        }
        SetCanvasState(hudGroup, false);
        SetCanvasState(main, true);
        SetCanvasState(death, false);
        SetCanvasState(win, false);
        // Keep the coin tracker above result-screen scrims, like the reference HUD.
        hudGroup.transform.SetAsLastSibling();
    }

    private static CoinTrackerRefs BuildCoinTracker(Transform parent)
    {
        RectTransform card = MakeRect("CoinTracker", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 56f), new Vector2(24f, -20f));
        Sprite badge = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/UI/CoinBadge.asset");
        Image icon = AddImage(card, "CoinIcon", Color.white, new Vector2(0f, -4f), new Vector2(42f, 42f));
        icon.sprite = badge;
        icon.type = Image.Type.Simple;
        icon.preserveAspect = true;
        TMP_Text score = AddTopLeftText(card, "ScoreValue", "00", 34f, Color.white, new Vector2(51f, -4f), new Vector2(60f, 46f), true);
        TMP_Text total = AddTopLeftText(card, "ScoreTotal", "/ 16", 30f, new Color32(237, 243, 242, 255), new Vector2(111f, -4f), new Vector2(104f, 46f), false);
        Image fill = null;
        score.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        score.outlineWidth = 0.22f;
        total.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        total.outlineWidth = 0.22f;
        score.rectTransform.localScale = Vector3.one;
        return new CoinTrackerRefs { Score = score, Total = total, Progress = fill };
    }

    private static TMP_Text AddTopLeftText(Transform parent, string name, string value, float fontSize, Color color, Vector2 position, Vector2 size, bool bold)
    {
        RectTransform rect = MakeRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0f, 1f), size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.72f);
        text.fontSizeMax = fontSize;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.outlineColor = new Color(0f, 0f, 0f, 0.82f);
        text.outlineWidth = 0.22f;
        return text;
    }

    private static CanvasGroup BuildMenu(string name, Transform canvas, Color overlayColor, Color accent)
    {
        RectTransform root = MakeRect(name, canvas, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        root.sizeDelta = Vector2.zero;
        Image overlay = root.gameObject.AddComponent<Image>();
        overlay.color = overlayColor;
        overlay.raycastTarget = true;
        CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
        SetCanvasState(group, false);
        AddImage(root, "FullPageAccent", accent, new Vector2(0f, -4f), new Vector2(1920f, 5f));
        return group;
    }

    private static void BuildControlChip(Transform parent, string label, string binding, Vector2 position)
    {
        RectTransform chip = MakeRect(label + "ControlChip", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(282f, 78f), position);
        AddImage(chip, "ChipSurface", new Color32(29, 50, 67, 255), Vector2.zero, new Vector2(282f, 78f));
        AddText(chip, "ChipTitle", label.ToUpperInvariant(), 17f, new Color32(124, 218, 199, 255), TextAlignmentOptions.Center, new Vector2(0f, 16f), new Vector2(250f, 28f), true);
        AddText(chip, "ChipBinding", binding, 22f, new Color32(235, 243, 242, 255), TextAlignmentOptions.Center, new Vector2(0f, -13f), new Vector2(260f, 32f), true);
    }

    private static Button AddButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color background, Color foreground)
    {
        RectTransform rect = MakeRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = roundedSprite;
        image.type = roundedSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = background;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = Color.Lerp(background, Color.white, 0.13f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = Color.Lerp(background, Color.black, 0.16f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        AddText(rect, "Label", label, 27f, foreground, TextAlignmentOptions.Center, Vector2.zero, size - new Vector2(28f, 16f), true);
        return button;
    }

    private static TMP_Text AddText(Transform parent, string name, string value, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 size, bool bold)
    {
        RectTransform rect = MakeRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.72f);
        text.fontSizeMax = fontSize;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.outlineColor = new Color(0f, 0f, 0f, 0.52f);
        text.outlineWidth = bold ? 0.16f : 0.08f;
        return text;
    }

    private static Image AddImage(Transform parent, string name, Color color, Vector2 position, Vector2 size)
    {
        RectTransform rect = MakeRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = roundedSprite;
        image.type = roundedSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(gameObject, "Create UI element");
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static CanvasGroup MakeGroup(string name, Transform parent, bool visible)
    {
        RectTransform root = MakeRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        root.sizeDelta = Vector2.zero;
        return root.gameObject.AddComponent<CanvasGroup>();
    }

    private static void SetCanvasState(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.gameObject.SetActive(visible);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static void EnsureEventSystem()
    {
        EventSystem system = Object.FindAnyObjectByType<EventSystem>();
        if (system == null)
        {
            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(go, "Create UI event system");
            system = go.GetComponent<EventSystem>();
        }
        foreach (BaseInputModule module in system.GetComponents<BaseInputModule>())
            if (!(module is InputSystemUIInputModule)) Undo.DestroyObjectImmediate(module);
        InputSystemUIInputModule input = system.GetComponent<InputSystemUIInputModule>();
        if (input == null) input = Undo.AddComponent<InputSystemUIInputModule>(system.gameObject);
        input.AssignDefaultActions();
    }

    private static void SetReference(SerializedObject serialized, string propertyName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null) property.objectReferenceValue = value;
    }
}
