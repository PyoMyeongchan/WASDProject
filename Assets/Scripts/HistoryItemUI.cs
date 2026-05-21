using TMPro;
using UnityEngine;

// HistoryPanel 의 ScrollView Content 안에 Instantiate 되는 단일 기록 항목 UI
public class HistoryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetData(LuckRecord record)
    {
        if (dateText == null || itemText == null || numberText == null || messageText == null)
        {
            Debug.LogError("[HistoryItemUI] 텍스트 필드가 연결되지 않았습니다. 씬을 재생성해 주세요.");
            return;
        }
        dateText.text    = record.DisplayDate;
        itemText.text    = $"물건: {record.item}";
        numberText.text  = $"숫자: {record.number}";
        messageText.text = record.message;
    }
}
