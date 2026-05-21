#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity 메뉴 → LuckSlot → 전체 씬 자동 생성 ★  실행
/// </summary>
public static class LuckSlotSceneBuilder
{
    // ── 컬러 팔레트 ────────────────────────────────────────────
    static readonly Color32 C_BgDeep    = new Color32(20,  12,  40, 255);
    static readonly Color32 C_BgMid     = new Color32(35,  20,  65, 255);
    static readonly Color32 C_ReelBg    = new Color32(12,   6,  25, 255);
    static readonly Color32 C_Gold      = new Color32(255, 200,  30, 255);
    static readonly Color32 C_Cream     = new Color32(255, 248, 225, 255);
    static readonly Color32 C_DarkText  = new Color32( 55,  30,  80, 255);
    static readonly Color32 C_SubText   = new Color32(120,  90, 150, 255);
    static readonly Color32 C_BlueBtn   = new Color32( 70, 140, 255, 255);
    static readonly Color32 C_PurpleBtn = new Color32(140,  80, 210, 255);
    static readonly Color32 C_RedBtn    = new Color32(210,  60,  60, 255);
    static readonly Color32 C_Divider   = new Color32(200, 185, 150, 120);

    // ── 진입점 ─────────────────────────────────────────────────
    [MenuItem("LuckSlot/전체 씬 자동 생성 ★")]
    public static void BuildScene()
    {
        // 기존 오브젝트 제거
        foreach (string n in new[] { "Canvas", "AudioManager", "ShareManager",
                                      "ServerLuckManager", "GameManager", "EventSystem" })
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }

        // 폴더 보장
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // ScriptableObject
        LuckDataSO luckData = AssetDatabase.LoadAssetAtPath<LuckDataSO>("Assets/Data/LuckData.asset");
        if (luckData == null)
        {
            luckData = ScriptableObject.CreateInstance<LuckDataSO>();
            AssetDatabase.CreateAsset(luckData, "Assets/Data/LuckData.asset");
        }

        // ── 비(非) UI 싱글턴 ───────────────────────────────────
        var audioGO  = new GameObject("AudioManager");
        var audioMgr = audioGO.AddComponent<AudioManager>();
        var sfxSrc   = audioGO.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        Link(audioMgr, "sfxSource", sfxSrc);

        var shareMgr = new GameObject("ShareManager").AddComponent<ShareManager>();
        new GameObject("ServerLuckManager").AddComponent<ServerLuckManager>();

        // ── Canvas ─────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler   = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — 컴파일 타임에 Input System 패키지 유무 판별
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        AddInputModule(esGO);

        Transform ct = canvasGO.transform;

        // ── Background ─────────────────────────────────────────
        var bgRT = Stretch(AddImage(NewRT("Background", ct), C_BgDeep));
        bgRT.gameObject.AddComponent<BackgroundStarEffect>();

        // ── Main Panel ─────────────────────────────────────────
        RectTransform mainPanel = Stretch(NewRT("MainPanel", ct));

        // 타이틀
        RectTransform titleRT = NewTMP("TitleText", mainPanel,
            "오늘의 행운 뽑기", 55, Color.white);
        Anchors(titleRT, 0f, 0.88f, 1f, 0.97f);
        titleRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 슬롯 영역
        RectTransform slotAreaRT = NewRT("SlotArea", mainPanel);
        AddImage(slotAreaRT, C_BgMid);
        Anchors(slotAreaRT, 0.03f, 0.37f, 0.97f, 0.80f);
        var hlg = slotAreaRT.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = hlg.childControlHeight = true;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = true;
        hlg.spacing = 8;
        hlg.padding = new RectOffset(15, 15, 15, 15);

        SlotReel reelItem   = BuildReel("Item",    slotAreaRT, "행운 물건", true);
        SlotReel reelNumber = BuildReel("Number",  slotAreaRT, "행운 숫자", false);
        SlotReel reelMsg    = BuildReel("Message", slotAreaRT, "행운 문구", false);

