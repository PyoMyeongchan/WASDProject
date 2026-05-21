using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SlotMachineController slotMachine;
    [SerializeField] private ResultCardUI          resultCard;
    [SerializeField] private LuckDataSO            luckData;   // 아이콘 조회용

    private void OnEnable()
    {
        slotMachine.OnSpinComplete += HandleSpinComplete;
    }

    private void OnDisable()
    {
        slotMachine.OnSpinComplete -= HandleSpinComplete;
    }

    private void Start()
    {
        slotMachine.InitializeReels();

        // 5단계: 서버에서 씨드를 먼저 요청, 실패 시 로컬 랜덤
        if (ServerLuckManager.Instance != null && ServerLuckManager.Instance.IsConfigured)
        {
            StartCoroutine(ServerLuckManager.Instance.FetchTodaySeed(
                seed  => slotMachine.SetDailySeed(seed),
                ()    => { /* 서버 실패 — 로컬 랜덤 사용 (기본 동작) */ }
            ));
        }

        CheckDailyStatus();
    }

    private void CheckDailyStatus()
    {
        if (SaveManager.HasPlayedToday())
        {
            var (item, number, message) = SaveManager.LoadResult();
            // PlayerPrefs와 히스토리 JSON이 어긋날 경우를 대비해 항상 동기화
            // AddRecord 내부에서 오늘 날짜 중복은 자동으로 덮어쓰기됨
            LuckHistoryManager.AddRecord(item, number, message);
            Sprite icon = luckData != null ? luckData.GetItemIcon(item) : null;
            slotMachine.SetButtonState(false);
            slotMachine.ShowSavedResult(item, number, message);
            resultCard.ShowCard(item, number, message, GetFormattedDate(), icon);
        }
        else
        {
            slotMachine.SetButtonState(true);
            resultCard.HideCard();
        }
    }

    private void HandleSpinComplete(string item, string number, string message)
    {
        SaveManager.SaveResult(item, number, message);

        // 3단계: 히스토리 기록
        LuckHistoryManager.AddRecord(item, number, message);

        Sprite icon = luckData != null ? luckData.GetItemIcon(item) : null;
        resultCard.ShowCard(item, number, message, GetFormattedDate(), icon);
    }

    private string GetFormattedDate() => DateTime.Now.ToString("yyyy년 MM월 dd일");

#if UNITY_EDITOR
    [ContextMenu("테스트: 저장 데이터 초기화")]
    private void ClearSaveForTest()
    {
        SaveManager.ClearSaveData();
        slotMachine.InitializeReels();
        CheckDailyStatus();
    }

    [ContextMenu("테스트: 히스토리 전체 삭제")]
    private void ClearHistory() => LuckHistoryManager.ClearHistory();
#endif
}
