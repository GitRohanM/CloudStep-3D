using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using UnityEditor.SceneManagement;

public static class Level01UIOverhaul
{
    private const string ScenePath = "Assets/_Project/Scenes/Level01.unity";
    private const string Root = "Assets/_Project";

    private static Sprite roundedSprite;
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
    private static TMP_FontAsset fontAsset;

    // [MenuItem("Tools/Platformer/Upgrade Level 01 UI")]
    public static void UpgradeLevel01UI()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        LoadVisualAssets();

        // 1. Destroy legacy HUD if present
        GameObject oldHud = GameObject.Find("HUD");
        if (oldHud != null) Undo.DestroyObjectImmediate(oldHud);

        GameObject oldCanvas = GameObject.Find("Canvas_GameUI");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        // 2. Ensure EventSystem with InputSystemUIInputModule
        GameObject esObj = GameObject.Find("EventSystem");
        if (esObj == null)
        {
            esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        }
        else
        {
            var oldModule = esObj.GetComponent<StandaloneInputModule>();
            if (oldModule != null) Object.DestroyImmediate(oldModule);
            var im = esObj.GetComponent<InputSystemUIInputModule>();
            if (im == null) im = esObj.AddComponent<InputSystemUIInputModule>();
            im.AssignDefaultActions();
        }

        // 3. Create Modern Canvas_GameUI
        GameObject canvasObj = new GameObject("Canvas_GameUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas_GameUI");

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 4. Build 5 Clean Screens
        CanvasGroup hudGroup = CreateCanvasGroup("GameplayHUD", canvasObj.transform);
        CoinTrackerRefs tracker = BuildHUD(hudGroup.transform);

        CanvasGroup startGroup = CreateCanvasGroup("StartMenu", canvasObj.transform);
        (Button startBtn, TMP_Text startTotalHint) = BuildStartScreen(startGroup.transform);

        CanvasGroup pauseGroup = CreateCanvasGroup("PauseMenu", canvasObj.transform);
        (Button resumeBtn, Button pauseRestartBtn, Button pauseMenuBtn) = BuildPauseScreen(pauseGroup.transform);

        CanvasGroup deathGroup = CreateCanvasGroup("DeathMenu", canvasObj.transform);
        (Button retryBtn, Button deathMenuBtn, TMP_Text deathScoreText) = BuildDeathScreen(deathGroup.transform);

        CanvasGroup winGroup = CreateCanvasGroup("WinMenu", canvasObj.transform);
        (Button nextLevelBtn, Button playAgainBtn, Button winMenuBtn, TMP_Text winScoreText) = BuildWinScreen(winGroup.transform);

        // Initial Visibility
        SetCanvasState(hudGroup, false);
        SetCanvasState(startGroup, true);
        SetCanvasState(pauseGroup, false);
        SetCanvasState(deathGroup, false);
        SetCanvasState(winGroup, false);

        // 5. Wire into GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject s = new SerializedObject(gm);

            s.FindProperty("scoreText").objectReferenceValue = tracker.Score;
            s.FindProperty("totalText").objectReferenceValue = tracker.Total;
            s.FindProperty("progressFill").objectReferenceValue = tracker.Progress;
            s.FindProperty("countPulseTarget").objectReferenceValue = tracker.Score.rectTransform;

            s.FindProperty("gameplayHud").objectReferenceValue = hudGroup;
            s.FindProperty("mainMenu").objectReferenceValue = startGroup;
            s.FindProperty("pauseMenu").objectReferenceValue = pauseGroup;
            s.FindProperty("deathMenu").objectReferenceValue = deathGroup;
            s.FindProperty("winMenu").objectReferenceValue = winGroup;

            s.FindProperty("mainMenuTotalText").objectReferenceValue = startTotalHint;
            s.FindProperty("deathScoreText").objectReferenceValue = deathScoreText;
            s.FindProperty("winScoreText").objectReferenceValue = winScoreText;

