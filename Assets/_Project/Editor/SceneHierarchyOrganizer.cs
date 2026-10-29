#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Professional automated hierarchy organizer for Cloudstep platformer levels.
/// Standardizes scene containers into a predictable, clean industry layout:
///   - [AudioManager]
///   - _Lighting
///   - _Environment
///   - _Platforms
///   - _Props
///   - _Collectibles
///   - _Gameplay
///   - _UI
/// </summary>
public static class SceneHierarchyOrganizer
{
    private static readonly string[] ScenePaths = new string[]
    {
        "Assets/_Project/Scenes/Level01.unity",
        "Assets/_Project/Scenes/Level02_SunsetArchipelago.unity",
        "Assets/_Project/Scenes/Level03_NeonSpireway.unity"
    };

    public static void OrganizeAllScenes()
    {
        Debug.Log("<color=#2196F3><b>[HierarchyOrganizer] Beginning Project-Wide Hierarchy Standardization...</b></color>");

        foreach (string scenePath in ScenePaths)
        {
            OrganizeScene(scenePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#4CAF50><b>[HierarchyOrganizer] Successfully organized all level hierarchies!</b></color>");
    }

    private static void OrganizeScene(string scenePath)
    {
        Debug.Log($"<b>[HierarchyOrganizer] Processing: {scenePath}</b>");
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"[HierarchyOrganizer] Could not open scene at {scenePath}");
            return;
        }

        // 1. Ensure Standard Root Containers Exist
        GameObject lightingRoot = GetOrCreateRoot("_Lighting");
        GameObject envRoot = GetOrCreateRoot("_Environment");
        GameObject platformsRoot = GetOrCreateRoot("_Platforms");
        GameObject propsRoot = GetOrCreateRoot("_Props");
        GameObject collectiblesRoot = GetOrCreateRoot("_Collectibles");
        GameObject gameplayRoot = GetOrCreateRoot("_Gameplay");
        GameObject uiRoot = GetOrCreateRoot("_UI");

        // 2. Identify and Relocate Objects
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject go in rootObjects)
        {
            string name = go.name;

            // Skip standard root containers
            if (name == "_Lighting" || name == "_Environment" || name == "_Platforms" ||
                name == "_Props" || name == "_Collectibles" || name == "_Gameplay" ||
                name == "_UI")
            {
                continue;
            }

            // Keep AudioManager at Root
            if (name.Contains("AudioManager"))
            {
                continue;
            }

            // Environment items
            if (name.Contains("Atmosphere") || name.Contains("Horizons") || name.Contains("Sun_System") ||
                name.Contains("Cloud") || name.Contains("Sky") || name.Contains("Embers") ||
                name.Contains("Stars") || name.Contains("Seabird") || name.Contains("Motes"))
            {
                MoveTo(go, envRoot.transform);
            }
            // Props items
            else if (name.StartsWith("Props_") || name.Contains("Dressing") || name.Contains("Scenery"))
            {
                MoveTo(go, propsRoot.transform);
            }
            // Platform items
            else if (name.StartsWith("0") || name.Contains("Platform") || name.Contains("Plaza") ||
                     name.Contains("Stepping") || name.Contains("Spire") || name.Contains("Sanctuary") ||
                     name.Contains("Raft") || name.Contains("Terrace") || name.Contains("Citadel"))
            {
                MoveTo(go, platformsRoot.transform);
            }
            // UI items
            else if (name.Contains("Canvas") || name.Contains("EventSystem"))
            {
                MoveTo(go, uiRoot.transform);
            }
            // Collectibles
            else if (name.StartsWith("Coin") || name.Contains("Pickup"))
            {
                MoveTo(go, collectiblesRoot.transform);
            }
            // Gameplay items
            else if (name.Contains("Player") || name.Contains("GameManager") || name.Contains("KillZone") ||
                     name.Contains("Finish") || name.Contains("Camera"))
            {
                MoveTo(go, gameplayRoot.transform);
            }
            // Lighting items
            else if (name.Contains("Light") || name.Contains("Probe"))
            {
                MoveTo(go, lightingRoot.transform);
            }
        }

        // 3. Fix misplaced children inside existing containers
        // Main Camera: ensure it's in _Gameplay, NOT in _Lighting
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform.parent != gameplayRoot.transform)
        {
            MoveTo(mainCam.gameObject, gameplayRoot.transform);
        }

        // Search for all CoinPickups and ensure they are under _Collectibles
        CoinPickup[] pickups = UnityEngine.Object.FindObjectsByType<CoinPickup>(FindObjectsSortMode.None);
        foreach (CoinPickup pickup in pickups)
        {
            if (pickup.transform.parent != collectiblesRoot.transform)
            {
                MoveTo(pickup.gameObject, collectiblesRoot.transform);
            }
        }

        // 4. Sort children in _Platforms sequentially for clean reading
        SortChildrenByName(platformsRoot.transform);

        // 5. Standardize Root Container Ordering in Hierarchy
        List<Transform> orderedRoots = new List<Transform>();

        // AudioManager first if present
        GameObject audioMgr = GameObject.Find("[AudioManager]") ?? GameObject.Find("AudioManager");
        if (audioMgr != null && audioMgr.transform.parent == null)
            orderedRoots.Add(audioMgr.transform);

        orderedRoots.Add(lightingRoot.transform);
        orderedRoots.Add(envRoot.transform);
        orderedRoots.Add(platformsRoot.transform);
        orderedRoots.Add(propsRoot.transform);
        orderedRoots.Add(collectiblesRoot.transform);
        orderedRoots.Add(gameplayRoot.transform);
        orderedRoots.Add(uiRoot.transform);

        for (int i = 0; i < orderedRoots.Count; i++)
        {
            if (orderedRoots[i] != null)
            {
                orderedRoots[i].SetSiblingIndex(i);
            }
        }

        // 6. Save Organized Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"<color=#4CAF50><b>[HierarchyOrganizer] Successfully organized & saved: {scene.name}</b></color>");
    }

    private static GameObject GetOrCreateRoot(string name)
    {
        GameObject go = GameObject.Find("/" + name);
        if (go == null)
        {
            go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        }
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static void MoveTo(GameObject go, Transform targetParent)
    {
        if (go == null || targetParent == null || go.transform.parent == targetParent)
            return;

        Undo.SetTransformParent(go.transform, targetParent, $"Reparent {go.name} to {targetParent.name}");
    }

    private static void SortChildrenByName(Transform parent)
    {
        if (parent == null) return;

        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }

        children = children.OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase).ToList();

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
#endif
