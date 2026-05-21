using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotReel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Image           iconDisplay;  // 선택 사항 — 결과 아이콘 표시
    [SerializeField] private Image           glowImage;    // 릴 멈춤 강조 오버레이

    [Header("스핀 속도 설정")]
    [SerializeField] private float fastSpinInterval  = 0.05f;
    [SerializeField] private int   slowdownSteps     = 6;
    [SerializeField] private float slowdownIncrement = 0.04f;

    [Header("아이콘 펀치 애니메이션")]
    [SerializeField] private float punchScale    = 1.3f;
    [SerializeField] private float punchDuration = 0.25f;

    private string[] items;
    private string   pendingResult;
    private Sprite   pendingIcon;
    private bool     isSpinning;
    private bool     isAnimationFinished;

    public bool IsAnimationFinished => isAnimationFinished;

    public void Initialize(string[] dataItems)
    {
        items = dataItems;
        displayText.text = "?";
        isSpinning = false;
        isAnimationFinished = true;
        SetIconVisible(false);
    }

    // 저장된 결과를 애니메이션 없이 즉시 표시
    public void ShowResult(string result, Sprite icon = null)
    {
        displayText.text = result;
        if (iconDisplay != null && icon != null)
        {
            iconDisplay.sprite = icon;
            SetIconVisible(true);
        }
    }

    public void StartSpin()
    {
        if (isSpinning) return;
        isSpinning = true;
        isAnimationFinished = false;
        pendingResult = string.Empty;
        pendingIcon = null;
        SetIconVisible(false);
        StartCoroutine(SpinCoroutine());
    }

    public void StopSpin(string result, Sprite icon = null)
    {
        pendingResult = result;
        pendingIcon = icon;
        isSpinning = false;
    }

    public IEnumerator WaitForAnimationFinish()
    {
        while (!isAnimationFinished)
            yield return null;
    }

    private IEnumerator SpinCoroutine()
    {
        int index = 0;
        int minIterations = 5;

        while (isSpinning || index < minIterations)
        {
            displayText.text = items[index % items.Length];
            index++;
            yield return new WaitForSeconds(fastSpinInterval);
        }

        float delay = fastSpinInterval;
        for (int i = 0; i < slowdownSteps; i++)
        {
            delay += slowdownIncrement;
            displayText.text = items[Random.Range(0, items.Length)];
            yield return new WaitForSeconds(delay);
        }

        displayText.text = pendingResult;

        if (iconDisplay != null && pendingIcon != null)
        {
            iconDisplay.sprite = pendingIcon;
            SetIconVisible(true);
            StartCoroutine(PunchScaleAnimation(iconDisplay.transform));
        }

        if (glowImage != null)
            StartCoroutine(GlowPulse());

        isAnimationFinished = true;
    }

    private IEnumerator GlowPulse()
    {
        float fadeIn  = 0.12f;
        float hold    = 0.18f;
        float fadeOut = 0.35f;

        // Fade in
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            SetGlowAlpha(Mathf.Clamp01(t / fadeIn));
            yield return null;
        }

        yield return new WaitForSeconds(hold);

        // Fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            SetGlowAlpha(Mathf.Clamp01(1f - t / fadeOut));
            yield return null;
        }
        SetGlowAlpha(0f);
    }

    private void SetGlowAlpha(float a)
    {
        if (glowImage == null) return;
        var c = glowImage.color;
        glowImage.color = new Color(c.r, c.g, c.b, a);
    }

    private IEnumerator PunchScaleAnimation(Transform target)
    {
        Vector3 original = Vector3.one;
        Vector3 big      = Vector3.one * punchScale;
        float   half     = punchDuration * 0.5f;
        float   elapsed  = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(original, big, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(big, original, elapsed / half);
            yield return null;
        }

        target.localScale = original;
    }

    private void SetIconVisible(bool visible)
    {
        if (iconDisplay != null)
            iconDisplay.gameObject.SetActive(visible);
    }
}
