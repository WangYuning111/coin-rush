using UnityEngine;

public static class SimpleSFXGenerator
{
    private const int SampleRate = 44100;

    public static AudioClip GenerateCoinCollect()
    {
        // 经典 8-bit 双音调上升 coin 声（类似 Mario）
        float duration = 0.22f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        float[] freqs = new float[] { 987.77f, 1318.51f }; // B5 -> E6
        float noteDur = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            int noteIdx = Mathf.Min((int)(t / noteDur), freqs.Length - 1);
            float noteTime = t - noteIdx * noteDur;
            float freq = freqs[noteIdx];

            // 三角波为主体，叠加少量正弦波，比纯正弦波更柔和且像游戏音效
            float tri = Mathf.Abs((t * freq * 4f) % 4f - 2f) - 1f;
            float sin = Mathf.Sin(2f * Mathf.PI * freq * t);
            float wave = tri * 0.6f + sin * 0.4f;

            // 短促起音 + 自然指数衰减，避免突兀
            float envelope;
            if (noteTime < 0.01f)
                envelope = noteTime / 0.01f;
            else
                envelope = Mathf.Exp(-(noteTime - 0.01f) * 18f);

            data[i] = wave * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("CoinCollect", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateButtonClick()
    {
        float duration = 0.08f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            float envelope = Mathf.Exp(-t * 60f);
            // 短促方波脉冲 + 高频噪声
            float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 800f * t));
            float noise = (Random.value * 2f - 1f);
            data[i] = (square * 0.6f + noise * 0.4f) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("ButtonClick", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateCountdownWarning()
    {
        float duration = 0.25f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            float progress = t / duration;
            float envelope = progress < 0.1f ? progress / 0.1f : Mathf.Pow(1f - progress, 2f);
            data[i] = Mathf.Sin(2f * Mathf.PI * 440f * t) * envelope * 0.7f;
        }

        AudioClip clip = AudioClip.Create("CountdownWarning", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateLevelSuccess()
    {
        float noteDuration = 0.18f;
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5 E5 G5 C6
        int noteSamples = (int)(SampleRate * noteDuration);
        int totalSamples = noteSamples * freqs.Length;
        float[] data = new float[totalSamples];

        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < noteSamples; i++)
            {
                float t = i / (float)SampleRate;
                float progress = t / noteDuration;
                float envelope = Mathf.Pow(1f - progress, 1.5f);
                float sample = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * envelope * 0.6f;
                data[n * noteSamples + i] = sample;
            }
        }

        AudioClip clip = AudioClip.Create("LevelSuccess", totalSamples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateLevelFail()
    {
        float noteDuration = 0.3f;
        float[] freqs = new float[] { 392f, 329.63f, 261.63f }; // G4 E4 C4
        int noteSamples = (int)(SampleRate * noteDuration);
        int totalSamples = noteSamples * freqs.Length;
        float[] data = new float[totalSamples];

        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < noteSamples; i++)
            {
                float t = i / (float)SampleRate;
                float progress = t / noteDuration;
                float envelope = Mathf.Pow(1f - progress, 1.2f);
                float sample = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * envelope * 0.6f;
                data[n * noteSamples + i] = sample;
            }
        }

        AudioClip clip = AudioClip.Create("LevelFail", totalSamples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateMenuMusic()
    {
        // 8秒循环：轻柔的C大调和弦琶音 + 简单旋律
        float duration = 8.0f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        // 琶音和弦 C - E - G - C6
        float[][] arpeggio = new float[][]
        {
            new float[] { 261.63f, 329.63f, 392.00f, 523.25f },
            new float[] { 261.63f, 329.63f, 392.00f, 523.25f },
            new float[] { 246.94f, 293.66f, 392.00f, 493.88f }, // Bm7b5-ish
            new float[] { 261.63f, 329.63f, 392.00f, 523.25f }
        };
        float beatLen = duration / (arpeggio.Length * 4f); // 16分音符长度

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            float beatPos = t / beatLen;
            int chordIdx = Mathf.Min((int)(beatPos / 4f), arpeggio.Length - 1);
            int noteInChord = (int)beatPos % 4;
            float freq = arpeggio[chordIdx][noteInChord];

            // 音符包络：快速起音，适中衰减
            float notePhase = (beatPos % 1f);
            float envelope = Mathf.Exp(-notePhase * 6f);
            if (notePhase < 0.05f) envelope *= notePhase / 0.05f;

            // 三角波 + 轻微正弦波混音，模拟8-bit柔和音色
            float tri = Mathf.Abs((t * freq * 4f) % 4f - 2f) - 1f;
            float sin = Mathf.Sin(2f * Mathf.PI * freq * t);
            float sampleVal = (tri * 0.5f + sin * 0.5f) * envelope * 0.12f;

            // 叠加低音根音，每拍变化
            float rootFreq = arpeggio[chordIdx][0] * 0.5f;
            float root = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * rootFreq * t)) * 0.08f * envelope;

            data[i] = Mathf.Clamp(sampleVal + root, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("MenuMusic", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateGameplayMusic()
    {
        // 4秒快节奏循环：F小调，更有紧迫感
        float duration = 4.0f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        float[] bassLine = new float[] { 174.61f, 174.61f, 155.56f, 164.81f }; // F3 F3 Eb3 E3
        float[] leadLine = new float[] { 349.23f, 392.00f, 349.23f, 311.13f, 349.23f, 392.00f, 440.00f, 349.23f };
        float beatLen = duration / bassLine.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            int beatIdx = Mathf.Min((int)(t / beatLen), bassLine.Length - 1);
            float bassFreq = bassLine[beatIdx];

            float notePhase = (t % beatLen) / beatLen;
            float envelope = Mathf.Exp(-notePhase * 3f);

            // 低音：方波，有节奏感
            float bass = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * bassFreq * t)) * 0.12f * envelope;

            // 主音旋律：每拍两个音符
            int leadNoteIdx = (int)((t / (beatLen * 0.5f)) % leadLine.Length);
            float leadFreq = leadLine[leadNoteIdx];
            float leadPhase = (t % (beatLen * 0.5f)) / (beatLen * 0.5f);
            float leadEnv = Mathf.Exp(-leadPhase * 8f);
            float lead = (Mathf.Abs((t * leadFreq * 4f) % 4f - 2f) - 1f) * 0.1f * leadEnv;

            // 高频琶音点缀
            float arpFreq = 523.25f + ((int)(t * 8f) % 3) * 130.81f;
            float arp = Mathf.Sin(2f * Mathf.PI * arpFreq * t) * 0.04f * Mathf.Exp(-((t * 8f) % 1f) * 10f);

            data[i] = Mathf.Clamp(bass + lead + arp, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("GameplayMusic", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