        // 상태 텍스트
        RectTransform statusRT = NewTMP("StatusText", mainPanel, "", 34, C_Gold);
        Anchors(statusRT, 0.05f, 0.30f, 0.95f, 0.37f);
        statusRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 스핀 버튼
        var (spinBtnGO, spinBtn, spinBtnTMP) =
            BuildButton("SpinButton", mainPanel, "오늘의 행운 뽑기!", C_Gold, C_DarkText, 46);
        Anchors(spinBtnGO.GetComponent<RectTransform>(), 0.08f, 0.07f, 0.92f, 0.17f);
        spinBtnGO.AddComponent<ButtonScaleAnimation>();

        // SlotMachineController (빈 자식에 부착)
        var smcGO = new GameObject("SlotMachineController");
        smcGO.transform.SetParent(mainPanel, false);
        smcGO.AddComponent<RectTransform>();
        var smc = smcGO.AddComponent<SlotMachineController>();
        Link(smc, "itemReel",       reelItem);
        Link(smc, "numberReel",     reelNumber);
        Link(smc, "messageReel",    reelMsg);
        Link(smc, "spinButton",     spinBtn);
        Link(smc, "spinButtonText", spinBtnTMP);
        Link(smc, "statusText",     statusRT.GetComponent<TextMeshProUGUI>());
        Link(smc, "luckData",       luckData);

        // ── Result Panel ───────────────────────────────────────
        var (rcUI, resultPanelGO) = BuildResultPanel(ct);
        resultPanelGO.SetActive(false);
        // 공유 버튼이 스크린샷을 찍을 대상 패널 연결
        Link(shareMgr, "shareTargetPanel", resultPanelGO.GetComponent<RectTransform>());

        // ── History Panel ──────────────────────────────────────
        HistoryPanel histComp      = BuildHistoryPanel(ct);
        HistoryItemUI histPrefab   = EnsureHistoryItemPrefab();
        Link(histComp, "historyItemPrefab", histPrefab);

        // ResultCardUI ↔ HistoryPanel 연결
        Link(rcUI, "historyPanel", histComp);

        // ── GameManager ────────────────────────────────────────
        var gmGO = new GameObject("GameManager");
        var gm   = gmGO.AddComponent<GameManager>();
        Link(gm, "slotMachine", smc);
        Link(gm, "resultCard",  rcUI);
        Link(gm, "luckData",    luckData);

