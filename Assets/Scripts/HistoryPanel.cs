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
        panelRoot.SetActive(true);
        RefreshList();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void RefreshList()
    {
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
    }
}
