using UnityEngine;

namespace MotMam
{
    // Gắn vào AudioSource phát nhạc nền: lặp bài nhạc và theo âm lượng "Nhạc nền" trong Cài đặt.
    // LevelRunner tự gắn lúc chạy cho mọi AudioSource khác trong scene, nên không cần kéo tay.
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusic : MonoBehaviour
    {
        AudioSource source;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.spatialBlend = 0f;
            Apply();
        }

        void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDisable()
        {
            GameSettings.Changed -= Apply;
        }

        void Apply()
        {
            if (source == null) return;
            source.volume = GameSettings.MusicVolume;
            if (!source.isPlaying && source.enabled && gameObject.activeInHierarchy) source.Play();
        }
    }
}
