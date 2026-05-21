using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 버튼에 부착하면 누를 때 살짝 줄어들고 뗄 때 통통 튀는 스케일 애니메이션
public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressedScale  = 0.91f;
    [SerializeField] private float bounceScale   = 1.06f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float bounceDuration= 0.08f;
    [SerializeField] private float settleDuration= 0.07f;

    private Coroutine active;

    public void OnPointerDown(PointerEventData _)
    {
        Run(ScaleDown());
    }

    public void OnPointerUp(PointerEventData _)
    {
        Run(ScaleBounce());
    }

    private void Run(IEnumerator routine)
    {
        if (active != null) StopCoroutine(active);
        active = StartCoroutine(routine);
    }

    private IEnumerator ScaleDown()
    {
        yield return Lerp(transform.localScale, Vector3.one * pressedScale, pressDuration);
    }

    private IEnumerator ScaleBounce()
    {
        // 누른 상태 → 살짝 오버슈팅 → 원래 크기
        yield return Lerp(transform.localScale, Vector3.one * bounceScale, bounceDuration);
        yield return Lerp(transform.localScale, Vector3.one,               settleDuration);
    }

    private IEnumerator Lerp(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        transform.localScale = to;
    }
}
