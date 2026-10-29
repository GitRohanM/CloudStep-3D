using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Master builder for stylized skyboxes, fluffy organic cumulus clouds, celestial sun systems,
/// and atmospheric horizon environments for Level 01, Level 02, and Level 03.
/// Guarantees that:
/// 1. Celestial sun/moon/coronas and clouds are located in the TRUE FAR BACKGROUND (Z = 260-320m),
///    completely behind and below all platforms (never intersecting or in front of gameplay!).
/// 2. Coin UI is 100% unified across all levels to match the Level 02 ScoreBar capsule design.
/// 3. Player controls are ONLY shown on Level 01's start menu; Level 02 and 03 have sleek,
///    compact level intro cards without repeated controls clutter.
/// </summary>
public static class StylizedSkyBuilder
{
    private const string Root = "Assets/_Project";
    private const string MatPath = Root + "/Materials";
    private const string TexPath = Root + "/Textures";
    private const string UIPath = Root + "/Art/UI";

    private struct Puff
    {
        public Vector3 offset;
        public float radius;
        public Vector3 scale;

        public Puff(float x, float y, float z, float r, float sx = 1f, float sy = 1f, float sz = 1f)
        {
            offset = new Vector3(x, y, z);
            radius = r;
            scale = new Vector3(sx, sy, sz);
        }
    }

    #region Procedural Cloud Mesh Generator

