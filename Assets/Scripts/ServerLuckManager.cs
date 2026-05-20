using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// 5단계: 서버 기반 오늘의 씨드 수신
// 서버 응답 예시: {"seed": 12345, "date": "2026-05-20"}
// 같은 씨드를 받은 모든 유저는 같은 결과를 뽑게 됨
public class ServerLuckManager : MonoBehaviour
{
    public static ServerLuckManager Instance { get; private set; }

    [Header("서버 URL (빈 칸이면 비활성화)")]
    [SerializeField] private string dailySeedUrl = "";

    [Header("타임아웃 (초)")]
    [SerializeField] private int timeoutSeconds = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsConfigured => !string.IsNullOrEmpty(dailySeedUrl);

    // 서버에서 오늘의 씨드를 받아온 뒤 onSuccess(seed) 또는 onFail() 호출
    public IEnumerator FetchTodaySeed(Action<int> onSuccess, Action onFail)
    {
        if (!IsConfigured)
        {
            onFail?.Invoke();
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(dailySeedUrl);
        request.timeout = timeoutSeconds;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SeedResponse response = JsonUtility.FromJson<SeedResponse>(request.downloadHandler.text);
            Debug.Log($"[Server] 씨드 수신 성공: {response.seed} ({response.date})");
            onSuccess?.Invoke(response.seed);
        }
        else
        {
            Debug.LogWarning($"[Server] 연결 실패 ({request.error}), 로컬 랜덤 사용");
            onFail?.Invoke();
        }
    }

    [Serializable]
    private class SeedResponse
    {
        public int    seed;
        public string date;
    }
}
