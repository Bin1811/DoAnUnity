using System.Collections.Generic;
using UnityEngine;

// Am thanh sinh bang code: tieng ban, tieng chuot, nhac nen nhe.
public class AudioKit : MonoBehaviour
{
    public static AudioKit I;

    AudioSource[] pool;
    AudioSource music;
    int next;
    public bool Muted { get; private set; }
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        I = this;
        pool = new AudioSource[5];
        for (int i = 0; i < pool.Length; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0.7f;
            pool[i] = src;
        }
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = 0f;
        music.volume = 0.22f;
        music.clip = BuildMusic();
        music.Play();
    }

    public void SetMuted(bool muted)
    {
        Muted = muted;
        AudioListener.volume = muted ? 0f : 1f;
    }

    public void ToggleMute() => SetMuted(!Muted);

    public void Play(string id, float volume = 1f, float pitch = 1f)
    {
        if (Muted) return;
        if (!clips.TryGetValue(id, out var clip))
        {
            clip = BuildSfx(id);
            clips[id] = clip;
        }
        if (clip == null) return;
        var src = pool[next];
        next = (next + 1) % pool.Length;
        src.pitch = pitch;
        src.PlayOneShot(clip, volume * 0.55f);
    }

    // ---------- Tao am thanh ----------

    const int Rate = 44100;

    static AudioClip Make(string name, float seconds, System.Func<float, float> gen)
    {
        int n = Mathf.Max(1, (int)(Rate * seconds));
        var data = new float[n];
        for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(gen(i / (float)Rate), -1f, 1f);
        var clip = AudioClip.Create(name, n, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Env(float t, float dur, float attack = 0.005f, float curve = 4f)
    {
        if (t < 0f || t > dur) return 0f;
        float a = Mathf.Clamp01(t / Mathf.Max(0.001f, attack));
        float d = 1f - t / dur;
        return a * Mathf.Pow(d, curve);
    }

    static float Sine(float hz, float t) => Mathf.Sin(t * hz * Mathf.PI * 2f);

    static AudioClip BuildSfx(string id)
    {
        switch (id)
        {
            case "shoot":
                return Make("shoot", 0.09f, t =>
                {
                    float f = Mathf.Lerp(880f, 480f, t / 0.09f);
                    return (Sine(f, t) * 0.6f + Mathf.Sin(t * 900f * Mathf.PI * 2f) * 0.2f) * Env(t, 0.09f);
                });
            case "ice":
                return Make("ice", 0.16f, t =>
                {
                    float env = Env(t, 0.16f, 0.003f, 3f);
                    return (Sine(1560f, t) * 0.5f + Sine(2340f, t) * 0.25f + Sine(780f, t) * 0.2f) * env;
                });
            case "bubble":
                return Make("bubble", 0.18f, t =>
                {
                    float f = Mathf.Lerp(300f, 720f, Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI));
                    float bubble = Sine(f, t) * 0.55f;
                    float noise = (Random.value * 2f - 1f) * 0.12f * Mathf.Exp(-t * 45f);
                    return (bubble + noise) * Env(t, 0.18f, 0.004f, 2f);
                });
            case "sniper":
                return Make("sniper", 0.22f, t =>
                {
                    float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 55f);
                    float thump = Sine(Mathf.Lerp(260f, 80f, t / 0.22f), t) * Mathf.Exp(-t * 20f);
                    return noise * 0.5f + thump * 0.75f;
                });
            case "hit":
                return Make("hit", 0.07f, t =>
                    ((Random.value * 2f - 1f) * 0.5f + Sine(220f, t) * 0.3f) * Mathf.Exp(-t * 60f));
            case "kill":
                return Make("kill", 0.17f, t =>
                {
                    float f = Mathf.Lerp(1400f, 620f, t / 0.17f);
                    float vib = 1f + 0.06f * Mathf.Sin(t * 90f);
                    return Sine(f * vib, t) * 0.5f * Env(t, 0.17f, 0.004f, 2.5f);
                });
            case "bosskill":
                return Make("bosskill", 0.6f, t =>
                {
                    float squeak = Sine(Mathf.Lerp(900f, 260f, t / 0.6f), t) * 0.35f;
                    float boom = Sine(Mathf.Lerp(120f, 45f, t / 0.6f), t) * 0.6f;
                    return (squeak + boom) * Mathf.Exp(-t * 5f);
                });
            case "place":
                return Make("place", 0.22f, t =>
                {
                    float v = 0f;
                    v += Sine(660f, t) * Env(t, 0.22f, 0.004f, 3f) * 0.35f;
                    v += Sine(990f, t) * Env(Mathf.Max(0f, t - 0.07f), 0.15f, 0.004f, 3f) * 0.3f;
                    return v;
                });
            case "upgrade":
                return Make("upgrade", 0.42f, t =>
                {
                    float v = 0f;
                    for (int i = 0; i < 3; i++)
                    {
                        float start = i * 0.09f;
                        float hz = 660f * Mathf.Pow(1.25f, i);
                        v += Sine(hz, t - start) * Env(t - start, 0.2f, 0.004f, 2.5f) * 0.3f;
                    }
                    return v;
                });
            case "sell":
                return Make("sell", 0.28f, t =>
                {
                    float v = 0f;
                    v += Sine(880f, t) * Env(t, 0.28f, 0.004f, 3f) * 0.28f;
                    v += Sine(620f, t - 0.09f) * Env(t - 0.09f, 0.2f, 0.004f, 3f) * 0.25f;
                    return v;
                });
            case "error":
                return Make("error", 0.22f, t =>
                    (Sine(150f, t) * 0.5f + Sine(105f, t) * 0.35f) * Env(t, 0.22f, 0.005f, 1.5f));
            case "basehit":
                return Make("basehit", 0.55f, t =>
                {
                    float boom = Sine(Mathf.Lerp(110f, 42f, t / 0.55f), t) * 0.8f;
                    float noise = (Random.value * 2f - 1f) * 0.25f * Mathf.Exp(-t * 14f);
                    return (boom + noise) * Mathf.Exp(-t * 4f);
                });
            case "wave":
                return Make("wave", 0.75f, t =>
                {
                    float f = t < 0.3f ? 330f : 440f;
                    float vib = 1f + 0.01f * Mathf.Sin(t * 40f);
                    return (Sine(f * vib, t) * 0.4f + Sine(f * 2f * vib, t) * 0.12f) * Env(t, 0.75f, 0.04f, 1.6f);
                });
            case "win":
                return Make("win", 1.8f, t =>
                {
                    float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                    float v = 0f;
                    for (int i = 0; i < notes.Length; i++)
                    {
                        float st = i * 0.18f;
                        float local = t - st;
                        v += (Sine(notes[i], local) * 0.24f + Sine(notes[i] * 2f, local) * 0.08f)
                             * Env(local, 0.7f, 0.01f, 2f);
                    }
                    return v;
                });
            case "lose":
                return Make("lose", 1.4f, t =>
                {
                    float[] notes = { 392f, 329.63f, 261.63f, 196f };
                    float v = 0f;
                    for (int i = 0; i < notes.Length; i++)
                    {
                        float st = i * 0.22f;
                        float local = t - st;
                        v += Sine(notes[i], local) * Env(local, 0.65f, 0.02f, 2f) * 0.26f;
                    }
                    return v;
                });
            case "click":
                return Make("click", 0.06f, t => Sine(880f, t) * Mathf.Exp(-t * 70f) * 0.4f);
            case "select":
                return Make("select", 0.09f, t => Sine(720f, t) * Mathf.Exp(-t * 45f) * 0.3f);
            default:
                return Make(id, 0.1f, t => Sine(440f, t) * Mathf.Exp(-t * 30f) * 0.3f);
        }
    }

    static AudioClip BuildMusic()
    {
        const float bpm = 76f;
        float beat = 60f / bpm;
        const int bars = 8;
        float dur = beat * 4f * bars;
        int n = Mathf.RoundToInt(Rate * dur);
        var data = new float[n];
        var rnd = new System.Random(42);

        // Hop am 4 hop, moi hop 4 nhip (2 o nhip).
        float[] roots = { 261.63f, 220f, 174.61f, 196f };
        float[][] chords =
        {
            new[] { 261.63f, 329.63f, 392f, 493.88f },
            new[] { 220f, 261.63f, 329.63f, 392f },
            new[] { 174.61f, 220f, 261.63f, 349.23f },
            new[] { 196f, 246.94f, 293.66f, 349.23f },
        };
        float[] melody = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 783.99f, 659.25f, 587.33f };

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Rate;
            int bar = Mathf.FloorToInt(t / (beat * 4f)) % bars;
            int chord = (bar / 2) % 4;
            float local = t % (beat * 4f);

            float v = 0f;
            // Pad mem
            for (int k = 0; k < 4; k++)
            {
                float hz = chords[chord][k];
                float swell = Mathf.Sin(Mathf.Clamp01(local / (beat * 4f)) * Mathf.PI);
                v += Mathf.Sin(t * hz * Mathf.PI * 2f) * 0.012f * swell;
                v += Mathf.Sin(t * hz * 2.003f * Mathf.PI * 2f) * 0.004f * swell;
            }
            // Bass
            float bassHz = roots[chord] * 0.5f;
            float bassEnv = Mathf.Exp(-(local % (beat * 2f)) * 2.4f);
            v += Mathf.Sin(t * bassHz * Mathf.PI * 2f) * 0.05f * bassEnv;

            // Giai dieu thoang
            float step = beat * 0.5f;
            int idx = Mathf.FloorToInt(t / step);
            float mt = t - idx * step;
            float mhz = melody[(idx + chord * 2) % melody.Length];
            if (idx % 3 != 2)
            {
                v += Mathf.Sin(mt * mhz * Mathf.PI * 2f) * Mathf.Exp(-mt * 7f) * 0.035f;
                v += Mathf.Sin(mt * mhz * 2f * Mathf.PI * 2f) * Mathf.Exp(-mt * 10f) * 0.012f;
            }
            // Tieng gio rat nhe
            v += (Random.value * 2f - 1f) * 0.002f;

            data[i] = Mathf.Clamp(v, -1f, 1f);
        }

        // Lam muot 2 dau de loop khong bi "cach".
        int fade = Rate / 2;
        for (int i = 0; i < fade; i++)
        {
            float k = i / (float)fade;
            data[i] *= k;
            data[n - 1 - i] *= k;
        }

        var clip = AudioClip.Create("music", n, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
