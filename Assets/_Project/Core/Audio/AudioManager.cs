using UnityEngine;

namespace TheLastEmber.Core.Audio
{
    /// <summary>
    /// Hệ thống âm thanh Procedural Sound Synthesis:
    /// Tự động sinh AudioClip trực tiếp trong bộ nhớ bằng thuật toán sóng âm (sine, noise, decay),
    /// không phụ thuộc vào tệp âm thanh bên ngoài, dung lượng 0KB.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private AudioSource sfxSource;

        private AudioClip clipSwing;
        private AudioClip clipHit;
        private AudioClip clipChopWood;
        private AudioClip clipMineStone;
        private AudioClip clipPickup;
        private AudioClip clipFuel;
        private AudioClip clipBuild;
        private AudioClip clipNightBell;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            GenerateAllClips();
        }

        private void GenerateAllClips()
        {
            clipSwing = CreateSwingClip();
            clipHit = CreateHitClip();
            clipChopWood = CreateChopWoodClip();
            clipMineStone = CreateMineStoneClip();
            clipPickup = CreatePickupClip();
            clipFuel = CreateFuelClip();
            clipBuild = CreateBuildClip();
            clipNightBell = CreateNightBellClip();
        }

        public static void PlaySwing() => Instance?.PlayClip(Instance.clipSwing, 0.45f, Random.Range(0.9f, 1.15f));
        public static void PlayHit() => Instance?.PlayClip(Instance.clipHit, 0.7f, Random.Range(0.95f, 1.05f));
        public static void PlayChopWood() => Instance?.PlayClip(Instance.clipChopWood, 0.75f, Random.Range(0.9f, 1.1f));
        public static void PlayMineStone() => Instance?.PlayClip(Instance.clipMineStone, 0.7f, Random.Range(0.95f, 1.15f));
        public static void PlayPickup() => Instance?.PlayClip(Instance.clipPickup, 0.6f, Random.Range(0.98f, 1.05f));
        public static void PlayFuel() => Instance?.PlayClip(Instance.clipFuel, 0.8f, 1.0f);
        public static void PlayBuild() => Instance?.PlayClip(Instance.clipBuild, 0.7f, Random.Range(0.95f, 1.05f));
        public static void PlayNightBell() => Instance?.PlayClip(Instance.clipNightBell, 0.85f, 0.9f);

        private void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume);
        }

        // ================== CÁC THUẬT TOÁN TẠO SÓNG ÂM THANH PROCEDURAL ==================

        private AudioClip CreateSwingClip()
        {
            int sampleRate = 44100;
            float duration = 0.12f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Sin(t * Mathf.PI);
                float freq = Mathf.Lerp(380f, 140f, t);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                float noise = (Random.value * 2f - 1f) * 0.35f;
                data[i] = (wave + noise) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("SFX_Swing", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateHitClip()
        {
            int sampleRate = 44100;
            float duration = 0.14f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Exp(-t * 14f);
                float freq = Mathf.Lerp(160f, 60f, t);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 22f);
                data[i] = (wave * 0.7f + noise * 0.6f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_Hit", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChopWoodClip()
        {
            int sampleRate = 44100;
            float duration = 0.13f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Exp(-t * 18f);
                float wave1 = Mathf.Sin(2f * Mathf.PI * 260f * (i / (float)sampleRate));
                float wave2 = Mathf.Sin(2f * Mathf.PI * 480f * (i / (float)sampleRate));
                float click = (Random.value * 2f - 1f) * Mathf.Exp(-t * 35f) * 0.7f;
                data[i] = ((wave1 * 0.5f + wave2 * 0.3f) * env + click) * 0.7f;
            }

            AudioClip clip = AudioClip.Create("SFX_ChopWood", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateMineStoneClip()
        {
            int sampleRate = 44100;
            float duration = 0.15f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Exp(-t * 16f);
                float clink = Mathf.Sin(2f * Mathf.PI * 1250f * (i / (float)sampleRate)) * 0.45f;
                float body = Mathf.Sin(2f * Mathf.PI * 340f * (i / (float)sampleRate)) * 0.4f;
                float snap = (Random.value * 2f - 1f) * Mathf.Exp(-t * 40f) * 0.45f;
                data[i] = (clink + body + snap) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("SFX_MineStone", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreatePickupClip()
        {
            int sampleRate = 44100;
            float duration = 0.16f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Sin(t * Mathf.PI);
                float freq = t < 0.45f ? 523.25f : 659.25f; // Nốt C5 -> E5
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                data[i] = wave * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("SFX_Pickup", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateFuelClip()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Sin(t * Mathf.PI);
                float rumble = Mathf.Sin(2f * Mathf.PI * 75f * (i / (float)sampleRate)) * 0.5f;
                float hiss = (Random.value * 2f - 1f) * 0.4f;
                data[i] = (rumble + hiss) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create("SFX_Fuel", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBuildClip()
        {
            int sampleRate = 44100;
            float duration = 0.18f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Exp(-t * 12f);
                float thud = Mathf.Sin(2f * Mathf.PI * 180f * (i / (float)sampleRate)) * 0.6f;
                float snap = (Random.value * 2f - 1f) * Mathf.Exp(-t * 25f) * 0.4f;
                data[i] = (thud + snap) * env * 0.65f;
            }

            AudioClip clip = AudioClip.Create("SFX_Build", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNightBellClip()
        {
            int sampleRate = 44100;
            float duration = 1.6f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float env = Mathf.Exp(-t * 3.5f);
                float bellFundamental = Mathf.Sin(2f * Mathf.PI * 220f * (i / (float)sampleRate)); // A3
                float bellHarmonic = Mathf.Sin(2f * Mathf.PI * 440f * (i / (float)sampleRate)) * 0.45f;
                float bellOvertone = Mathf.Sin(2f * Mathf.PI * 880f * (i / (float)sampleRate)) * 0.2f;
                data[i] = (bellFundamental + bellHarmonic + bellOvertone) * env * 0.55f;
            }

            AudioClip clip = AudioClip.Create("SFX_NightBell", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
