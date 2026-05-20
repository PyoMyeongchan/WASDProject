using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 소스")]
    [SerializeField] private AudioSource sfxSource;

    [Header("효과음 클립")]
    [SerializeField] private AudioClip spinStartClip;   // 스핀 시작
    [SerializeField] private AudioClip reelStopClip;    // 릴 하나 멈춤
    [SerializeField] private AudioClip cardRevealClip;  // 결과 카드 등장
    [SerializeField] private AudioClip buttonClickClip; // 버튼 클릭

    [Header("볼륨 (0~1)")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySpinStart()   => Play(spinStartClip);
    public void PlayReelStop()    => Play(reelStopClip);
    public void PlayCardReveal()  => Play(cardRevealClip);
    public void PlayButtonClick() => Play(buttonClickClip);

    public void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
