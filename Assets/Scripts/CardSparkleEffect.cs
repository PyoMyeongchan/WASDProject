using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardSparkleEffect : MonoBehaviour
{
    [SerializeField] private int   sparkleCount   = 18;
    [SerializeField] private float spawnRadius    = 320f;
    [SerializeField] private float minLifetime    = 0.5f;
    [SerializeField] private float maxLifetime    = 1.1f;
    [SerializeField] private float minSize        = 8f;
    [SerializeField] private float maxSize        = 26f;
    [SerializeField] private float moveSpeed      = 110f;

    static readonly Color32[] Colors =
    {
        new Color32(255, 200,  30, 255), // 골드
        new Color32(255, 255, 255, 255), // 흰색
        new Color32(255, 160, 220, 255), // 핑크
        new Color32(160, 220, 255, 255), // 하늘
    };

    public void Play()
    {
        StartCoroutine(SpawnBurst());
    }

    private IEnumerator SpawnBurst()
    {
        for (int i = 0; i < sparkleCount; i++)
        {
            StartCoroutine(AnimateOne());
            yield return new WaitForSeconds(0.04f);
        }
    }

    private IEnumerator AnimateOne()
    {
        var go  = new GameObject("Sparkle");
        go.transform.SetParent(transform, false);

        var rt  = go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();

        float size = Random.Range(minSize, maxSize);
        rt.sizeDelta = new Vector2(size, size);

        // 카드 중앙 부근 랜덤 위치에서 시작
        float angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(0f, spawnRadius);
        rt.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius,
                                          Mathf.Sin(angle) * radius);

        Color baseColor = Colors[Random.Range(0, Colors.Length)];
        img.color = new Color(baseColor.r / 255f, baseColor.g / 255f, baseColor.b / 255f, 0f);

        // 바깥쪽으로 퍼지는 방향
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        float lifetime = Random.Range(minLifetime, maxLifetime);
        float elapsed  = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / lifetime;
            float alpha = Mathf.Sin(t * Mathf.PI);     // 종 모양 — 부드럽게 등장/소멸
            float scale = Mathf.Lerp(0.2f, 1.2f, t);

            img.color       = new Color(baseColor.r / 255f, baseColor.g / 255f, baseColor.b / 255f, alpha);
            rt.localScale   = Vector3.one * scale;
            rt.anchoredPosition += dir * moveSpeed * Time.deltaTime;
            rt.Rotate(0f, 0f, 200f * Time.deltaTime);  // 빙글빙글

            yield return null;
        }

        Destroy(go);
    }
}
