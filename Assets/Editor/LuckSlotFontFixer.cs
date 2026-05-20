#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

// Unity 6 TMP API 기준으로 작성
// · AtlasPopulationMode → TMPro.AtlasPopulationMode 명시 (TextCore 충돌 방지)
// · GlyphRenderMode     → UnityEngine.TextCore.LowLevel.GlyphRenderMode 명시
// · TryAddCharacters    → uint[] 버전, out 없는 오버로드 사용
public static class LuckSlotFontFixer
{
    const string SDF_PATH    = "Assets/Fonts/Maplestory Bold SDF.asset";
    const string FONT_FOLDER = "Assets/Fonts";

    [MenuItem("LuckSlot/한글 폰트 에셋 재생성 ★")]
    public static void FixKoreanFont()
    {
        if (!AssetDatabase.IsValidFolder(FONT_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Fonts");

        Font srcFont = FindSourceFont();
        if (srcFont == null)
        {
            EditorUtility.DisplayDialog("소스 폰트 없음",
                "Maplestory Bold .otf/.ttf 파일을 Assets 폴더에 넣어주세요.\n\n" +
                "파일명에 'Maplestory' 와 'Bold' 가 포함되어야 합니다.\n" +
                "(예: MapleStoryBold.otf, Maplestory Bold.ttf)", "확인");
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SDF_PATH);

        TMP_FontAsset fontAsset;

        if (existing != null &&
            existing.atlasPopulationMode == TMPro.AtlasPopulationMode.Dynamic)
        {
            // 기존 Dynamic 에셋 재사용: 문자만 추가
            fontAsset = existing;
            Debug.Log("[FontFixer] 기존 Dynamic 에셋 재사용");
        }
        else
        {
            // Static 이거나 없음 → Dynamic 에셋 새로 생성
            if (existing != null)
                AssetDatabase.DeleteAsset(SDF_PATH);

            fontAsset = TMP_FontAsset.CreateFontAsset(
                srcFont,
                samplingPointSize:       90,
                atlasPadding:            9,
                renderMode:              UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                atlasWidth:              4096,
                atlasHeight:             4096,
                atlasPopulationMode:     TMPro.AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            fontAsset.name = "Maplestory Bold SDF";

            AssetDatabase.CreateAsset(fontAsset, SDF_PATH);

            if (fontAsset.material != null)
                AssetDatabase.AddObjectToAsset(fontAsset.material, SDF_PATH);

            foreach (var tex in fontAsset.atlasTextures)
                if (tex != null)
                    AssetDatabase.AddObjectToAsset(tex, SDF_PATH);

            Debug.Log($"[FontFixer] 새 Dynamic 에셋 생성: {SDF_PATH}");
        }

        // 한글 + Latin 문자 사전 추가 (uint[] 버전, out 없음)
        uint[] chars = BuildUnicodeArray();
        fontAsset.TryAddCharacters(chars);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 저장 후 다시 로드해서 참조 안정화
        var saved = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SDF_PATH);
        int count = ApplyToScene(saved);

        EditorUtility.DisplayDialog("완료 ✓",
            $"한글 폰트 에셋 생성 + 씬 {count}개 적용 완료!\n\n경로: {SDF_PATH}", "확인");
    }

    // ── 유니코드 배열 생성 ──────────────────────────────────────
    static uint[] BuildUnicodeArray()
    {
        var list = new System.Collections.Generic.List<uint>(12000);

        for (uint c = 0x0020; c <= 0x007E; c++) list.Add(c); // Basic Latin
        for (uint c = 0x3130; c <= 0x318F; c++) list.Add(c); // 한글 자모
        for (uint c = 0xAC00; c <= 0xD7A3; c++) list.Add(c); // 한글 음절 전체 (11,172자)

        return list.ToArray();
    }

    // ── 소스 폰트 파일 검색 ────────────────────────────────────
    static Font FindSourceFont()
    {
        // 우선순위 1: "maplestory" + "bold" 둘 다 포함
        foreach (string guid in AssetDatabase.FindAssets("t:Font"))
        {
            string p     = AssetDatabase.GUIDToAssetPath(guid);
            string lower = p.ToLowerInvariant();
            if (lower.Contains("maplestory") && lower.Contains("bold"))
                return AssetDatabase.LoadAssetAtPath<Font>(p);
        }
        // 우선순위 2: "maplestory" 만 포함
        foreach (string guid in AssetDatabase.FindAssets("t:Font"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.ToLowerInvariant().Contains("maplestory"))
                return AssetDatabase.LoadAssetAtPath<Font>(p);
        }
        return null;
    }

    // ── 씬 + 프리팹 일괄 적용 ─────────────────────────────────
    static int ApplyToScene(TMP_FontAsset font)
    {
        if (font == null) return 0;

        int count = 0;
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(tmp, "Apply Maplestory Bold SDF");
            tmp.font = font;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HistoryItemUI.prefab");
        if (prefab != null)
        {
            foreach (var tmp in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.font = font;
                EditorUtility.SetDirty(tmp);
            }
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[FontFixer] '{font.name}' → 씬 {count}개 + 프리팹 적용 완료");
        return count;
    }
}
#endif