        // ── Toast UI ───────────────────────────────────────────
        BuildToast(ct);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료 ✓",
            "씬 자동 생성 완료!\n\n수동 작업이 남아있습니다:\n\n" +
            "① LuckData.asset (Assets/Data/)\n" +
            "   → Lucky Item Icons 배열에 Sprite 드래그\n\n" +
            "② AudioManager\n" +
            "   → Spin Start / Reel Stop / Card Reveal / Button Click Clip 연결\n\n" +
            "③ TMP가 없다면:\n" +
            "   Window > TextMeshPro > Import TMP Essential Resources",
            "확인");

        Selection.activeGameObject = canvasGO;
    }

    // ── Result Panel 전체 구성 ─────────────────────────────────
    static (ResultCardUI rcUI, GameObject panelGO) BuildResultPanel(Transform ct)
    {
        // ── 풀스크린 딤 오버레이 ──────────────────────────────
        // 전체 화면을 덮어 스핀 버튼 등 하위 UI 입력을 완전히 차단
        var panelGO = new GameObject("ResultPanel");
        panelGO.transform.SetParent(ct, false);
        RectTransform panelRT = panelGO.AddComponent<RectTransform>();
        Stretch(panelRT);
        AddImage(panelRT, new Color32(10, 5, 25, 200)); // 반투명 어두운 오버레이
        panelGO.AddComponent<CanvasGroup>();

        // ── 카드 (기존 bounds 유지) ───────────────────────────
        var cardGO = new GameObject("Card");
        cardGO.transform.SetParent(panelGO.transform, false);
        var cardRT = cardGO.AddComponent<RectTransform>();
        Anchors(cardRT, 0.04f, 0.12f, 0.96f, 0.88f);
        AddImage(cardRT, C_Cream);

        Transform p = cardGO.transform;

        // 카드 제목
        RectTransform rcTitleRT = NewTMP("CardTitle", p, "오늘의 행운 카드", 50, C_DarkText);
        Anchors(rcTitleRT, 0.02f, 0.85f, 0.98f, 0.98f);
        var rcTitleTMP = rcTitleRT.GetComponent<TextMeshProUGUI>();
        rcTitleTMP.alignment = TextAlignmentOptions.Center;
        rcTitleTMP.fontStyle = FontStyles.Bold;

        // 날짜
        RectTransform dateRT = NewTMP("DateText", p, "2026년 05월 20일", 33, C_SubText);
        Anchors(dateRT, 0.04f, 0.75f, 0.96f, 0.84f);
        dateRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 구분선
        Anchors(AddImage(NewRT("Divider1", p), C_Divider), 0.05f, 0.737f, 0.95f, 0.744f);

        // 행운 물건 행 — 레이블·아이콘·값 모두 좌/우 영역 내 중앙 정렬
        RectTransform iLabelRT = NewTMP("Label_Item", p, "행운 물건", 33, C_DarkText);
        Anchors(iLabelRT, 0.04f, 0.62f, 0.46f, 0.73f);
        iLabelRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var iconGO  = new GameObject("ItemIconImage");
        iconGO.transform.SetParent(p, false);
        var iconRT  = iconGO.AddComponent<RectTransform>();
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        Anchors(iconRT, 0.44f, 0.615f, 0.60f, 0.735f);

        RectTransform iValueRT = NewTMP("ItemValue", p, "—", 38, C_DarkText);
        Anchors(iValueRT, 0.60f, 0.62f, 0.98f, 0.73f);
        var iValueTMP = iValueRT.GetComponent<TextMeshProUGUI>();
        iValueTMP.alignment = TextAlignmentOptions.Center;
        iValueTMP.fontStyle = FontStyles.Bold;

        // 구분선
        Anchors(AddImage(NewRT("Divider2", p), C_Divider), 0.05f, 0.607f, 0.95f, 0.614f);

        // 행운 숫자 행 — 50/50 분할 후 양쪽 모두 중앙 정렬
        RectTransform nLabelRT = NewTMP("Label_Number", p, "행운 숫자", 33, C_DarkText);
        Anchors(nLabelRT, 0.04f, 0.49f, 0.50f, 0.60f);
        nLabelRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        RectTransform nValueRT = NewTMP("NumberValue", p, "—", 33, C_DarkText);
        Anchors(nValueRT, 0.50f, 0.49f, 0.98f, 0.60f);
        var nValueTMP = nValueRT.GetComponent<TextMeshProUGUI>();
        nValueTMP.alignment = TextAlignmentOptions.Center;

        // 구분선
        Anchors(AddImage(NewRT("Divider3", p), C_Divider), 0.05f, 0.477f, 0.95f, 0.484f);

        // 행운 문구 행 — 버튼 바로 위까지 공간 확보, AutoSizing으로 전체 표시
        RectTransform mLabelRT = NewTMP("Label_Message", p, "오늘의 메시지", 30, C_DarkText);
        Anchors(mLabelRT, 0.04f, 0.39f, 0.98f, 0.47f);
        mLabelRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        RectTransform mValueRT = NewTMP("MessageValue", p, "—", 55, new Color32(80, 50, 110, 255));
        Anchors(mValueRT, 0.04f, 0.155f, 0.98f, 0.39f);
        var mValueTMP = mValueRT.GetComponent<TextMeshProUGUI>();
        mValueTMP.alignment = TextAlignmentOptions.Center;
        mValueTMP.enableWordWrapping = true;
        mValueTMP.fontStyle = FontStyles.Italic;

        // 공유 / 기록 버튼
        var (shareBtnGO, shareBtn, _) =
            BuildButton("ShareButton", p, "공유하기", C_BlueBtn, Color.white, 34);
        Anchors(shareBtnGO.GetComponent<RectTransform>(), 0.04f, 0.04f, 0.48f, 0.15f);
        shareBtnGO.AddComponent<ButtonScaleAnimation>();

        var (histBtnGO, histBtn, _) =
            BuildButton("HistoryButton", p, "기록 보기", C_PurpleBtn, Color.white, 34);
        Anchors(histBtnGO.GetComponent<RectTransform>(), 0.52f, 0.04f, 0.96f, 0.15f);
        histBtnGO.AddComponent<ButtonScaleAnimation>();

        // 스파클 이펙트 컨테이너 — 카드 위에 렌더링되도록 마지막 자식으로 추가
        var sparkleGO = new GameObject("SparkleContainer");
        sparkleGO.transform.SetParent(p, false);
        Stretch(sparkleGO.AddComponent<RectTransform>());
        sparkleGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
        var sparkle = sparkleGO.AddComponent<CardSparkleEffect>();

        // ResultCardUI 컴포넌트 부착 & 연결
        ResultCardUI rcUI = panelGO.AddComponent<ResultCardUI>();
        Link(rcUI, "cardPanel",        panelGO);
        Link(rcUI, "dateText",         dateRT.GetComponent<TextMeshProUGUI>());
        Link(rcUI, "itemValueText",    iValueTMP);
        Link(rcUI, "numberValueText",  nValueTMP);
        Link(rcUI, "messageValueText", mValueTMP);
        Link(rcUI, "itemIconImage",    iconImg);
        Link(rcUI, "shareButton",      shareBtn);
        Link(rcUI, "historyButton",    histBtn);
        Link(rcUI, "sparkleEffect",    sparkle);
        // historyPanel 은 BuildScene 에서 연결

        return (rcUI, panelGO);
    }

    // ── History Panel 전체 구성 ────────────────────────────────
    static HistoryPanel BuildHistoryPanel(Transform ct)
    {
        // 컨테이너 (항상 활성 — HistoryPanel.cs 부착)
        var containerGO = new GameObject("HistoryPanelContainer");
        containerGO.transform.SetParent(ct, false);
        Stretch(containerGO.AddComponent<RectTransform>());
        HistoryPanel histComp = containerGO.AddComponent<HistoryPanel>();

        // 어두운 오버레이 (PanelRoot — 초기 비활성)
        var dimGO = new GameObject("PanelRoot");
        dimGO.transform.SetParent(containerGO.transform, false);
        Stretch(dimGO.AddComponent<RectTransform>());
        AddImage(dimGO.GetComponent<RectTransform>(), new Color32(0, 0, 0, 190));

        // 흰 카드
        var cardGO = new GameObject("Card");
        cardGO.transform.SetParent(dimGO.transform, false);
        RectTransform cardRT = cardGO.AddComponent<RectTransform>();
        Anchors(cardRT, 0.04f, 0.08f, 0.96f, 0.92f);
        AddImage(cardRT, C_Cream);

        Transform cardT = cardGO.transform;

        // 카드 타이틀
        RectTransform hTitleRT = NewTMP("Title", cardT, "행운 기록", 50, C_DarkText);
        Anchors(hTitleRT, 0.02f, 0.88f, 0.98f, 0.99f);
        hTitleRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        hTitleRT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // ScrollView
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(cardT, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        Anchors(scrollRT, 0.02f, 0.11f, 0.98f, 0.86f);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport
        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
        Stretch(viewportRT);
        var vpImg = viewportGO.AddComponent<Image>();
        vpImg.color = Color.white; // 스텐실 마스크가 정상 기록되려면 불투명해야 함; 시각적으로는 showMaskGraphic이 숨김
        var mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content  = contentRT;

        // 닫기 버튼
        var (closeBtnGO, closeBtn, _) =
            BuildButton("CloseButton", cardT, "닫기", C_RedBtn, Color.white, 38);
        Anchors(closeBtnGO.GetComponent<RectTransform>(), 0.25f, 0.02f, 0.75f, 0.10f);
        closeBtnGO.AddComponent<ButtonScaleAnimation>();

        dimGO.SetActive(false);

        Link(histComp, "panelRoot",     dimGO);
        Link(histComp, "contentParent", contentRT.transform);
        Link(histComp, "closeButton",   closeBtn);

        return histComp;
    }

    // ── 릴 하나 구성 ───────────────────────────────────────────
    static SlotReel BuildReel(string type, RectTransform slotAreaRT, string labelText, bool withIcon)
    {
        var containerGO = new GameObject($"Reel_{type}_Container");
        containerGO.transform.SetParent(slotAreaRT, false);
        containerGO.AddComponent<RectTransform>();
        var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 6;

        // 레이블
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(containerGO.transform, false);
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredHeight = 50;
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = labelText;
        labelTMP.fontSize = 28;
        labelTMP.color = C_Gold;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.fontStyle = FontStyles.Bold;

        // 릴 박스
        var reelGO = new GameObject($"Reel_{type}");
        reelGO.transform.SetParent(containerGO.transform, false);
        reelGO.AddComponent<RectTransform>();
        AddImage(reelGO.GetComponent<RectTransform>(), C_ReelBg);
        var reelLE = reelGO.AddComponent<LayoutElement>();
        reelLE.flexibleHeight = 1;

        // 릴 텍스트
        var reelTextGO = new GameObject("ReelText");
        reelTextGO.transform.SetParent(reelGO.transform, false);
        Stretch(reelTextGO.AddComponent<RectTransform>());
        var reelTMP = reelTextGO.AddComponent<TextMeshProUGUI>();
        reelTMP.text = "?";
        reelTMP.color = Color.white;
        reelTMP.alignment = TextAlignmentOptions.Center;
        reelTMP.fontStyle = FontStyles.Bold;

        if (type == "Message")
        {
            // 행운 문구는 길이가 가변적이므로 자동 크기 조정 + 줄바꿈
            reelTMP.enableAutoSizing = true;
            reelTMP.fontSizeMin = 16;
            reelTMP.fontSizeMax = 52;
            reelTMP.enableWordWrapping = true;
        }
        else
        {
            reelTMP.fontSize = 52;
        }

        // 릴 멈춤 강조 오버레이 (평소 alpha=0, 멈출 때 펄스)
        var glowGO  = new GameObject("ReelGlow");
        glowGO.transform.SetParent(reelGO.transform, false);
        Stretch(glowGO.AddComponent<RectTransform>());
        var glowImg = glowGO.AddComponent<Image>();
        glowImg.color = new Color32(255, 220, 60, 0); // 골드, 초기 투명

        // SlotReel 컴포넌트
        var slotReel = reelGO.AddComponent<SlotReel>();
        Link(slotReel, "displayText", reelTMP);
        Link(slotReel, "glowImage",   glowImg);

        // 아이콘 (행운 물건 릴만)
        if (withIcon)
        {
            var iconGO  = new GameObject("ReelIcon");
            iconGO.transform.SetParent(reelGO.transform, false);
            var iconReelRT = iconGO.AddComponent<RectTransform>();
            Image iconImg  = iconGO.AddComponent<Image>();
            Anchors(iconReelRT, 0.15f, 0.03f, 0.85f, 0.43f);
            iconGO.SetActive(false);
            Link(slotReel, "iconDisplay", iconImg);
        }

        return slotReel;
    }

    // ── HistoryItemUI 프리팹 ───────────────────────────────────
    static HistoryItemUI EnsureHistoryItemPrefab()
    {
        const string path = "Assets/Prefabs/HistoryItemUI.prefab";
        // 씬 재생성 시 항상 새로 만들어 내부 링크(텍스트 필드 등) 누락 방지
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var rootGO = new GameObject("HistoryItemUI");
        RectTransform rootRT = rootGO.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(0, 165);
        AddImage(rootRT, new Color32(245, 238, 220, 255));
        var le = rootGO.AddComponent<LayoutElement>();
        le.preferredHeight = 165;
        le.flexibleWidth   = 1;

        // 날짜 (상단)
        RectTransform dateRT = NewTMP("DateText", rootGO.transform,
            "2026년 05월 20일", 28, C_DarkText);
        Anchors(dateRT, 0.02f, 0.62f, 0.98f, 0.95f);
        dateRT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // 물건 / 숫자 (중단 좌우)
        RectTransform itemRT = NewTMP("ItemText", rootGO.transform,
            "물건: —", 26, C_DarkText);
        Anchors(itemRT, 0.02f, 0.36f, 0.52f, 0.62f);

        RectTransform numRT = NewTMP("NumberText", rootGO.transform,
            "숫자: —", 26, C_DarkText);
        Anchors(numRT, 0.52f, 0.36f, 0.98f, 0.62f);
        numRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // 문구 (하단)
        RectTransform msgRT = NewTMP("MessageText", rootGO.transform,
            "—", 24, new Color32(80, 55, 110, 255));
        Anchors(msgRT, 0.02f, 0.02f, 0.98f, 0.36f);
        var msgTMP = msgRT.GetComponent<TextMeshProUGUI>();
        msgTMP.enableWordWrapping = true;
        msgTMP.fontStyle = FontStyles.Italic;

        // HistoryItemUI 컴포넌트
        var hiUI = rootGO.AddComponent<HistoryItemUI>();
        Link(hiUI, "dateText",    dateRT.GetComponent<TextMeshProUGUI>());
        Link(hiUI, "itemText",    itemRT.GetComponent<TextMeshProUGUI>());
        Link(hiUI, "numberText",  numRT.GetComponent<TextMeshProUGUI>());
        Link(hiUI, "messageText", msgTMP);

        var prefab = PrefabUtility.SaveAsPrefabAsset(rootGO, path);
        Object.DestroyImmediate(rootGO);
        return prefab.GetComponent<HistoryItemUI>();
    }

    // ── Toast UI 생성 ──────────────────────────────────────────
    static void BuildToast(Transform ct)
    {
        var toastGO = new GameObject("ToastUI");
        toastGO.transform.SetParent(ct, false);
        RectTransform toastRT = toastGO.AddComponent<RectTransform>();
        // 화면 하단 중앙, 다른 UI 위에 렌더링되도록 Canvas 마지막에 추가
        Anchors(toastRT, 0.10f, 0.08f, 0.90f, 0.17f);

        AddImage(toastRT, new Color32(30, 30, 30, 220));

        var cg = toastGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        RectTransform textRT = NewTMP("Text", toastGO.transform, "", 32, Color.white);
        Stretch(textRT);
        var tmp = textRT.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;

        var toast = toastGO.AddComponent<ToastUI>();
        Link(toast, "canvasGroup",  cg);
        Link(toast, "messageText",  tmp);
    }

    // ── 버튼 생성 헬퍼 ─────────────────────────────────────────
    static (GameObject go, Button btn, TextMeshProUGUI labelTMP) BuildButton(
        string name, Transform parent, string labelText, Color bgColor, Color textColor, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        AddImage(go.GetComponent<RectTransform>(), bgColor);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.15f;
        cb.pressedColor     = bgColor * 0.80f;
        btn.colors = cb;

        var labelGO = new GameObject("Text");
        labelGO.transform.SetParent(go.transform, false);
        Stretch(labelGO.AddComponent<RectTransform>());
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = labelText;
        labelTMP.fontSize  = fontSize;
        labelTMP.color     = textColor;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.fontStyle = FontStyles.Bold;

        return (go, btn, labelTMP);
    }

    // ── 저수준 UI 헬퍼 ─────────────────────────────────────────

    static RectTransform NewRT(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    // TMP 노드: 부모에 자식 GO 생성, RectTransform + TMP 추가
    static RectTransform NewTMP(string name, Transform parent,
        string text, float size, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = size;
        tmp.color    = color;
        return rt;
    }

    static RectTransform AddImage(RectTransform rt, Color color)
    {
        rt.gameObject.AddComponent<Image>().color = color;
        return rt;
    }

    static RectTransform AddImage(GameObject go, Color color)
        => AddImage(go.GetComponent<RectTransform>(), color);

    static RectTransform Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    static void Anchors(RectTransform rt,
        float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // SerializedObject 를 통해 [SerializeField] private 필드 연결
    static void Link(Object target, string fieldName, Object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning(
                $"[SceneBuilder] '{target.GetType().Name}' 에서 필드 '{fieldName}' 를 찾지 못했습니다.");
        }
    }

    // ── Input System 모듈 헬퍼 ────────────────────────────────
    // #if 전처리기로 컴파일 타임에 결정 — 런타임 리플렉션보다 안전
    static void AddInputModule(GameObject esGO)
    {
#if ENABLE_INPUT_SYSTEM
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        esGO.AddComponent<StandaloneInputModule>();
#endif
    }

    // ── 별도 메뉴: EventSystem 모듈만 교체 ───────────────────
    [MenuItem("LuckSlot/EventSystem 모듈 교체 (Input System 오류 수정)")]
    public static void FixEventSystem()
    {
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            Debug.LogError("[LuckSlot] 씬에 EventSystem 이 없습니다.");
            return;
        }

        // StandaloneInputModule 제거
        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null)
        {
            Object.DestroyImmediate(old);
            Debug.Log("[LuckSlot] StandaloneInputModule 제거 완료");
        }

        // 올바른 모듈 추가
#if ENABLE_INPUT_SYSTEM
        if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("[LuckSlot] InputSystemUIInputModule 추가 완료");
        }
        else
        {
            Debug.Log("[LuckSlot] InputSystemUIInputModule 이미 있음 — 변경 없음");
        }
