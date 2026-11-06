using UnityEngine;

namespace MotMam
{
    public static class GameSettings
    {
        const string SoundKey = "motmam.sound";
        const string SpeedKey = "motmam.speed";

        public static bool Sound
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
                PlayerPrefs.Save();
                ApplySound();
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

        public static void ApplySound()
        {
            AudioListener.volume = Sound ? 1f : 0f;
        }
    }
}
