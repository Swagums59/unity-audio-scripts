using UnityEngine;
using UnityEngine.Audio;

// Audio buses by Zach.
// Uses the mixer at Zach/Resources/Audio/ZachAudioMixer:
//   Master
//   ├── Loops  (music and ambience loops)
//   └── SFX    (one-shot sound effects)
//
// Examples from any script:
//   AudioBus.PlaySfx(myClip);                     // play a sound effect on the SFX bus
//   AudioBus.PlaySfx(myClip, 0.8f);               // ...at 80% volume
//   mySource.outputAudioMixerGroup = AudioBus.Loops;   // route your own AudioSource
//   AudioBus.SetVolume(AudioBus.SfxVolume, 0.5f);      // e.g. from a settings slider (0..1)
public static class AudioBus
{
    public const string MasterVolume = "MasterVolume";
    public const string LoopsVolume = "LoopsVolume";
    public const string SfxVolume = "SFXVolume";

    private static AudioMixer _mixer;
    private static AudioMixerGroup _loops;
    private static AudioMixerGroup _sfx;
    private static AudioSource _sfxSource;

    public static AudioMixer Mixer
    {
        get
        {
            if (_mixer == null)
            {
                _mixer = Resources.Load<AudioMixer>("Audio/ZachAudioMixer");
                if (_mixer == null) Debug.LogWarning("AudioBus: couldn't load Resources/Audio/ZachAudioMixer.");
            }
            return _mixer;
        }
    }

    public static AudioMixerGroup Loops => _loops != null ? _loops : (_loops = FindGroup("Loops"));
    public static AudioMixerGroup Sfx => _sfx != null ? _sfx : (_sfx = FindGroup("SFX"));

    // Plays a 2D one-shot on the SFX bus. Several can overlap.
    public static void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (_sfxSource == null)
        {
            var go = new GameObject("AudioBus SFX");
            Object.DontDestroyOnLoad(go);
            _sfxSource = go.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.outputAudioMixerGroup = Sfx;
        }
        _sfxSource.PlayOneShot(clip, volume);
    }

    // Plays a one-shot at a point in the world (3D) on the SFX bus.
    public static void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        var go = new GameObject("AudioBus SFX 3D");
        go.transform.position = position;
        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.outputAudioMixerGroup = Sfx;
        source.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }

    // Sets a bus volume from 0..1 (slider-friendly). 0 = silent, 1 = unchanged.
    public static void SetVolume(string exposedParameter, float linear01)
    {
        if (Mixer == null) return;
        float db = linear01 > 0.0001f ? Mathf.Log10(linear01) * 20f : -80f;
        Mixer.SetFloat(exposedParameter, db);
    }

    private static AudioMixerGroup FindGroup(string name)
    {
        if (Mixer == null) return null;
        var groups = Mixer.FindMatchingGroups("Master/" + name);
        if (groups.Length == 0)
        {
            Debug.LogWarning($"AudioBus: no mixer group named '{name}'.");
            return null;
        }
        return groups[0];
    }
}