#else
        if (es.GetComponent<StandaloneInputModule>() == null)
        {
            es.gameObject.AddComponent<StandaloneInputModule>();
            Debug.Log("[LuckSlot] StandaloneInputModule 추가 완료");
        }
#endif
        EditorUtility.SetDirty(es.gameObject);
    }

    [MenuItem("LuckSlot/테스트: 하루 제한 초기화")]
    public static void ClearDailyLimit()
    {
        SaveManager.ClearSaveData();
        LuckHistoryManager.ClearHistory();
        Debug.Log("[LuckSlot] 하루 제한 + 히스토리 초기화 완료. Play 재시작하세요.");
    }

    // ── 폰트 일괄 적용 ───────────────────────────────────────
    [MenuItem("LuckSlot/폰트 일괄 적용 (Maplestory Bold SDF)")]
    public static void ApplyMaplestoryFont()
    {
        // 프로젝트 전체에서 폰트 에셋 검색
        string[] guids = AssetDatabase.FindAssets("Maplestory Bold SDF t:TMP_FontAsset");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("폰트 에셋 없음",
                "프로젝트에서 'Maplestory Bold SDF' 폰트 에셋을 찾지 못했습니다.\n\n" +
                "아래 순서로 만들어 주세요:\n\n" +
                "① 맵플스토리 .otf/.ttf 파일을 Assets/Fonts/ 에 넣기\n" +
                "② Window > TextMeshPro > Font Asset Creator\n" +
                "③ Source Font: Maplestory Bold 선택\n" +
                "④ Atlas Population Mode: Dynamic  ← 한글 필수\n" +
                "⑤ Generate Font Atlas → Save\n" +
                "⑥ 파일명을 'Maplestory Bold SDF' 로 저장\n" +
                "⑦ 이 메뉴 다시 실행",
                "확인");
            return;
        }

        string fontPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);

        if (font == null)
        {
            Debug.LogError("[LuckSlot] 폰트 에셋 로드 실패: " + fontPath);
            return;
        }

        // Dynamic 여부 확인 (Static 이면 한글 깨짐 가능)
        bool isDynamic = font.atlasPopulationMode == AtlasPopulationMode.Dynamic;

        // 씬의 모든 TextMeshProUGUI 에 적용
        var allTMPs = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in allTMPs)
        {
            Undo.RecordObject(tmp, "Apply Maplestory Bold SDF");
            tmp.font = font;
            EditorUtility.SetDirty(tmp);
        }

        // HistoryItemUI 프리팹에도 적용
        string prefabPath = "Assets/Prefabs/HistoryItemUI.prefab";
        var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabGO != null)
        {
            var prefabTMPs = prefabGO.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in prefabTMPs)
            {
                Undo.RecordObject(tmp, "Apply Maplestory Bold SDF");
                tmp.font = font;
                EditorUtility.SetDirty(tmp);
            }
            AssetDatabase.SaveAssets();
        }

        string msg = $"씬 {allTMPs.Length}개 + 프리팹 적용 완료\n경로: {fontPath}";

        if (!isDynamic)
            msg += "\n\n⚠️  폰트가 Static 모드입니다.\n" +
                   "한글이 깨진다면 Font Asset Creator 에서\n" +
                   "Atlas Population Mode → Dynamic 으로 재생성하세요.";

        EditorUtility.DisplayDialog("폰트 적용 완료", msg, "확인");
        Debug.Log($"[LuckSlot] Maplestory Bold SDF 적용: 씬 {allTMPs.Length}개, Dynamic={isDynamic}");
    }
}
#endif
