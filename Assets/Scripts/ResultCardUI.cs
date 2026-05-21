using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCardUI : MonoBehaviour
{
    [Header("카드 패널 루트")]
    [SerializeField] private GameObject cardPanel;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI itemValueText;
    [SerializeField] private TextMeshProUGUI numberValueText;
    [SerializeField] private TextMeshProUGUI messageValueText;

    [Header("아이콘 (1단계)")]
    [SerializeField] private Image itemIconImage;

    [Header("버튼 (4단계)")]
    [SerializeField] private Button shareButton;
    [SerializeField] private Button historyButton;

    [Header("히스토리 패널 (3단계)")]
    [SerializeField] private HistoryPanel historyPanel;

    [Header("연출 효과")]
    [SerializeField] private CardSparkleEffect sparkleEffect;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private string lastItem;
    private string lastNumber;
    private string lastMessage;

    private void Awake()
    {
        EnsureCanvasGroup();

        if (shareButton != null)
            shareButton.onClick.AddListener(OnShareClicked);

        if (historyButton != null)
            historyButton.onClick.AddListener(OnHistoryClicked);
    }

    // cardPanel 이 비활성 상태일 때도 안전하게 CanvasGroup 을 보장
    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null) return;
        if (cardPanel == null)   return;

        canvasGroup = cardPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = cardPanel.AddComponent<CanvasGroup>();
    }

    public void ShowCard(string item, string number, string message, string date, Sprite itemIcon = null)
    {
        lastItem    = item;
        lastNumber  = number;
        lastMessage = message;

        dateText.text         = date;
        itemValueText.text    = item;
        numberValueText.text  = number;
        messageValueText.text = message;

        if (itemIconImage != null)
        {
            if (itemIcon != null)
            {
                itemIconImage.sprite = itemIcon;
                itemIconImage.gameObject.SetActive(true);
            }
            else
            {
                itemIconImage.gameObject.SetActive(false);
            }
        }

        cardPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeIn());
        sparkleEffect?.Play();
        AudioManager.Instance?.PlayCardReveal();
    }

    public void HideCard()
    {
        EnsureCanvasGroup();
        StopAllCoroutines();
        if (cardPanel    != null) cardPanel.SetActive(false);
        if (canvasGroup  != null) canvasGroup.alpha = 0f;
    }

    private void OnShareClicked()
    {
        ShareManager.Instance?.ShareResult(lastItem, lastNumber, lastMessage);
    }

    private void OnHistoryClicked()
    {
        historyPanel?.Show();
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
