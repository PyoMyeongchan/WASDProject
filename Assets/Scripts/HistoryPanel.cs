using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanel : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject   panelRoot;       // 패널 루트 (Show/Hide)
    [SerializeField] private Transform    contentParent;   // ScrollView > Viewport > Content
    [SerializeField] private HistoryItemUI historyItemPrefab; // 행 하나 프리팹
    [SerializeField] private Button       closeButton;

    private readonly List<HistoryItemUI> spawnedItems = new List<HistoryItemUI>();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        // Canvas 내 최상위로 이동해 다른 UI에 가려지지 않도록 보장
        transform.SetAsLastSibling();
        panelRoot.SetActive(true);
        RefreshList();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void RefreshList()
    {
        if (historyItemPrefab == null)
        {
            Debug.LogError("[HistoryPanel] historyItemPrefab이 연결되지 않았습니다. 씬을 재생성해 주세요.");
            return;
        }

        // 기존 항목 제거
        foreach (var item in spawnedItems)
            Destroy(item.gameObject);
        spawnedItems.Clear();

        List<LuckRecord> records = LuckHistoryManager.GetAllRecords();

        if (records.Count == 0)
        {
            Debug.Log("[HistoryPanel] 저장된 기록이 없습니다.");
            return;
        }

        foreach (LuckRecord record in records)
        {
            HistoryItemUI item = Instantiate(historyItemPrefab, contentParent);
            item.SetData(record);
            spawnedItems.Add(item);
        }

        // ContentSizeFitter가 즉시 반영되도록 강제 갱신
        Canvas.ForceUpdateCanvases();
        var contentRT = contentParent.GetComponent<RectTransform>();
        if (contentRT != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
    }
}
