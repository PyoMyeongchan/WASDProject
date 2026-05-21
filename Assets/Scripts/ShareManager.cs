using System.Collections;
using System.IO;
using UnityEngine;

// 4단계: 결과 공유
// NativeShare 플러그인(무료, Asset Store) 을 설치하면 실제 모바일 공유 가능
// 미설치 시 텍스트를 클립보드에 복사
public class ShareManager : MonoBehaviour
{
    public static ShareManager Instance { get; private set; }

    [Header("공유할 패널 (스크린샷 대상)")]
    [SerializeField] private RectTransform shareTargetPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShareResult(string item, string number, string message)
    {
        StartCoroutine(CaptureAndShare(item, number, message));
    }

    private IEnumerator CaptureAndShare(string item, string number, string message)
    {
        yield return new WaitForEndOfFrame();

        string shareText = $"✨ 오늘의 행운 카드 ✨\n" +
                           $"행운 물건: {item}\n" +
                           $"행운 숫자: {number}\n" +
                           $"메시지: {message}";

        string screenshotPath = Path.Combine(Application.persistentDataPath, "lucky_card_share.png");
        ScreenCapture.CaptureScreenshot(screenshotPath);

        // NativeShare 플러그인 설치 후 아래 주석 해제:
        // yield return new WaitForSeconds(0.1f); // 스크린샷 저장 대기
        // new NativeShare()
        //     .SetSubject("오늘의 행운!")
        //     .SetText(shareText)
        //     .AddFile(screenshotPath)
        //     .Share();

        // 플러그인 없을 때 fallback
        GUIUtility.systemCopyBuffer = shareText;
        Debug.Log($"[ShareManager] 클립보드에 복사:\n{shareText}");
        ToastUI.Instance?.Show("클립보드에 복사됐어요!");

#if UNITY_ANDROID || UNITY_IOS
        // 모바일에서는 Android Toast 또는 iOS 알림으로 "복사됨" 안내
        // 별도 플러그인 없이는 Debug.Log 만 출력됨
#endif
    }
}