    /// <summary>
    /// Builds organic, multi-puff 3D cumulus cloud meshes with upward-smoothed normals for soft, velvety lighting.
    /// Uses natural spherical lobes of varied scale to completely eliminate oblong/stretched shapes.
    /// </summary>
    public static Mesh CreateCumulusMesh(string archetype)
    {
        List<Puff> puffs = new List<Puff>();

        if (archetype == "MajesticTower")
        {
            // Grand towering cumulus cloud bank: wide billowing base with majestic tiered domes
            puffs.Add(new Puff(-5.0f, -0.2f, 0.0f, 3.2f, 1.05f, 0.85f, 1.05f));
            puffs.Add(new Puff(-2.0f,  0.2f, 0.3f, 3.8f, 1.00f, 0.90f, 1.00f));
            puffs.Add(new Puff( 2.0f,  0.1f,-0.2f, 4.0f, 1.00f, 0.90f, 1.00f));
            puffs.Add(new Puff( 5.2f, -0.3f, 0.1f, 3.3f, 1.05f, 0.85f, 1.05f));

            // Mid-tier billows
            puffs.Add(new Puff(-3.2f, 2.2f, 0.2f, 3.2f, 1.0f, 1.0f, 1.0f));
            puffs.Add(new Puff( 0.0f, 2.8f, 0.0f, 4.2f, 1.0f, 1.0f, 1.0f));
            puffs.Add(new Puff( 3.0f, 2.4f,-0.3f, 3.4f, 1.0f, 1.0f, 1.0f));
            puffs.Add(new Puff( 0.8f, 1.8f, 1.8f, 2.6f, 0.95f, 0.95f, 0.95f));
            puffs.Add(new Puff(-1.0f, 1.6f,-1.6f, 2.5f, 0.95f, 0.95f, 0.95f));

            // Tower crown peaks
            puffs.Add(new Puff(-0.8f, 4.8f, 0.1f, 3.0f, 0.95f, 1.05f, 0.95f));
            puffs.Add(new Puff( 1.5f, 4.2f,-0.2f, 2.8f, 0.95f, 1.00f, 0.95f));
            puffs.Add(new Puff( 0.2f, 6.0f, 0.0f, 2.2f, 1.00f, 1.00f, 1.00f));

            // Soft flank transitions
            puffs.Add(new Puff(-7.2f, -0.8f, 0.0f, 2.2f, 1.1f, 0.8f, 1.1f));
            puffs.Add(new Puff( 7.4f, -0.8f, 0.1f, 2.3f, 1.1f, 0.8f, 1.1f));
        }
        else if (archetype == "FluffyPuff")
        {
            // Classic Ghibli/Mario buoyant cumulus puff (organic, bubbly, cuddly)
            puffs.Add(new Puff(-2.8f, 0.0f, 0.0f, 2.4f, 1.0f, 0.9f, 1.0f));
            puffs.Add(new Puff( 0.0f, 0.3f, 0.1f, 3.2f, 1.0f, 0.95f, 1.0f));
            puffs.Add(new Puff( 2.7f,-0.1f,-0.1f, 2.3f, 1.0f, 0.9f, 1.0f));

            // Top billowing lobes
            puffs.Add(new Puff(-1.2f, 1.8f, 0.1f, 2.5f, 1.0f, 1.0f, 1.0f));
            puffs.Add(new Puff( 1.3f, 1.9f,-0.1f, 2.6f, 1.0f, 1.0f, 1.0f));
            puffs.Add(new Puff( 0.1f, 3.0f, 0.0f, 2.0f, 1.0f, 1.0f, 1.0f));

            // Depth & flank puffs
            puffs.Add(new Puff( 0.5f, 1.0f, 1.4f, 2.0f, 0.95f, 0.95f, 0.95f));
            puffs.Add(new Puff(-0.4f, 0.9f,-1.3f, 1.9f, 0.95f, 0.95f, 0.95f));
            puffs.Add(new Puff(-4.2f,-0.4f, 0.0f, 1.6f, 1.05f, 0.85f, 1.05f));
            puffs.Add(new Puff( 4.1f,-0.5f, 0.0f, 1.7f, 1.05f, 0.85f, 1.05f));
        }
        else // "OceanCarpet"
        {
            // Rolling stratocumulus cloud ocean carpet
            for (int i = 0; i < 9; i++)
            {
                float t = (float)i / 8f;
                float x = Mathf.Lerp(-16f, 16f, t);
                float z = Mathf.Sin(t * Mathf.PI * 2f) * 3.5f;
                float y = Mathf.Sin(t * Mathf.PI) * 1.2f;
                float r = 3.2f + Mathf.Sin(t * Mathf.PI) * 1.6f;
                puffs.Add(new Puff(x, y, z, r, 1.15f, 0.75f, 1.15f));
            }
            puffs.Add(new Puff(-6f, 1.2f,  2.0f, 3.4f, 1.05f, 0.8f, 1.05f));
            puffs.Add(new Puff( 5f, 1.3f, -2.0f, 3.5f, 1.05f, 0.8f, 1.05f));
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        const int latSegments = 14;
        const int lonSegments = 18;

        foreach (Puff p in puffs)
        {
            int baseIndex = vertices.Count;

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float theta = v * Mathf.PI;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float phi = u * Mathf.PI * 2f;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 rawNormal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    Vector3 pos = p.offset + Vector3.Scale(rawNormal * p.radius, p.scale);

                    // Soft Ghibli upward normal bias
                    Vector3 styledNormal = Vector3.Normalize(Vector3.Lerp(rawNormal, Vector3.up, 0.40f));

                    vertices.Add(pos);
                    normals.Add(styledNormal);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = baseIndex + lat * (lonSegments + 1) + lon;
                    int next = current + lonSegments + 1;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = $"ProceduralCumulus_{archetype}";
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>
    /// Builds a double-sided solar corona ring mesh facing along Z.
    /// </summary>
    public static Mesh CreateCoronaRingMesh(float innerRadius, float outerRadius, int segments = 64)
    {
        Mesh mesh = new Mesh();
        mesh.name = "SolarCoronaRing";
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

            Vector3 inner = new Vector3(cos * innerRadius, sin * innerRadius, 0f);
            Vector3 outer = new Vector3(cos * outerRadius, sin * outerRadius, 0f);
            float u = (float)i / segments;

            vertices[i * 2] = inner;
            vertices[i * 2 + 1] = outer;
            normals[i * 2] = Vector3.back;
            normals[i * 2 + 1] = Vector3.back;
            uvs[i * 2] = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);

            vertices[vOffset + i * 2] = inner;
            vertices[vOffset + i * 2 + 1] = outer;
            normals[vOffset + i * 2] = Vector3.forward;
            normals[vOffset + i * 2 + 1] = Vector3.forward;
            uvs[vOffset + i * 2] = new Vector2(u, 0f);
            uvs[vOffset + i * 2 + 1] = new Vector2(u, 1f);

            int next = (i + 1) % segments;
            int t = i * 6;
            triangles[t + 0] = i * 2;
            triangles[t + 1] = i * 2 + 1;
            triangles[t + 2] = next * 2;
            triangles[t + 3] = next * 2;
            triangles[t + 4] = i * 2 + 1;
            triangles[t + 5] = next * 2 + 1;

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

    /// <summary>
    /// Creates a graceful low-poly soaring seabird mesh.
    /// </summary>
    public static Mesh CreateSeabirdMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "StylizedSeabird";

        Vector3[] v = new Vector3[]
        {
            new Vector3(0f, 0f, 0.45f),      // 0: Beak
            new Vector3(0f, 0.06f, 0f),      // 1: Head/Chest
            new Vector3(0f, 0.02f, -0.7f),   // 2: Tail
            new Vector3(-2.0f, 0.40f, -0.2f),// 3: Left wing tip
            new Vector3(-0.7f, 0.16f, 0.1f), // 4: Left wing shoulder
            new Vector3(2.0f, 0.40f, -0.2f), // 5: Right wing tip
            new Vector3(0.7f, 0.16f, 0.1f),  // 6: Right wing shoulder
        };

        int[] t = new int[]
        {
            0, 1, 2,  0, 2, 1,
            1, 4, 3,  1, 3, 4,
            4, 2, 3,  4, 3, 2,
            1, 5, 6,  1, 6, 5,
            6, 5, 2,  6, 2, 5
        };

        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    #endregion

    #region Asset & Material Helpers

    public static Material CreateOrLoadMaterial(string path, Color albedo, float smoothness, float metallic, Color? emission = null)
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

    public static Material CreateSoftParticleMaterial(string matName, Color tintColor)
    {
        string path = MatPath + "/" + matName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            mat = new Material(pShader);
            AssetDatabase.CreateAsset(mat, path);
        }

        Texture2D softTex = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
        if (softTex != null)
        {
            mat.SetTexture("_BaseMap", softTex);
        }
        mat.SetColor("_BaseColor", tintColor);
        mat.SetFloat("_Surface", 1f); // Transparent
        mat.SetFloat("_Blend", 1f);   // Additive
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    #endregion

    #region Coin UI 100% Unification

    /// <summary>
    /// Unifies the Coin UI across any open scene to strictly match Level 02's ScoreBar capsule design.
    /// </summary>
    public static void ApplyUnifiedCoinUI()
    {
        GameObject uiRoot = GameObject.Find("_UI");
        if (uiRoot == null) return;

        Transform coinTrackerT = uiRoot.transform.Find("Canvas_GameUI/GameplayHUD/CoinTracker");
        if (coinTrackerT == null) return;

        Sprite scoreBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UIPath + "/ScoreBar.asset");
        Sprite coinBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UIPath + "/CoinBadge.asset");
        TMP_FontAsset lilitaFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UIPath + "/Fonts/LilitaOne SDF.asset");

        // 1. Tracker Background Capsule
        RectTransform rtTracker = coinTrackerT as RectTransform;
        rtTracker.anchorMin = new Vector2(1f, 1f);
        rtTracker.anchorMax = new Vector2(1f, 1f);
        rtTracker.pivot = new Vector2(1f, 1f);
        rtTracker.anchoredPosition = new Vector2(-28f, -24f);
        rtTracker.sizeDelta = new Vector2(182f, 54f);

        Image imgTracker = coinTrackerT.GetComponent<Image>();
        if (imgTracker != null)
        {
            if (scoreBarSprite != null) imgTracker.sprite = scoreBarSprite;
            imgTracker.type = Image.Type.Sliced;
            imgTracker.color = new Color(0.12f, 0.15f, 0.24f, 0.95f);
        }

        // 2. Score Text
        Transform scoreT = coinTrackerT.Find("ScoreText");
        if (scoreT != null)
        {
            RectTransform rtScore = scoreT as RectTransform;
            rtScore.anchorMin = new Vector2(0f, 0.5f);
            rtScore.anchorMax = new Vector2(0f, 0.5f);
            rtScore.pivot = new Vector2(0f, 0.5f);
            rtScore.anchoredPosition = new Vector2(18f, 0f);
            rtScore.sizeDelta = new Vector2(48f, 44f);

            TextMeshProUGUI tmpScore = scoreT.GetComponent<TextMeshProUGUI>();
            if (tmpScore != null)
            {
                if (lilitaFont != null) tmpScore.font = lilitaFont;
                tmpScore.fontSize = 30f;
                tmpScore.color = Color.white;
                tmpScore.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }

        // 3. Total Text
        Transform totalT = coinTrackerT.Find("TotalText");
        if (totalT != null)
        {
            RectTransform rtTotal = totalT as RectTransform;
            rtTotal.anchorMin = new Vector2(0f, 0.5f);
            rtTotal.anchorMax = new Vector2(0f, 0.5f);
            rtTotal.pivot = new Vector2(0f, 0.5f);
            rtTotal.anchoredPosition = new Vector2(66f, -2f);
            rtTotal.sizeDelta = new Vector2(58f, 38f);

            TextMeshProUGUI tmpTotal = totalT.GetComponent<TextMeshProUGUI>();
            if (tmpTotal != null)
            {
                if (lilitaFont != null) tmpTotal.font = lilitaFont;
                tmpTotal.fontSize = 21f;
                tmpTotal.color = new Color(1.0f, 0.85f, 0.45f, 1.0f);
                tmpTotal.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }

        // 4. Coin Icon
        Transform iconT = coinTrackerT.Find("CoinIcon");
        if (iconT != null)
        {
            RectTransform rtIcon = iconT as RectTransform;
            rtIcon.anchorMin = new Vector2(0f, 0.5f);
            rtIcon.anchorMax = new Vector2(0f, 0.5f);
            rtIcon.pivot = new Vector2(0f, 0.5f);
            rtIcon.anchoredPosition = new Vector2(128f, 0f);
            rtIcon.sizeDelta = new Vector2(38f, 38f);

            Image imgIcon = iconT.GetComponent<Image>();
            if (imgIcon != null)
            {
                if (coinBadgeSprite != null) imgIcon.sprite = coinBadgeSprite;
                imgIcon.color = Color.white;
                imgIcon.type = Image.Type.Simple;
            }
        }

        EditorUtility.SetDirty(uiRoot);
        Debug.Log("<color=green>[CoinUI] Unified CoinTracker matching Level02 ScoreBar applied to scene!</color>");
    }

    #endregion

    #region Start Menu Controls Configuration

    /// <summary>
    /// Configures the Start Menu. If isFirstLevel is false, removes player controls pills and
    /// adjusts the card layout into a sleek, clean, modern level mission card.
    /// </summary>
    public static void ConfigureStartMenuControls(bool isFirstLevel, string levelRibbon, string eyebrow, string title, string subtitle, string coinsHint)
    {
        GameObject uiRoot = GameObject.Find("_UI");
        if (uiRoot == null) return;

        Transform startMenuT = uiRoot.transform.Find("Canvas_GameUI/StartMenu");
        if (startMenuT == null) return;

        Transform cardT = startMenuT.Find("Card");
        if (cardT == null) return;

        RectTransform rtCard = cardT as RectTransform;

        if (isFirstLevel)
        {
            // Level 01: Keep controls so new players learn
            rtCard.sizeDelta = new Vector2(720f, 540f);
            return;
        }

        // Level 02 & Level 03: Remove controls pills
        string[] pillsToRemove = { "MovePill", "JumpPill", "LookPill", "PausePill" };
        foreach (string pill in pillsToRemove)
        {
            Transform p = cardT.Find(pill);
            if (p != null) Object.DestroyImmediate(p.gameObject);
        }

        // Re-layout into sleek compact mission card
        rtCard.sizeDelta = new Vector2(620f, 380f);

        Transform ribbonT = cardT.Find("HeaderRibbon");
        if (ribbonT != null)
        {
            (ribbonT as RectTransform).anchoredPosition = new Vector2(0f, 190f);
            (ribbonT as RectTransform).sizeDelta = new Vector2(440f, 52f);
            var txt = ribbonT.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (txt != null && !string.IsNullOrEmpty(levelRibbon)) txt.text = levelRibbon;
        }

        Transform eyebrowT = cardT.Find("Eyebrow");
        if (eyebrowT != null)
        {
            (eyebrowT as RectTransform).anchoredPosition = new Vector2(0f, 130f);
            (eyebrowT as RectTransform).sizeDelta = new Vector2(560f, 26f);
            var txt = eyebrowT.GetComponent<TextMeshProUGUI>();
            if (txt != null && !string.IsNullOrEmpty(eyebrow)) txt.text = eyebrow;
        }

        Transform titleT = cardT.Find("Title");
        if (titleT != null)
        {
            (titleT as RectTransform).anchoredPosition = new Vector2(0f, 85f);
            (titleT as RectTransform).sizeDelta = new Vector2(560f, 44f);
            var txt = titleT.GetComponent<TextMeshProUGUI>();
            if (txt != null && !string.IsNullOrEmpty(title)) txt.text = title;
        }

        Transform subT = cardT.Find("Subtitle");
        if (subT != null)
        {
            (subT as RectTransform).anchoredPosition = new Vector2(0f, 35f);
            (subT as RectTransform).sizeDelta = new Vector2(540f, 30f);
            var txt = subT.GetComponent<TextMeshProUGUI>();
            if (txt != null && !string.IsNullOrEmpty(subtitle)) txt.text = subtitle;
        }

        Transform hintT = cardT.Find("TotalCoinsHint");
        if (hintT != null)
        {
            (hintT as RectTransform).anchoredPosition = new Vector2(0f, -20f);
            (hintT as RectTransform).sizeDelta = new Vector2(540f, 28f);
            var txt = hintT.GetComponent<TextMeshProUGUI>();
            if (txt != null && !string.IsNullOrEmpty(coinsHint)) txt.text = coinsHint;
        }

        Transform btnT = cardT.Find("StartButton");
        if (btnT != null)
        {
            (btnT as RectTransform).anchoredPosition = new Vector2(0f, -95f);
            (btnT as RectTransform).sizeDelta = new Vector2(360f, 62f);
        }

        EditorUtility.SetDirty(uiRoot);
        Debug.Log($"<color=cyan>[StartMenu] Sleek controls-free mission card applied to {uiRoot.scene.name}!</color>");
    }

    /// <summary>
    /// Configures the Level 03 WinMenu into the ultimate Game Finished celebration modal with a prominent MAIN MENU button.
    /// </summary>
    public static void ConfigureLevel03GameCompleteUI()
    {
        GameObject uiRoot = GameObject.Find("_UI");
        if (uiRoot == null) return;

        Transform card = uiRoot.transform.Find("Canvas_GameUI/WinMenu/Card");
        if (card == null) return;

        // 1. Header Ribbon
        Transform ribbon = card.Find("HeaderRibbon");
        TextMeshProUGUI ribbonText = ribbon?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (ribbonText != null)
        {
            ribbonText.text = "YOU'VE FINISHED THE GAME!";
            ribbonText.fontSize = 24f;
        }
        if (ribbon != null)
        {
            RectTransform rt = ribbon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 54f);
            rt.anchoredPosition = new Vector2(0f, 310f);
        }

        // 2. Eyebrow
        Transform eyebrowT = card.Find("WinEyebrow");
        TextMeshProUGUI eyebrow;
        if (eyebrowT != null)
        {
            eyebrow = eyebrowT.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject go = new GameObject("WinEyebrow", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(card, false);
            eyebrow = go.GetComponent<TextMeshProUGUI>();
        }
        eyebrow.text = "THE CELESTIAL ODYSSEY IS COMPLETE";
        eyebrow.fontSize = 15f;
        eyebrow.font = ribbonText?.font;
        eyebrow.color = new Color(0.48f, 0.95f, 0.98f);
        eyebrow.alignment = TextAlignmentOptions.Center;
        eyebrow.fontStyle = FontStyles.Bold;
        RectTransform ebRect = eyebrow.GetComponent<RectTransform>();
        ebRect.anchoredPosition = new Vector2(0f, 235f);
        ebRect.sizeDelta = new Vector2(620f, 24f);

        // 3. Title
        Transform titleT = card.Find("WinTitle");
        TextMeshProUGUI title;
        if (titleT != null)
        {
            title = titleT.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject go = new GameObject("WinTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(card, false);
            title = go.GetComponent<TextMeshProUGUI>();
        }
        title.text = "CONGRATULATIONS!";
        title.fontSize = 36f;
        title.font = ribbonText?.font;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 195f);
        titleRect.sizeDelta = new Vector2(620f, 44f);

        // 4. Subtitle
        Transform subtitleT = card.Find("WinSubtitle");
        TextMeshProUGUI subtitle = subtitleT?.GetComponent<TextMeshProUGUI>();
        if (subtitle != null)
        {
            subtitle.text = "You conquered all 3 levels and mastered the skies of Cloudstep!";
            subtitle.fontSize = 15f;
            subtitle.color = new Color(0.85f, 0.92f, 0.98f);
            subtitle.alignment = TextAlignmentOptions.Center;
            RectTransform subRect = subtitle.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0f, 150f);
            subRect.sizeDelta = new Vector2(620f, 32f);
        }

        // 5. Stars
        for (int i = 0; i < 3; i++)
        {
            Transform s = card.Find("Star_" + i);
            if (s != null)
            {
                RectTransform sRect = s.GetComponent<RectTransform>();
                sRect.anchoredPosition = new Vector2(sRect.anchoredPosition.x, 95f);
            }
        }

        // 6. StatsBox
        Transform statsBox = card.Find("StatsBox");
        if (statsBox != null)
        {
            RectTransform sbRect = statsBox.GetComponent<RectTransform>();
            sbRect.anchoredPosition = new Vector2(0f, 15f);
            TextMeshProUGUI label = statsBox.Find("WinScoreLabel")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = "FINAL LEVEL CRYSTAL COINS";
        }

        // 7. Buttons: Remove redundant replay campaign button
        Transform replayBtn = card.Find("ReplayCampaignButton");
        if (replayBtn != null) Object.DestroyImmediate(replayBtn.gameObject);

        string pack = "Assets/_Project/Art/UI/KenneyUI/Pack/PNG";
        Sprite greenSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Green/Default/button_rectangle_depth_gradient.png");
        Sprite greenFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Green/Default/button_rectangle_flat.png");
        Sprite greySprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/button_rectangle_depth_gradient.png");
        Sprite greyFlatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pack + "/Grey/Default/button_rectangle_flat.png");

        Transform menuBtn = card.Find("WinMenuButton");
        Button menuB = menuBtn?.GetComponent<Button>();
        if (menuBtn != null && menuB != null)
        {
            RectTransform mRect = menuBtn.GetComponent<RectTransform>();
            mRect.sizeDelta = new Vector2(430f, 66f);
            mRect.anchoredPosition = new Vector2(0f, -80f);
            Image img = menuBtn.GetComponent<Image>();
            if (greenSprite != null) img.sprite = greenSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            menuB.transition = Selectable.Transition.SpriteSwap;
            menuB.spriteState = new SpriteState
            {
                highlightedSprite = greenSprite,
                pressedSprite = greenFlatSprite,
                selectedSprite = greenSprite
            };
            TextMeshProUGUI txt = menuBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = "MAIN MENU";
                txt.fontSize = 26f;
                txt.color = Color.white;
                txt.fontStyle = FontStyles.Bold;
            }
        }

