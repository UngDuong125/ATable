using UnityEngine;

namespace MotMam
{
    public static class GameSettings
    {
        const string LegacySoundKey = "motmam.sound";
        const string MusicKey = "motmam.music";
        const string SfxKey = "motmam.sfx";
        const string SpeedKey = "motmam.speed";
        const float DefaultMusic = 0.6f;
        const float DefaultSfx = 1f;

        // Gọi mỗi khi đổi âm lượng, để nhạc nền cập nhật ngay.
        public static event System.Action Changed;

        // Âm lượng nhạc nền, 0 = tắt, 1 = to nhất.
        public static float MusicVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, LegacyOff() ? 0f : DefaultMusic));
            set
            {
                PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
                Changed?.Invoke();
            }
        }

        // Âm lượng tiếng hiệu ứng (cắn, trượt, qua màn...), 0 = tắt.
        public static float SfxVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, LegacyOff() ? 0f : DefaultSfx));
            set
            {
                PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
                Changed?.Invoke();
            }
        }

        public static int SpeedStep
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(SpeedKey, 1), 0, 2);
            set
            {
                PlayerPrefs.SetInt(SpeedKey, Mathf.Clamp(value, 0, 2));
                PlayerPrefs.Save();
            }
        }

        public static float SpinScale
        {
            get
            {
                if (SpeedStep == 0) return 0.8f;
                if (SpeedStep == 2) return 1.2f;
                return 1f;
            }
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void ApplySound()
        {
            // Bản cũ tắt tiếng bằng AudioListener; giờ mỗi loại tự chỉnh âm lượng riêng.
            AudioListener.volume = 1f;
            Changed?.Invoke();
        }

        // Người chơi từng bấm "Tắt" ở bản cũ thì giữ nguyên là tắt.
        static bool LegacyOff()
        {
            return PlayerPrefs.GetInt(LegacySoundKey, 1) == 0;
        }
    }
}