            s.FindProperty("startButton").objectReferenceValue = startBtn;
            s.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            s.FindProperty("pauseRestartButton").objectReferenceValue = pauseRestartBtn;
            s.FindProperty("pauseMenuButton").objectReferenceValue = pauseMenuBtn;
            s.FindProperty("hudPauseButton").objectReferenceValue = tracker.PauseBtn;
            s.FindProperty("retryButton").objectReferenceValue = retryBtn;
            s.FindProperty("deathMenuButton").objectReferenceValue = deathMenuBtn;
            s.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn;
            s.FindProperty("nextLevelButton").objectReferenceValue = nextLevelBtn;
            s.FindProperty("winMenuButton").objectReferenceValue = winMenuBtn;
            s.FindProperty("nextSceneName").stringValue = "Level02_SunsetArchipelago";

            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gm);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("<color=green><b>[Level01 UI Overhaul]</b> Successfully upgraded Level01 UI to match Level 02 and 03!</color>");

        OrganizeLevel01Hierarchy();
    }

    // [MenuItem("Tools/Platformer/Organize Level 01 Hierarchy")]
    public static void OrganizeLevel01Hierarchy()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject GetOrCreateRoot(string name)
        {
            GameObject go = GameObject.Find("/" + name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Create " + name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            return go;
        }

        GameObject envRoot = GetOrCreateRoot("_Environment");
        GameObject platformsRoot = GetOrCreateRoot("_Platforms");
        GameObject propsRoot = GetOrCreateRoot("_Props");
        GameObject collectiblesRoot = GetOrCreateRoot("_Collectibles");
        GameObject gameplayRoot = GetOrCreateRoot("_Gameplay");
        GameObject uiRoot = GetOrCreateRoot("_UI");
        GameObject lightingRoot = GetOrCreateRoot("_Lighting");

        void MoveTo(string objName, Transform parent)
        {
            GameObject go = GameObject.Find(objName);
            if (go != null && go.transform.parent != parent)
            {
                Undo.SetTransformParent(go.transform, parent, "Move " + objName);
            }
        }

        MoveTo("Quaternius_CloudLayers", envRoot.transform);
        MoveTo("Quaternius_FloatingIslandRocks", envRoot.transform);
        MoveTo("PlatformerCourse_Generated", platformsRoot.transform);
        MoveTo("Environment_Dressing", propsRoot.transform);

        var allRoots = scene.GetRootGameObjects();
        foreach (var r in allRoots)
        {
            if (r.name.StartsWith("Coin_"))
            {
                Undo.SetTransformParent(r.transform, collectiblesRoot.transform, "Move Coin");
            }
        }

        MoveTo("PlayerSpawn", gameplayRoot.transform);
        MoveTo("Player", gameplayRoot.transform);
        MoveTo("GameManager", gameplayRoot.transform);
        MoveTo("KillZone", gameplayRoot.transform);
        MoveTo("Finish_Trigger", gameplayRoot.transform);

        MoveTo("Canvas_GameUI", uiRoot.transform);
        MoveTo("EventSystem", uiRoot.transform);

        MoveTo("Directional Light", lightingRoot.transform);
        MoveTo("Main Camera", lightingRoot.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("<color=green><b>[Level01 Hierarchy]</b> Successfully organized Level01 into clean 7-container hierarchy!</color>");
    }

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

    private struct CoinTrackerRefs
    {
        public TMP_Text Score;
        public TMP_Text Total;
        public Image Progress;
        public Button PauseBtn;
    }

    private static CoinTrackerRefs BuildHUD(Transform parent)
    {
        // Top-Right Snug Coin Badge with Coin Icon placed on the RIGHT
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

        // 2. Total Text (Middle)
        TMP_Text totalText = CreateText(card, "TotalText", "/ 16", 21f, new Color(1f, 0.85f, 0.45f), TextAlignmentOptions.MidlineLeft, new Vector2(66f, -2f), new Vector2(58f, 38f), false);
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

        CreateHeaderRibbon(card, "EMERALD ARCHIPELAGO", greenBtnSprite, Color.white, new Vector2(460f, 54f), 270f);

        CreateText(card, "Eyebrow", "LEVEL 01 - THE ADVENTURE BEGINS", 15f, new Color(0.48f, 0.95f, 0.70f), TextAlignmentOptions.Center, new Vector2(0f, 195f), new Vector2(660f, 26f), true);
        CreateText(card, "Title", "CROSS THE FLOATING ISLES", 34f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 148f), new Vector2(660f, 46f), true);
        CreateText(card, "Subtitle", "Leap across vibrant emerald grass islands to reach the Sun Portal.", 15f, new Color(0.85f, 0.92f, 0.95f), TextAlignmentOptions.Center, new Vector2(0f, 105f), new Vector2(620f, 30f), false);

        CreateControlPill(card, "MovePill", "WASD / ARROWS", "MOVE", new Vector2(-160f, 32f));
        CreateControlPill(card, "JumpPill", "SPACE", "JUMP", new Vector2(160f, 32f));
        CreateControlPill(card, "LookPill", "MOUSE", "LOOK", new Vector2(-160f, -32f));
        CreateControlPill(card, "PausePill", "ESC / P", "PAUSE", new Vector2(160f, -32f));

        Button startBtn = CreateJuicyButton(card, "StartButton", "START RUN", greenBtnSprite, greenBtnFlatSprite, new Vector2(420f, 68f), new Vector2(0f, -118f), 28f, Color.white);

        TMP_Text totalCoinsHint = CreateText(card, "TotalCoinsHint", "16 EMERALD COINS HIDDEN ACROSS THE ISLES", 14f, new Color(0.95f, 0.85f, 0.65f, 0.88f), TextAlignmentOptions.Center, new Vector2(0f, -195f), new Vector2(600f, 28f), false);

        return (startBtn, totalCoinsHint);
    }

    private static (Button, Button, Button) BuildPauseScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(580f, 470f));

        CreateHeaderRibbon(card, "GAME PAUSED", blueBtnSprite, Color.white, new Vector2(360f, 52f), 235f);

        CreateText(card, "PauseSubtitle", "Take a breather. The emerald isles will wait for you.", 16f, new Color(0.85f, 0.90f, 0.96f), TextAlignmentOptions.Center, new Vector2(0f, 145f), new Vector2(520f, 32f), false);

        Button resumeBtn = CreateJuicyButton(card, "ResumeButton", "RESUME", greenBtnSprite, greenBtnFlatSprite, new Vector2(380f, 64f), new Vector2(0f, 50f), 24f, Color.white);
        Button restartBtn = CreateJuicyButton(card, "RestartButton", "RESTART LEVEL", yellowBtnSprite, yellowBtnFlatSprite, new Vector2(380f, 60f), new Vector2(0f, -28f), 22f, new Color(0.14f, 0.11f, 0.05f));
        Button menuBtn = CreateJuicyButton(card, "PauseMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(340f, 52f), new Vector2(0f, -102f), 19f, new Color(0.18f, 0.20f, 0.28f));

        return (resumeBtn, restartBtn, menuBtn);
    }

    private static (Button, Button, TMP_Text) BuildDeathScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(600f, 500f));

        CreateHeaderRibbon(card, "FELL INTO THE OCEAN", redBtnSprite, Color.white, new Vector2(400f, 54f), 250f);

        CreateText(card, "DeathSubtitle", "The ocean depths caught you. Dust yourself off and leap again!", 16f, new Color(0.95f, 0.85f, 0.85f), TextAlignmentOptions.Center, new Vector2(0f, 160f), new Vector2(540f, 32f), false);

        RectTransform statsPlaque = CreateRect("StatsBox", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380f, 70f), new Vector2(0f, 75f));
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
        iconRect.sizeDelta = new Vector2(42f, 42f);
        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = coinBadgeSprite;
        icon.preserveAspect = true;

        CreateText(statsPlaque, "DeathScoreLabel", "COINS COLLECTED BEFORE FALL", 13f, new Color(1f, 0.70f, 0.70f), TextAlignmentOptions.MidlineLeft, new Vector2(70f, 12f), new Vector2(250f, 22f), true);
        TMP_Text deathScore = CreateText(statsPlaque, "DeathScoreText", "00 / 16", 26f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(70f, -12f), new Vector2(250f, 30f), true);

        Button retryBtn = CreateJuicyButton(card, "RetryButton", "TRY AGAIN", redBtnSprite, redBtnFlatSprite, new Vector2(400f, 68f), new Vector2(0f, -35f), 26f, Color.white);
        Button menuBtn = CreateJuicyButton(card, "DeathMenuButton", "MAIN MENU", greyBtnSprite, greyBtnFlatSprite, new Vector2(360f, 52f), new Vector2(0f, -120f), 20f, new Color(0.18f, 0.20f, 0.28f));

        return (retryBtn, menuBtn, deathScore);
    }

    private static (Button, Button, Button, TMP_Text) BuildWinScreen(Transform parent)
    {
        RectTransform card = BuildModalCard(parent, new Vector2(660f, 620f));

        CreateHeaderRibbon(card, "PORTAL REACHED!", yellowBtnSprite, new Color(0.14f, 0.11f, 0.05f), new Vector2(440f, 54f), 310f);

        CreateText(card, "WinSubtitle", "Spectacular run! You mastered the Emerald Archipelago!", 16f, new Color(0.88f, 0.95f, 0.92f), TextAlignmentOptions.Center, new Vector2(0f, 220f), new Vector2(600f, 32f), false);

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
        CreateScreenScrim(screenRoot, new Color(0.04f, 0.04f, 0.08f, 0.78f));

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
        bg.color = new Color(0.18f, 0.22f, 0.32f, 0.95f);

        CreateText(pill, "Keys", keys, 17f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 7f), new Vector2(280f, 22f), true);
        CreateText(pill, "Action", action, 12f, new Color(0.48f, 0.94f, 0.78f), TextAlignmentOptions.Center, new Vector2(0f, -11f), new Vector2(280f, 16f), false);
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

        TMP_Text tmp = CreateText(btnRect, "Text", label, fontSize, textColor ?? Color.white, TextAlignmentOptions.Center, new Vector2(0f, 3f), size, true);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.sizeDelta = Vector2.zero;

        return btn;
    }

    private static void CreateScreenScrim(Transform parent, Color color)
    {
        GameObject scrimObj = new GameObject("Scrim", typeof(RectTransform), typeof(Image));
        scrimObj.transform.SetParent(parent, false);
        scrimObj.transform.SetAsFirstSibling();
        RectTransform rect = scrimObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        Image img = scrimObj.GetComponent<Image>();
        img.color = color;
    }

    private static CanvasGroup CreateCanvasGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        return cg;
    }

    private static void SetCanvasState(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
        cg.gameObject.SetActive(visible);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        return rect;
    }

    private static TMP_Text CreateText(Transform parent, string name, string content, float fontSize, Color color, TextAlignmentOptions align, Vector2 pos, Vector2 size, bool bold = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }
}
