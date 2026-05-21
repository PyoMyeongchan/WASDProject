using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundStarEffect : MonoBehaviour
{
    [SerializeField] private int   maxStars      = 18;
    [SerializeField] private float spawnInterval = 0.35f;
    [SerializeField] private float minSize       = 3f;
    [SerializeField] private float maxSize       = 11f;
    [SerializeField] private float minLifetime   = 3.5f;
    [SerializeField] private float maxLifetime   = 6.5f;
    [SerializeField] private float driftSpeed    = 28f;

    private int activeCount;

    private void Start() => StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (activeCount < maxStars)
                StartCoroutine(AnimateStar());
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator AnimateStar()
    {
        activeCount++;

        var go  = new GameObject("Star");
        go.transform.SetParent(transform, false);

        var rt  = go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();

        float size = Random.Range(minSize, maxSize);
        rt.sizeDelta = new Vector2(size, size);

        var parentRT = transform as RectTransform;
        float hw = parentRT != null ? parentRT.rect.width  * 0.5f : 540f;
        float hh = parentRT != null ? parentRT.rect.height * 0.5f : 960f;
        Vector2 startPos = new Vector2(Random.Range(-hw, hw), Random.Range(-hh, hh));
        rt.anchoredPosition = startPos;

        // 흰색 or 골드, 작고 은은하게
        bool gold = Random.value > 0.5f;
        Color base_ = gold ? new Color(1f, 0.87f, 0.35f) : Color.white;
        float maxAlpha = Random.Range(0.15f, 0.5f);

        float lifetime = Random.Range(minLifetime, maxLifetime);
        float elapsed  = 0f;
        // 살짝 비스듬한 방향으로 부유
        Vector2 dir = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            float alpha = t < 0.2f
                ? Mathf.Lerp(0f, maxAlpha, t / 0.2f)
                : t > 0.8f
                    ? Mathf.Lerp(maxAlpha, 0f, (t - 0.8f) / 0.2f)
                    : maxAlpha;

            img.color = new Color(base_.r, base_.g, base_.b, alpha);
            rt.anchoredPosition = startPos + dir * driftSpeed * elapsed;

            yield return null;
        }

        activeCount--;
        Destroy(go);
    }
}
