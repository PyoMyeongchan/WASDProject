using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineController : MonoBehaviour
{
    [Header("슬롯 릴 3개")]
    [SerializeField] private SlotReel itemReel;
    [SerializeField] private SlotReel numberReel;
    [SerializeField] private SlotReel messageReel;

    [Header("UI 요소")]
    [SerializeField] private Button          spinButton;
    [SerializeField] private TextMeshProUGUI spinButtonText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("행운 데이터")]
    [SerializeField] private LuckDataSO luckData;

    [Header("타이밍 설정 (초)")]
    [SerializeField] private float initialSpinDuration = 1.5f;
    [SerializeField] private float reelStopGap         = 0.3f;

    // (물건이름, 숫자, 문구) 전달
    public event Action<string, string, string> OnSpinComplete;

    // 서버에서 받은 씨드 (null 이면 순수 랜덤)
    private int? todaySeed = null;

    private void Awake()
    {
        spinButton.onClick.AddListener(OnSpinButtonClicked);
    }

    public void SetDailySeed(int seed)
    {
        todaySeed = seed;
        Debug.Log($"[SlotMachine] 오늘의 씨드 설정: {seed}");
    }

    public void InitializeReels()
    {
        itemReel.Initialize(luckData.luckyItems);
        numberReel.Initialize(luckData.luckyNumbers);
        messageReel.Initialize(luckData.luckyMessages);
    }

    public void SetButtonState(bool canSpin)
    {
        spinButton.interactable = canSpin;
        spinButtonText.text = canSpin ? "오늘의 행운 뽑기!" : "내일 다시 도전하세요";
        if (statusText != null)
            statusText.text = canSpin ? "" : "오늘의 결과입니다";
    }

    public void ShowSavedResult(string item, string number, string message)
    {
        itemReel.ShowResult(item,   luckData.GetItemIcon(item));
        numberReel.ShowResult(number);
        messageReel.ShowResult(message);
    }

    private void OnSpinButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        StartCoroutine(SpinSequence());
    }

    private IEnumerator SpinSequence()
    {
        SetButtonState(false);
        statusText.text = "행운을 뽑는 중...";

        // 씨드 적용: 서버 씨드가 있으면 결정론적 랜덤, 없으면 순수 랜덤
        if (todaySeed.HasValue)
            UnityEngine.Random.InitState(todaySeed.Value);

        int    itemIdx      = UnityEngine.Random.Range(0, luckData.luckyItems.Length);
        string resultItem   = luckData.luckyItems[itemIdx];
        Sprite resultIcon   = luckData.GetItemIcon(resultItem);
        string resultNumber = luckData.luckyNumbers[UnityEngine.Random.Range(0, luckData.luckyNumbers.Length)];
        string resultMsg    = luckData.luckyMessages[UnityEngine.Random.Range(0, luckData.luckyMessages.Length)];

        AudioManager.Instance?.PlaySpinStart();

        itemReel.StartSpin();
        numberReel.StartSpin();
        messageReel.StartSpin();

        yield return new WaitForSeconds(initialSpinDuration);

        itemReel.StopSpin(resultItem, resultIcon);
        statusText.text = "행운 물건 확정!";
        AudioManager.Instance?.PlayReelStop();
        yield return StartCoroutine(itemReel.WaitForAnimationFinish());

        yield return new WaitForSeconds(reelStopGap);
        numberReel.StopSpin(resultNumber);
        statusText.text = "행운 숫자 확정!";
        AudioManager.Instance?.PlayReelStop();
        yield return StartCoroutine(numberReel.WaitForAnimationFinish());

        yield return new WaitForSeconds(reelStopGap);
        messageReel.StopSpin(resultMsg);
        statusText.text = "행운 문구 확정!";
        AudioManager.Instance?.PlayReelStop();
        yield return StartCoroutine(messageReel.WaitForAnimationFinish());

        yield return new WaitForSeconds(0.3f);
        statusText.text = "";

        OnSpinComplete?.Invoke(resultItem, resultNumber, resultMsg);
    }
}
