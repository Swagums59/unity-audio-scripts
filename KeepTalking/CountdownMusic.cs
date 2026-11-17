using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Layered countdown music by Zach.
// Put this on the same GameObject as the UI Document that shows the countdown.
// It reads the time from the "Countdown" label (mm:ss), so the countdown script
// itself doesn't need any changes.
//
// It uses two normal AudioSource components that you can edit outside Play mode
// (clip, volume, pitch, output bus...). Adding this component creates them for you;
// if they're missing, right-click this component > "Set Up Audio Sources".
//
// - Base source: loops the whole time.
// - Tension source: loops in sync underneath at volume 0, then fades in to the
//   volume set on its AudioSource when the timer gets low.
[RequireComponent(typeof(UIDocument))]
public class CountdownMusic : MonoBehaviour
{
    [Header("Audio sources (edit their clip / volume / pitch on the components)")]
    [Tooltip("Plays the whole time.")]
    public AudioSource baseSource;
    [Tooltip("Fades in on top of the base theme when the timer gets low. Its Volume is the level it fades up to.")]
    public AudioSource tensionSource;

    [Header("When the tension theme comes in")]
    [Tooltip("Name of the Label in the UI Document that shows the time (mm:ss).")]
    public string countdownLabelName = "Countdown";
    [Tooltip("Seconds left on the timer when the tension theme fades in.")]
    public float lowTimeThreshold = 60f;
    public float tensionFadeInSeconds = 2f;

    private const string BaseClipPath = "Audio/Music/music_alert_base_loop";
    private const string TensionClipPath = "Audio/Music/music_alert_tension_loop";

    private Label _countdownLabel;
    private float _tensionTargetVolume;
    private bool _tensionActive = false;

    // Runs in the editor when the component is added (or Reset from its menu).
    void Reset()
    {
        SetUpAudioSources();
    }

    [ContextMenu("Set Up Audio Sources")]
    public void SetUpAudioSources()
    {
        if (baseSource == null) baseSource = AddSource(BaseClipPath);
        if (tensionSource == null) tensionSource = AddSource(TensionClipPath);

        // Fill in any empty clip slots with the default loops.
        if (baseSource.clip == null) baseSource.clip = Resources.Load<AudioClip>(BaseClipPath);
        if (tensionSource.clip == null) tensionSource.clip = Resources.Load<AudioClip>(TensionClipPath);
#if UNITY_EDITOR
        EditorUtility.SetDirty(baseSource);
        EditorUtility.SetDirty(tensionSource);
        EditorUtility.SetDirty(this);
#endif
    }

    private AudioSource AddSource(string clipPath)
    {
#if UNITY_EDITOR
        var source = Application.isPlaying
            ? gameObject.AddComponent<AudioSource>()
            : Undo.AddComponent<AudioSource>(gameObject);
#else
        var source = gameObject.AddComponent<AudioSource>();
#endif
        source.clip = Resources.Load<AudioClip>(clipPath);
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D music
        source.volume = 0.7f;     // both together stay under 0 dBFS
        source.outputAudioMixerGroup = AudioBus.Loops;
        return source;
    }

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _countdownLabel = root.Q<Label>(countdownLabelName);
        if (_countdownLabel == null)
            Debug.LogWarning($"CountdownMusic: no Label named '{countdownLabelName}' found, tension theme won't trigger.");

        // Safety net: if the sources were never set up, make them now.
        if (baseSource == null || tensionSource == null || baseSource.clip == null || tensionSource.clip == null)
            SetUpAudioSources();

        _tensionTargetVolume = tensionSource.volume;
        tensionSource.volume = 0f; // silent until time runs low

        // Start both on the same audio clock tick so they stay locked together.
        double startTime = AudioSettings.dspTime + 0.1;
        foreach (var source in new[] { baseSource, tensionSource })
        {
            source.loop = true;
            source.Stop(); // in case Play On Awake was ticked
            source.PlayScheduled(startTime);
        }
    }

    void Update()
    {
        if (tensionSource == null) return;

        if (!_tensionActive && TryGetSecondsLeft(out int secondsLeft) && secondsLeft <= lowTimeThreshold)
        {
            _tensionActive = true;
            Debug.Log("CountdownMusic: time is low, tension theme in.");
        }

        if (_tensionActive && tensionSource.volume < _tensionTargetVolume)
        {
            float step = tensionFadeInSeconds > 0f
                ? _tensionTargetVolume / tensionFadeInSeconds * Time.deltaTime
                : _tensionTargetVolume;
            tensionSource.volume = Mathf.Min(_tensionTargetVolume, tensionSource.volume + step);
        }
    }

    // Reads "mm:ss" from the label. Returns false while the timer hasn't started
    // (the label is empty or shows other text).
    private bool TryGetSecondsLeft(out int seconds)
    {
        seconds = 0;
        if (_countdownLabel == null || string.IsNullOrEmpty(_countdownLabel.text)) return false;

        string[] parts = _countdownLabel.text.Split(':');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out int minutes) || !int.TryParse(parts[1], out int secs)) return false;

        seconds = minutes * 60 + secs;
        return true;
    }
}
