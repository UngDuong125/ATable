using UnityEngine;

namespace MotMam
{
    public enum SfxKind { Press, Eat, Miss, Foul, Swap, Clear, Fail }

    // Tiếng tạo bằng code, chưa cần file âm thanh. Muốn thay bằng tiếng thật:
    // kéo AudioClip vào ô Override của từng loại.
    public class Sfx : MonoBehaviour
    {
        const int Rate = 44100;

        [SerializeField] AudioClip[] overrides = new AudioClip[7];

        AudioSource source;
        readonly AudioClip[] clips = new AudioClip[7];

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (overrides == null || overrides.Length < clips.Length) overrides = new AudioClip[clips.Length];
        }

        public void Play(SfxKind kind, float volume = 1f)
        {
            if (source == null) return;
            float level = GameSettings.SfxVolume;
            if (level <= 0f) return;
            source.volume = level;
            var clip = Get(kind);
            if (clip != null) source.PlayOneShot(clip, volume);
        }

        AudioClip Get(SfxKind kind)
        {
            int i = (int)kind;
            if (overrides[i] != null) return overrides[i];
            if (clips[i] == null) clips[i] = Build(kind);
            return clips[i];
        }

        static AudioClip Build(SfxKind kind)
        {
            switch (kind)
            {
                case SfxKind.Press: return Make("press", 0.04f, (t, n) => Tone(720f, t) * Env(t, 0.04f, 0.002f) * 0.25f);
                case SfxKind.Eat:
                    return Make("eat", 0.22f, (t, n) =>
                    {
                        float thud = Tone(Mathf.Lerp(230f, 110f, t / 0.12f), t) * Env(t, 0.14f, 0.003f);
                        float crunch = n * Env(t, 0.05f, 0.001f) * 0.5f;
                        float pop = t > 0.06f ? Tone(660f, t) * Env(t - 0.06f, 0.14f, 0.004f) * 0.45f : 0f;
                        return (thud * 0.8f + crunch + pop) * 0.7f;
                    });
                case SfxKind.Miss:
                    return Make("miss", 0.16f, (t, n) => n * Env(t, 0.16f, 0.02f) * 0.22f);
                case SfxKind.Foul:
                    return Make("foul", 0.32f, (t, n) =>
                    {
                        float f = Mathf.Lerp(140f, 85f, t / 0.32f);
                        float saw = 2f * (Mathf.Repeat(t * f, 1f) - 0.5f);
                        return (saw * 0.45f + n * 0.12f) * Env(t, 0.32f, 0.004f) * 0.7f;
                    });
                case SfxKind.Swap:
                    return Make("swap", 0.4f, (t, n) =>
                    {
                        float f = Mathf.Repeat(t, 0.14f) < 0.07f ? 880f : 660f;
                        return Tone(f, t) * Env(Mathf.Repeat(t, 0.14f), 0.07f, 0.004f) * 0.35f;
                    });
                case SfxKind.Clear:
                    return Make("clear", 0.7f, (t, n) =>
                    {
                        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                        float sum = 0f;
                        for (int k = 0; k < notes.Length; k++)
                        {
                            float s = t - k * 0.1f;
                            if (s > 0f) sum += Tone(notes[k], t) * Env(s, 0.45f, 0.005f);
                        }
                        return sum * 0.28f;
                    });
                default:
                    return Make("fail", 0.6f, (t, n) =>
                    {
                        float f = t < 0.28f ? 392f : 261.6f;
                        float s = t < 0.28f ? t : t - 0.28f;
                        return Tone(f, t) * Env(s, 0.3f, 0.005f) * 0.4f;
                    });
            }
        }

        static float Tone(float freq, float t)
        {
            return Mathf.Sin(2f * Mathf.PI * freq * t);
        }

        // Đóng nhanh, tắt dần.
        static float Env(float t, float length, float attack)
        {
            if (t < 0f || t > length) return 0f;
            float a = attack > 0f ? Mathf.Clamp01(t / attack) : 1f;
            float d = 1f - t / length;
            return a * d * d;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float, float> sample)
        {
            int count = Mathf.CeilToInt(seconds * Rate);
            var data = new float[count];
            var rng = new System.Random(name.GetHashCode());
            for (int i = 0; i < count; i++)
            {
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = Mathf.Clamp(sample(i / (float)Rate, noise), -1f, 1f);
            }
            var clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