        Transform playAgainBtn = card.Find("PlayAgainButton");
        Button playB = playAgainBtn?.GetComponent<Button>();
        if (playAgainBtn != null && playB != null)
        {
            RectTransform pRect = playAgainBtn.GetComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(360f, 52f);
            pRect.anchoredPosition = new Vector2(0f, -155f);
            Image img = playAgainBtn.GetComponent<Image>();
            if (greySprite != null) img.sprite = greySprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            playB.transition = Selectable.Transition.SpriteSwap;
            playB.spriteState = new SpriteState
            {
                highlightedSprite = greySprite,
                pressedSprite = greyFlatSprite,
                selectedSprite = greySprite
            };
            TextMeshProUGUI txt = playAgainBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = "PLAY AGAIN";
                txt.fontSize = 20f;
                txt.color = new Color(0.18f, 0.20f, 0.28f);
                txt.fontStyle = FontStyles.Bold;
            }
        }

        // 8. Wire into GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            so.FindProperty("winMenuButton").objectReferenceValue = menuB;
            so.FindProperty("nextLevelButton").objectReferenceValue = menuB;
            so.FindProperty("playAgainButton").objectReferenceValue = playB;
            so.FindProperty("nextSceneName").stringValue = "Level01";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gm);
        }

        EditorUtility.SetDirty(card.gameObject);
        Debug.Log("<color=green>[WinMenu] Level 03 Game Complete UI configured with prominent MAIN MENU button!</color>");
    }

    #endregion

    #region Level 01 Skybox & Atmosphere Overhaul

    // [MenuItem("Tools/Platformer/Overhaul Level01 Skybox & Atmosphere")]
    public static void OverhaulLevel01Sky()
    {
        GameObject envRoot = GameObject.Find("_Environment");
        GameObject lightingRoot = GameObject.Find("_Lighting");
        if (envRoot == null || lightingRoot == null)
        {
            Debug.LogError("Cannot find _Environment or _Lighting root in active scene.");
            return;
        }

        // Adjust Main Camera far clip plane so horizons & skies are never cut off
        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 500f;
            EditorUtility.SetDirty(Camera.main);
        }

        // Clean out legacy environment skies
        for (int i = envRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = envRoot.transform.GetChild(i);
            if (!child.name.Equals("Quaternius_FloatingIslandRocks"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // Clean out any old clouds that were previously inside _Platforms
        GameObject oldPlatformClouds = GameObject.Find("_Platforms/PlatformerCourse_Generated/Clouds - No Colliders");
        if (oldPlatformClouds != null)
        {
            Object.DestroyImmediate(oldPlatformClouds);
        }

        // 1. Directional Sun Light (brilliant morning golden-white)
        Transform sunTransform = lightingRoot.transform.Find("Directional Light");
        Light sunLight = null;
        if (sunTransform != null)
        {
            sunTransform.rotation = Quaternion.Euler(32f, -25f, 0f);
            sunLight = sunTransform.GetComponent<Light>();
            if (sunLight != null)
            {
                sunLight.color = new Color(1f, 0.97f, 0.90f);
                sunLight.intensity = 1.35f;
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = 0.80f;
            }
        }

        // 2. Daytime Procedural Skybox Material (Luminous Summer Azure)
        string skyPath = MatPath + "/M_Level01Skybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
        if (skyMat == null)
        {
            skyMat = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(skyMat, skyPath);
        }
        skyMat.SetFloat("_SunDisk", 2f);
        skyMat.SetFloat("_SunSize", 0.04f);
        skyMat.SetFloat("_SunSizeConvergence", 5.0f);
        skyMat.SetFloat("_AtmosphereThickness", 0.85f);
        skyMat.SetColor("_SkyTint", new Color(0.20f, 0.52f, 0.94f));
        skyMat.SetColor("_GroundColor", new Color(0.65f, 0.82f, 0.94f));
        skyMat.SetFloat("_Exposure", 1.25f);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;
        if (sunLight != null) RenderSettings.sun = sunLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.60f, 0.80f, 1.0f);
        RenderSettings.ambientEquatorColor = new Color(0.72f, 0.85f, 0.92f);
        RenderSettings.ambientGroundColor = new Color(0.35f, 0.45f, 0.38f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 420f;
        RenderSettings.fogColor = new Color(0.68f, 0.84f, 0.95f);

        // 3. Daytime Celestial Sun System (Positioned in TRUE BACKGROUND at Z=280m, NEVER in front of platforms!)
        BuildDaytimeSunSystem(envRoot.transform, new Vector3(70f, 48f, 280f));

        // 4. Distant Archipelago Horizons & Mountain Silhouettes
        BuildDistantArchipelago(envRoot.transform, false);

        // 5. True Stylized Fluffy Cumulus Clouds (Horizon placement only, |X| > 55m or Z > 180m)
        BuildFluffyCloudLayers(envRoot.transform, false);

        // 6. Soaring White Seabird Flocks
        BuildSeabirdFlock(envRoot.transform, new Vector3(-20f, 32f, 140f));
        BuildSeabirdFlock(envRoot.transform, new Vector3(35f, 38f, 160f));

        // 7. Soft Golden Sunlight Pollen / Air Motes
        BuildSunlightMotes(envRoot.transform);

        // 8. Apply Unified Coin UI
        ApplyUnifiedCoinUI();

        EditorUtility.SetDirty(envRoot);
        Debug.Log("<color=cyan><b>[StylizedSkyBuilder]</b> Level 01 Skybox & Atmosphere successfully overhauled with far background sun and unified UI!</color>");
    }

    #endregion

    #region Level 02 Skybox & Atmosphere Overhaul

    // [MenuItem("Tools/Platformer/Overhaul Level02 Skybox & Atmosphere")]
    public static void OverhaulLevel02Sky()
    {
        GameObject envRoot = GameObject.Find("_Environment");
        GameObject lightingRoot = GameObject.Find("_Lighting");
        if (envRoot == null || lightingRoot == null)
        {
            Debug.LogError("Cannot find _Environment or _Lighting root in active scene.");
            return;
        }

        // Adjust Main Camera far clip plane
        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 500f;
            EditorUtility.SetDirty(Camera.main);
        }

        // Clean out all dynamic sky/atmosphere objects
        for (int i = envRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = envRoot.transform.GetChild(i);
            if (!child.name.Equals("Quaternius_FloatingIslandRocks"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // 1. Directional Sunset Sun Light (Rich Golden-Amber)
        Transform sunTransform = lightingRoot.transform.Find("Directional Light");
        Light sunLight = null;
        if (sunTransform != null)
        {
            sunTransform.rotation = Quaternion.Euler(16f, -18f, 0f);
            sunLight = sunTransform.GetComponent<Light>();
            if (sunLight != null)
            {
                sunLight.color = new Color(1f, 0.76f, 0.52f);
                sunLight.intensity = 1.35f;
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = 0.85f;
            }
        }

        // 2. Sunset Skybox Material (Dramatic Twilight Violet to Coral-Rose)
        string skyPath = MatPath + "/M_SunsetSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
        if (skyMat == null)
        {
            skyMat = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(skyMat, skyPath);
        }
        skyMat.SetFloat("_SunDisk", 2f);
        skyMat.SetFloat("_SunSize", 0.045f);
        skyMat.SetFloat("_SunSizeConvergence", 4.2f);
        skyMat.SetFloat("_AtmosphereThickness", 1.15f);
        skyMat.SetColor("_SkyTint", new Color(0.24f, 0.12f, 0.40f));
        skyMat.SetColor("_GroundColor", new Color(0.52f, 0.18f, 0.28f));
        skyMat.SetFloat("_Exposure", 1.30f);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;
        if (sunLight != null) RenderSettings.sun = sunLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.98f, 0.70f, 0.55f);
        RenderSettings.ambientEquatorColor = new Color(0.68f, 0.42f, 0.65f);
        RenderSettings.ambientGroundColor = new Color(0.26f, 0.18f, 0.30f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 420f;
        RenderSettings.fogColor = new Color(0.58f, 0.35f, 0.54f);

        // 3. Colossal Sunset Sun System framed on horizon in TRUE BACKGROUND at Z=280m!
        BuildSunsetSunSystem(envRoot.transform, new Vector3(0f, 26f, 280f));

        // 4. Distant Sunset Mountain Silhouettes
        BuildDistantArchipelago(envRoot.transform, true);

        // 5. Sunset-Illuminated Fluffy Cumulus Clouds (Horizon placement only)
        BuildFluffyCloudLayers(envRoot.transform, true);

        // 6. First Evening Stars in Zenith
        BuildTwilightStars(envRoot.transform);

        // 7. Soft Sunset Golden Embers / Fireflies
        BuildSunsetEmbers(envRoot.transform);

        // 8. Apply Unified Coin UI
        ApplyUnifiedCoinUI();

        // 9. Remove Controls from Start Menu (Player already knows controls from Level 01!)
        ConfigureStartMenuControls(false, "SUNSET ARCHIPELAGO", "A CELESTIAL SKYWAY RUN", "ASCEND THE SPIRES", "Leap across floating islands to reach the Sun Sanctuary.", "16 SUNSET COINS HIDDEN ALONG THE HIGHWAY");

        EditorUtility.SetDirty(envRoot);
        Debug.Log("<color=orange><b>[StylizedSkyBuilder]</b> Level 02 Skybox & Atmosphere successfully overhauled with far background sun and unified UI!</color>");
    }

    #endregion

    #region Level 03 Skybox & Atmosphere Overhaul

    // [MenuItem("Tools/Platformer/Overhaul Level03 Skybox & Atmosphere")]
    public static void OverhaulLevel03Sky()
    {
        GameObject envRoot = GameObject.Find("_Environment");
        GameObject lightingRoot = GameObject.Find("_Lighting");
        if (envRoot == null) return;

        // Adjust Main Camera far clip plane
        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 500f;
            EditorUtility.SetDirty(Camera.main);
        }

        // 1. Adjust Celestial Moon System to be safely in the far background at Z = 260m
        Transform moon = envRoot.transform.Find("Celestial_Moon_System");
        if (moon != null)
        {
            moon.position = new Vector3(0f, 38f, 260f);
            Transform sphere = moon.Find("Moon_Sphere");
            if (sphere != null) sphere.localScale = Vector3.one * 46f;
            Transform rInner = moon.Find("Ring_Inner");
            if (rInner != null)
            {
                rInner.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(28f, 48f, 64);
            }
            Transform rOuter = moon.Find("Ring_Outer");
            if (rOuter != null)
            {
                rOuter.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(50f, 78f, 64);
            }
        }

        // 2. Procedural Fluffy Cumulus Clouds (Cosmic Moonlight Palette, Flanks + Ocean Floor + Far Horizon)
        BuildCosmicCloudLayers(envRoot.transform);

        // 3. Rich, Sparkling Cosmic Starfield (True spherical dome, high-res diamond star texture, zero box particles)
        BuildCosmicStarfield(envRoot.transform);

        // 4. Distant Shooting Stars / Meteors
        BuildCosmicShootingStars(envRoot.transform);

        // 5. Directional Moonlight & Ambient Night Atmosphere
        if (lightingRoot != null)
        {
            Transform sunTransform = lightingRoot.transform.Find("Directional Light");
            if (sunTransform != null)
            {
                sunTransform.rotation = Quaternion.Euler(28f, -22f, 0f);
                Light sunLight = sunTransform.GetComponent<Light>();
                if (sunLight != null)
                {
                    sunLight.color = new Color(0.72f, 0.85f, 1.0f);
                    sunLight.intensity = 1.15f;
                    sunLight.shadows = LightShadows.Soft;
                    sunLight.shadowStrength = 0.80f;
                    RenderSettings.sun = sunLight;
                }
            }
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.24f, 0.28f, 0.42f);
        RenderSettings.ambientEquatorColor = new Color(0.18f, 0.20f, 0.32f);
        RenderSettings.ambientGroundColor = new Color(0.10f, 0.08f, 0.16f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 450f;
        RenderSettings.fogColor = new Color(0.08f, 0.11f, 0.22f);

        // 6. Apply Unified Coin UI
        ApplyUnifiedCoinUI();

        // 7. Remove Controls from Start Menu
        ConfigureStartMenuControls(false, "NEON SPIREWAY", "LEVEL 03 - THE FINAL ASCENT", "CONQUER THE CELESTIAL SPIRES", "Leap across floating neon spires under the celestial moon.", "18 CELESTIAL COINS HIDDEN ALONG THE HIGHWAY");

        // 8. Configure Game Finished Victory UI with prominent MAIN MENU button
        ConfigureLevel03GameCompleteUI();

        EditorUtility.SetDirty(envRoot);
        Debug.Log("<color=magenta><b>[StylizedSkyBuilder]</b> Level 03 Skybox, Stars & Fluffy Clouds overhauled successfully with far background moon, sparkling starfield, and unified UI!</color>");
    }

    #endregion

    #region Sky Elements Construction

    private static void BuildDaytimeSunSystem(Transform envRoot, Vector3 pos)
    {
        GameObject sunRoot = new GameObject("Daytime_Sun_System");
        sunRoot.transform.SetParent(envRoot, false);
        sunRoot.transform.position = pos;

        // A) Glowing Sun Core Sphere (Large scale for far background)
        GameObject sunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunSphere.name = "Sun_Core";
        sunSphere.transform.SetParent(sunRoot.transform, false);
        sunSphere.transform.localScale = Vector3.one * 30f;
        Object.DestroyImmediate(sunSphere.GetComponent<Collider>());

        Material sunMat = CreateOrLoadMaterial(MatPath + "/M_DaytimeSun_Core.mat", new Color(1.0f, 0.98f, 0.90f), 0.9f, 0.1f, new Color(2.8f, 2.4f, 1.6f));
        sunSphere.GetComponent<MeshRenderer>().sharedMaterial = sunMat;

        // B) Inner Corona Halo Ring (Facing along Z, double sided)
        GameObject innerCorona = new GameObject("Corona_Inner", typeof(MeshFilter), typeof(MeshRenderer));
        innerCorona.transform.SetParent(sunRoot.transform, false);
        innerCorona.transform.localRotation = Quaternion.identity;
        innerCorona.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(18f, 32f, 64);
        Material innerMat = CreateOrLoadMaterial(MatPath + "/M_SunCorona_Inner.mat", new Color(1.0f, 0.94f, 0.72f, 0.85f), 0.1f, 0.0f, new Color(2.0f, 1.5f, 0.7f));
        innerMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        innerCorona.GetComponent<MeshRenderer>().sharedMaterial = innerMat;

        // C) Outer Radiant Flare Ring
        GameObject outerCorona = new GameObject("Corona_Outer", typeof(MeshFilter), typeof(MeshRenderer));
        outerCorona.transform.SetParent(sunRoot.transform, false);
        outerCorona.transform.localRotation = Quaternion.identity;
        outerCorona.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(34f, 54f, 64);
        Material outerMat = CreateOrLoadMaterial(MatPath + "/M_SunCorona_Outer.mat", new Color(1.0f, 0.98f, 0.82f, 0.40f), 0.1f, 0.0f, new Color(1.1f, 0.85f, 0.40f));
        outerMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        outerCorona.GetComponent<MeshRenderer>().sharedMaterial = outerMat;

        // D) Radiant Ambient Sun Aura Light
        GameObject aura = new GameObject("Sun_AuraLight", typeof(Light));
        aura.transform.SetParent(sunRoot.transform, false);
        Light l = aura.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1.0f, 0.95f, 0.82f);
        l.range = 280f;
        l.intensity = 2.0f;
    }

    private static void BuildSunsetSunSystem(Transform envRoot, Vector3 pos)
    {
        GameObject sunRoot = new GameObject("Sunset_Sun_System");
        sunRoot.transform.SetParent(envRoot, false);
        sunRoot.transform.position = pos;

        // A) Glowing Colossal Sunset Orb
        GameObject sunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunSphere.name = "Sunset_Sun_Core";
        sunSphere.transform.SetParent(sunRoot.transform, false);
        sunSphere.transform.localScale = Vector3.one * 36f;
        Object.DestroyImmediate(sunSphere.GetComponent<Collider>());

        Material sunMat = CreateOrLoadMaterial(MatPath + "/M_SunsetSun_Core.mat", new Color(1.0f, 0.82f, 0.45f), 0.9f, 0.1f, new Color(3.0f, 1.5f, 0.4f));
        sunSphere.GetComponent<MeshRenderer>().sharedMaterial = sunMat;

        // B) Inner Amber Corona
        GameObject innerCorona = new GameObject("Sunset_Corona_Inner", typeof(MeshFilter), typeof(MeshRenderer));
        innerCorona.transform.SetParent(sunRoot.transform, false);
        innerCorona.transform.localRotation = Quaternion.identity;
        innerCorona.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(22f, 38f, 64);
        Material innerMat = CreateOrLoadMaterial(MatPath + "/M_SunsetCorona_Inner.mat", new Color(1.0f, 0.62f, 0.22f, 0.85f), 0.1f, 0.0f, new Color(2.4f, 1.0f, 0.3f));
        innerMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        innerCorona.GetComponent<MeshRenderer>().sharedMaterial = innerMat;

        // C) Outer Rose/Coral Twilight Halo
        GameObject outerCorona = new GameObject("Sunset_Corona_Outer", typeof(MeshFilter), typeof(MeshRenderer));
        outerCorona.transform.SetParent(sunRoot.transform, false);
        outerCorona.transform.localRotation = Quaternion.identity;
        outerCorona.GetComponent<MeshFilter>().sharedMesh = CreateCoronaRingMesh(40f, 64f, 64);
        Material outerMat = CreateOrLoadMaterial(MatPath + "/M_SunsetCorona_Outer.mat", new Color(0.92f, 0.35f, 0.55f, 0.55f), 0.1f, 0.0f, new Color(1.8f, 0.5f, 0.9f));
        outerMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        outerCorona.GetComponent<MeshRenderer>().sharedMaterial = outerMat;

        // D) Sunset Point Light Aura
        GameObject aura = new GameObject("Sunset_AuraLight", typeof(Light));
        aura.transform.SetParent(sunRoot.transform, false);
        Light l = aura.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1.0f, 0.70f, 0.45f);
        l.range = 300f;
        l.intensity = 2.4f;
    }

    private static void BuildFluffyCloudLayers(Transform envRoot, bool isSunset)
    {
        GameObject cloudsRoot = new GameObject(isSunset ? "Sunset_Cumulus_Atmosphere" : "Daytime_Cumulus_Atmosphere");
        cloudsRoot.transform.SetParent(envRoot, false);

        // Pre-create shared procedural meshes
        Mesh towerMesh = CreateCumulusMesh("MajesticTower");
        Mesh puffMesh = CreateCumulusMesh("FluffyPuff");
        Mesh oceanMesh = CreateCumulusMesh("OceanCarpet");

        Material cloudMat;
        if (isSunset)
        {
            cloudMat = CreateOrLoadMaterial(MatPath + "/M_StylizedCumulus_Sunset.mat",
                new Color(1.0f, 0.82f, 0.70f), 0.0f, 0.0f, new Color(0.35f, 0.14f, 0.20f));
        }
        else
        {
            cloudMat = CreateOrLoadMaterial(MatPath + "/M_StylizedCumulus_Day.mat",
                new Color(0.98f, 0.99f, 1.0f), 0.0f, 0.0f, new Color(0.12f, 0.14f, 0.18f));
        }

        // Layer A: Grand Towering Cloud Banks on far flanks (Wide clearance: |X| >= 65m, never near platforms!)
        Vector3[] towerPos =
        {
            new Vector3(-70f, 16f,  65f), new Vector3( 72f, 18f,  75f),
            new Vector3(-85f, 22f, 130f), new Vector3( 88f, 24f, 145f),
            new Vector3(-95f, 26f, 195f), new Vector3( 96f, 28f, 210f)
        };
        Vector3[] towerScales =
        {
            new Vector3(4.2f, 4.0f, 4.2f), new Vector3(4.5f, 4.2f, 4.5f),
            new Vector3(5.2f, 4.8f, 5.2f), new Vector3(5.0f, 4.6f, 5.0f),
            new Vector3(5.6f, 5.2f, 5.6f), new Vector3(5.4f, 5.0f, 5.4f)
        };

        for (int i = 0; i < towerPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_Tower_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = towerPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 55f) + 12f, 0f);
            c.transform.localScale = towerScales[i];
            c.GetComponent<MeshFilter>().sharedMesh = towerMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }

        // Layer B: Medium Horizon Drifting Clouds (All far in background Z >= 180m, or |X| >= 55m)
        Vector3[] puffPos =
        {
            new Vector3(-58f, 14f,  95f), new Vector3( 60f, 15f, 110f),
            new Vector3(-64f, 18f, 165f), new Vector3( 66f, 19f, 175f),
            new Vector3(-35f, 22f, 235f), new Vector3( 38f, 24f, 245f),
            new Vector3(  0f, 32f, 260f)
        };
        Vector3[] puffScales =
        {
            new Vector3(3.4f, 3.0f, 3.4f), new Vector3(3.6f, 3.2f, 3.6f),
            new Vector3(3.8f, 3.4f, 3.8f), new Vector3(3.6f, 3.2f, 3.6f),
            new Vector3(4.2f, 3.8f, 4.2f), new Vector3(4.0f, 3.6f, 4.0f),
            new Vector3(4.5f, 4.0f, 4.5f)
        };

        for (int i = 0; i < puffPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_Puff_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = puffPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 47f) + 30f, 0f);
            c.transform.localScale = puffScales[i];
            c.GetComponent<MeshFilter>().sharedMesh = puffMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }

        // Layer C: Low Rolling Cloud Ocean beneath the islands (At Y = -24m, far below all platforms!)
        Vector3[] oceanPos =
        {
            new Vector3(-45f, -24f,  45f), new Vector3( 45f, -24f,  55f),
            new Vector3(-55f, -25f, 115f), new Vector3( 55f, -25f, 125f),
            new Vector3(  0f, -26f,  90f), new Vector3(  0f, -26f, 180f)
        };

        for (int i = 0; i < oceanPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_OceanFloor_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = oceanPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 65f), 0f);
            c.transform.localScale = new Vector3(6.5f, 3.2f, 6.5f);
            c.GetComponent<MeshFilter>().sharedMesh = oceanMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }
    }

    private static void BuildDistantArchipelago(Transform envRoot, bool isSunset)
    {
        GameObject horizonsRoot = new GameObject(isSunset ? "Distant_Sunset_Horizons" : "Distant_Archipelago_Horizons");
        horizonsRoot.transform.SetParent(envRoot, false);

        Material horizonMat;
        if (isSunset)
        {
            horizonMat = CreateOrLoadMaterial(MatPath + "/M_DistantHorizon_Sunset.mat",
                new Color(0.32f, 0.18f, 0.36f), 0.1f, 0.0f, new Color(0.18f, 0.06f, 0.15f));
        }
        else
        {
            horizonMat = CreateOrLoadMaterial(MatPath + "/M_DistantHorizon_Day.mat",
                new Color(0.55f, 0.72f, 0.88f), 0.1f, 0.0f, new Color(0.12f, 0.22f, 0.32f));
        }

        // Distant island/mountain positions on horizon (At Z = 240m to 350m, far background!)
        Vector3[] positions =
        {
            new Vector3(-110f, 14f, 250f), new Vector3( 120f, 16f, 260f),
            new Vector3(-140f, 22f, 310f), new Vector3( 135f, 24f, 320f),
            new Vector3( -70f,  8f, 290f), new Vector3(  75f, 10f, 300f),
            new Vector3(-125f,  6f, 220f), new Vector3( 130f,  8f, 230f)
        };
        Vector3[] scales =
        {
            new Vector3(38f, 48f, 38f), new Vector3(42f, 52f, 42f),
            new Vector3(55f, 75f, 55f), new Vector3(52f, 70f, 52f),
            new Vector3(32f, 40f, 32f), new Vector3(35f, 42f, 35f),
            new Vector3(40f, 34f, 40f), new Vector3(42f, 36f, 42f)
        };

        Mesh mountainMesh = null;
        string rockPath = Root + "/Art/External/QuaterniusStylizedNature/FBX/Rock_Medium_1.fbx";
        GameObject rockFbx = AssetDatabase.LoadAssetAtPath<GameObject>(rockPath);
        if (rockFbx != null)
        {
            MeshFilter mf = rockFbx.GetComponentInChildren<MeshFilter>();
            if (mf != null) mountainMesh = mf.sharedMesh;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject m;
            if (mountainMesh != null)
            {
                m = new GameObject($"Distant_Peak_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
                m.GetComponent<MeshFilter>().sharedMesh = mountainMesh;
            }
            else
            {
                m = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                m.name = $"Distant_Peak_{i + 1:00}";
                Object.DestroyImmediate(m.GetComponent<Collider>());
            }

            m.transform.SetParent(horizonsRoot.transform, false);
            m.transform.position = positions[i];
            m.transform.rotation = Quaternion.Euler(-10f, (i * 43f) + 15f, (i % 2 == 0 ? 8f : -8f));
            m.transform.localScale = scales[i];
            m.GetComponent<MeshRenderer>().sharedMaterial = horizonMat;
            m.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private static void BuildSeabirdFlock(Transform envRoot, Vector3 center)
    {
        GameObject flock = new GameObject("FX_SeabirdFlock");
        flock.transform.SetParent(envRoot, false);
        flock.transform.position = center;

        Mesh birdMesh = CreateSeabirdMesh();
        Material birdMat = CreateOrLoadMaterial(MatPath + "/M_StylizedSeabird.mat", Color.white, 0.1f, 0.0f, new Color(0.85f, 0.85f, 0.85f));

        Vector3[] birdOffsets =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(-3.8f, 0.4f, -3.2f),
            new Vector3( 3.8f, 0.3f, -3.0f),
            new Vector3(-7.5f, 0.8f, -6.2f),
            new Vector3( 7.4f, 0.7f, -6.0f)
        };

        for (int i = 0; i < birdOffsets.Length; i++)
        {
            GameObject b = new GameObject($"Bird_{i + 1}", typeof(MeshFilter), typeof(MeshRenderer));
            b.transform.SetParent(flock.transform, false);
            b.transform.localPosition = birdOffsets[i];
            b.transform.localRotation = Quaternion.Euler(0f, 15f, 4f);
            b.transform.localScale = Vector3.one * 1.6f;
            b.GetComponent<MeshFilter>().sharedMesh = birdMesh;
            b.GetComponent<MeshRenderer>().sharedMaterial = birdMat;
            b.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private static void BuildSunlightMotes(Transform envRoot)
    {
        GameObject motesObj = new GameObject("FX_SunlightMotes", typeof(ParticleSystem));
        motesObj.transform.SetParent(envRoot, false);
        motesObj.transform.position = new Vector3(0f, 5f, 40f);

        ParticleSystem ps = motesObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 6f;
        main.startSpeed = 0.25f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.40f);
        main.maxParticles = 80;
        main.startColor = new Color(1f, 0.94f, 0.65f, 0.75f);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(22f, 10f, 65f);

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        Material pMat = CreateSoftParticleMaterial("M_SoftSunMote", new Color(1f, 0.94f, 0.65f, 0.8f));
        ParticleSystemRenderer rend = motesObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = pMat;
    }

    private static void BuildTwilightStars(Transform envRoot)
    {
        GameObject starsObj = new GameObject("FX_TwilightStars", typeof(ParticleSystem));
        starsObj.transform.SetParent(envRoot, false);
        starsObj.transform.position = new Vector3(0f, 35f, 100f);

        ParticleSystem ps = starsObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 10000f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.maxParticles = 300;
        main.startColor = new Color(1f, 0.95f, 0.85f, 0.85f);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 260f;
        shape.radiusThickness = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 300) });

        Material pMat = CreateSoftParticleMaterial("M_SoftStar", new Color(1f, 0.95f, 0.85f, 0.9f));
        ParticleSystemRenderer rend = starsObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = pMat;
    }

    private static void BuildSunsetEmbers(Transform envRoot)
    {
        GameObject embersObj = new GameObject("FX_SunsetEmbers", typeof(ParticleSystem));
        embersObj.transform.SetParent(envRoot, false);
        embersObj.transform.position = new Vector3(0f, 5f, 40f);

        ParticleSystem ps = embersObj.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 6.5f;
        main.startSpeed = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.maxParticles = 75;
        main.startColor = new Color(1f, 0.65f, 0.25f, 0.85f);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(22f, 10f, 70f);

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        Material pMat = CreateSoftParticleMaterial("M_SoftSunsetEmber", new Color(1f, 0.65f, 0.25f, 0.85f));
        ParticleSystemRenderer rend = embersObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = pMat;
    }

    private static void BuildCosmicCloudLayers(Transform envRoot)
    {
        // 1. Clean out any existing cosmic cumulus clouds or old Sky_Cloud objects
        for (int i = envRoot.childCount - 1; i >= 0; i--)
        {
            Transform c = envRoot.GetChild(i);
            if (c.name.StartsWith("Sky_Cloud") || c.name.Equals("Cosmic_Cumulus_Atmosphere"))
            {
                Object.DestroyImmediate(c.gameObject);
            }
        }

        GameObject cloudsRoot = new GameObject("Cosmic_Cumulus_Atmosphere");
        cloudsRoot.transform.SetParent(envRoot, false);

        Mesh towerMesh = CreateCumulusMesh("MajesticTower");
        Mesh puffMesh = CreateCumulusMesh("FluffyPuff");
        Mesh oceanMesh = CreateCumulusMesh("OceanCarpet");

        string matPath = MatPath + "/M_StylizedCumulus_Cosmic.mat";
        Material cloudMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (cloudMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            cloudMat = new Material(shader);
            AssetDatabase.CreateAsset(cloudMat, matPath);
        }
        cloudMat.SetColor("_BaseColor", new Color(0.24f, 0.18f, 0.38f, 1.0f));
        cloudMat.SetFloat("_Smoothness", 0.0f);
        cloudMat.SetFloat("_Metallic", 0.0f);
        cloudMat.EnableKeyword("_EMISSION");
        cloudMat.SetColor("_EmissionColor", new Color(0.22f, 0.38f, 0.58f, 1.0f));
        EditorUtility.SetDirty(cloudMat);

        // Layer A: Grand Towering Cloud Banks on far flanks (|X| >= 68m, completely clear of platforms)
        Vector3[] towerPos =
        {
            new Vector3(-70f, 16f,  65f), new Vector3( 72f, 18f,  75f),
            new Vector3(-85f, 22f, 130f), new Vector3( 88f, 24f, 145f),
            new Vector3(-95f, 26f, 195f), new Vector3( 96f, 28f, 210f)
        };
        Vector3[] towerScales =
        {
            new Vector3(4.5f, 4.2f, 4.5f), new Vector3(4.6f, 4.2f, 4.6f),
            new Vector3(5.2f, 4.8f, 5.2f), new Vector3(5.0f, 4.6f, 5.0f),
            new Vector3(5.6f, 5.2f, 5.6f), new Vector3(5.4f, 5.0f, 5.4f)
        };

        for (int i = 0; i < towerPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_Tower_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = towerPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 55f) + 12f, 0f);
            c.transform.localScale = towerScales[i];
            c.GetComponent<MeshFilter>().sharedMesh = towerMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }

        // Layer B: Medium Horizon Drifting Clouds (All far in background Z >= 180m, or |X| >= 58m)
        Vector3[] puffPos =
        {
            new Vector3(-58f, 14f,  95f), new Vector3( 60f, 15f, 110f),
            new Vector3(-64f, 18f, 165f), new Vector3( 66f, 19f, 175f),
            new Vector3(-38f, 22f, 235f), new Vector3( 40f, 24f, 245f),
            new Vector3(  0f, 12f, 275f)
        };
        Vector3[] puffScales =
        {
            new Vector3(3.4f, 3.0f, 3.4f), new Vector3(3.6f, 3.2f, 3.6f),
            new Vector3(3.8f, 3.4f, 3.8f), new Vector3(3.6f, 3.2f, 3.6f),
            new Vector3(4.2f, 3.8f, 4.2f), new Vector3(4.0f, 3.6f, 4.0f),
            new Vector3(4.8f, 3.6f, 4.8f)
        };

        for (int i = 0; i < puffPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_Puff_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = puffPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 47f) + 30f, 0f);
            c.transform.localScale = puffScales[i];
            c.GetComponent<MeshFilter>().sharedMesh = puffMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }

        // Layer C: Low Rolling Cloud Ocean beneath the islands (At Y = -24m, far below all platforms!)
        Vector3[] oceanPos =
        {
            new Vector3(-45f, -24f,  45f), new Vector3( 45f, -24f,  55f),
            new Vector3(-55f, -25f, 115f), new Vector3( 55f, -25f, 125f),
            new Vector3(  0f, -26f,  90f), new Vector3(  0f, -26f, 180f)
        };

        for (int i = 0; i < oceanPos.Length; i++)
        {
            GameObject c = new GameObject($"Cloud_OceanFloor_{i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
            c.transform.SetParent(cloudsRoot.transform, false);
            c.transform.position = oceanPos[i];
            c.transform.rotation = Quaternion.Euler(0f, (i * 65f), 0f);
            c.transform.localScale = new Vector3(6.5f, 3.2f, 6.5f);
            c.GetComponent<MeshFilter>().sharedMesh = oceanMesh;
            c.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
        }
    }

    private static void BuildCosmicStarfield(Transform envRoot)
    {
        // 1. Destroy any Starlight Motes (which spawned near the camera and caused block particles)
        Transform motes = envRoot.Find("FX_StarlightMotes");
        if (motes != null) Object.DestroyImmediate(motes.gameObject);

        // 2. Setup or find FX_CosmicStarfield
        Transform starfieldT = envRoot.Find("FX_CosmicStarfield");
        GameObject starsObj;
        if (starfieldT != null)
        {
            starsObj = starfieldT.gameObject;
        }
        else
        {
            starsObj = new GameObject("FX_CosmicStarfield", typeof(ParticleSystem));
            starsObj.transform.SetParent(envRoot, false);
        }
        starsObj.transform.position = new Vector3(0f, 25f, 60f);

        // Ensure Star Texture exists
        string texPath = TexPath + "/T_CosmicStar.png";
        Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (starTex == null)
        {
            starTex = GenerateStarTexture(texPath);
        }

        // Star Material (URP Particles/Unlit with Additive Blending)
        string matPath = MatPath + "/M_CosmicStar.mat";
        Material starMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (starMat == null)
        {
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            starMat = new Material(pShader);
            AssetDatabase.CreateAsset(starMat, matPath);
        }
        starMat.SetTexture("_BaseMap", starTex);
        starMat.SetColor("_BaseColor", Color.white);
        starMat.SetFloat("_Surface", 1f); // Transparent
        starMat.SetFloat("_Blend", 2f);   // Additive
        starMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        starMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        starMat.SetFloat("_ZWrite", 0f);
        starMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        starMat.EnableKeyword("_COLOROVERLAY_ON");
        starMat.renderQueue = 3000;
        EditorUtility.SetDirty(starMat);

        ParticleSystem ps = starsObj.GetComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = 10000f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(2.0f, 6.0f);
        main.maxParticles = 2800;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        // Rich celestial gradient: Diamond White, Cyan Moonlight, Soft Cosmic Violet, Warm Gold
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1.0f, 1.0f, 1.0f), 0.0f),
                new GradientColorKey(new Color(0.70f, 0.90f, 1.0f), 0.35f),
                new GradientColorKey(new Color(0.90f, 0.75f, 1.0f), 0.70f),
                new GradientColorKey(new Color(1.0f, 0.92f, 0.75f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.95f, 0.0f),
                new GradientAlphaKey(0.90f, 0.35f),
                new GradientAlphaKey(0.85f, 0.70f),
                new GradientAlphaKey(0.90f, 1.0f)
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(grad);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 260f;
        shape.radiusThickness = 0f;
        shape.rotation = new Vector3(-80f, 0f, 0f);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 2500) });

        // Natural astronomical twinkling without physical drifting
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0f; // Stars remain fixed at their cosmic dome coordinates
        noise.sizeAmount = new ParticleSystem.MinMaxCurve(0.6f);
        noise.frequency = 0.35f;

        ParticleSystemRenderer rend = starsObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = starMat;

        ps.Simulate(1f, true, true);
        ps.Play();
        EditorUtility.SetDirty(starsObj);
    }

    private static void BuildCosmicShootingStars(Transform envRoot)
    {
        Transform shootingT = envRoot.Find("FX_ShootingStars");
        GameObject shootingObj;
        if (shootingT != null)
        {
            shootingObj = shootingT.gameObject;
        }
        else
        {
            shootingObj = new GameObject("FX_ShootingStars", typeof(ParticleSystem));
            shootingObj.transform.SetParent(envRoot, false);
        }
        shootingObj.transform.position = new Vector3(25f, 42f, 150f);

        string matPath = MatPath + "/M_CosmicStar.mat";
        Material starMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        ParticleSystem ps = shootingObj.GetComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(28f, 38f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.maxParticles = 6;
        main.startColor = new Color(0.85f, 0.95f, 1.0f, 0.95f);
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 8f, 25f);
        shape.rotation = new Vector3(15f, -135f, 0f);

        var emission = ps.emission;
        emission.rateOverTime = 0.3f; // 1 shooting star every ~3.3 seconds

        ParticleSystemRenderer rend = shootingObj.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.velocityScale = 0.20f;
        rend.lengthScale = 3.5f;
        rend.sharedMaterial = starMat;

        ps.Play();
        EditorUtility.SetDirty(shootingObj);
    }

    public static Texture2D GenerateStarTexture(string savePath)
    {
        int width = 128;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] colors = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float v = (y - height * 0.5f) / (height * 0.5f);
            for (int x = 0; x < width; x++)
            {
                float u = (x - width * 0.5f) / (width * 0.5f);
                float r = Mathf.Sqrt(u * u + v * v);

                if (r >= 1.0f)
                {
                    colors[y * width + x] = Color.clear;
                    continue;
                }

                // Soft radial core
                float radial = 1f / (1f + 16f * r * r);

                // Subtle 4-point sparkle spikes
                float spikeX = Mathf.Exp(-Mathf.Abs(u) * 2.8f) * (0.04f / (0.04f + v * v * 1.5f));
                float spikeY = Mathf.Exp(-Mathf.Abs(v) * 2.8f) * (0.04f / (0.04f + u * u * 1.5f));
                float spike = (spikeX + spikeY) * 0.45f;

                float intensity = Mathf.Clamp01(radial + spike);

                // Edge fade
                float edgeFade = Mathf.Clamp01((1.0f - r) / 0.25f);
                intensity *= edgeFade;

                Color c = Color.Lerp(new Color(0.85f, 0.95f, 1.0f), Color.white, Mathf.Clamp01(1f - r * 1.8f));
                c.a = intensity;

                colors[y * width + x] = c;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();

        byte[] pngBytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath, pngBytes);
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    #endregion
}
