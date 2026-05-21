using System.Collections;
using TMPro;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
    public static ToastUI Instance { get; private set; }

    [SerializeField] private CanvasGroup      canvasGroup;
    [SerializeField] private TextMeshProUGUI  messageText;

    [SerializeField] private float fadeInDuration  = 0.2f;
    [SerializeField] private float holdDuration    = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string message)
    {
        messageText.text = message;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        activeCoroutine = null;
    }
}
