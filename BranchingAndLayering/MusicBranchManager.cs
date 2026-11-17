using System.Collections;
using UnityEngine;

// Branching: holds one looping music track per level branch and crossfades to
// whichever branch the player last walked into (index -1 = silence).
// startBranch plays from the first frame of the game.
public class MusicBranchManager : MonoBehaviour
{
    [Tooltip("One looping AudioSource per branch, in branch order")]
    public AudioSource[] tracks;

    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Tooltip("Seconds for the new branch to fade in")]
    public float fadeInTime = 2.5f;
    [Tooltip("Seconds for the branch you leave to fade out")]
    public float fadeOutTime = 0.75f;
    [Tooltip("Branch playing when the game starts (-1 = silence)")]
    public int startBranch = 0;

    int current = -1;
    Coroutine fade;

    void Start()
    {
        for (int i = 0; i < tracks.Length; i++)
        {
            tracks[i].volume = i == startBranch ? musicVolume : 0f;
            tracks[i].loop = true;
            if (!tracks[i].isPlaying) tracks[i].Play();
        }
        current = startBranch;
    }

    public void SetBranch(int index)
    {
        if (index == current) return;
        current = index;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade());
    }

    IEnumerator Crossfade()
    {
        bool done = false;
        while (!done)
        {
            done = true;
            for (int i = 0; i < tracks.Length; i++)
            {
                bool fadingIn = i == current;
                float target = fadingIn ? musicVolume : 0f;
                float speed = musicVolume / Mathf.Max(0.01f, fadingIn ? fadeInTime : fadeOutTime);
                tracks[i].volume = Mathf.MoveTowards(tracks[i].volume, target, speed * Time.deltaTime);
                if (!Mathf.Approximately(tracks[i].volume, target)) done = false;
            }
            yield return null;
        }
    }
}
