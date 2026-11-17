using UnityEngine;

// One-shot: plays a sound once when the player enters this trigger volume.
[RequireComponent(typeof(BoxCollider))]
public class OneShotTrigger : MonoBehaviour
{
    public AudioSource source;
    [Tooltip("Off = plays again every time the player re-enters")]
    public bool playOnlyOnce = false;

    bool played;

    void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || (playOnlyOnce && played)) return;
        source.PlayOneShot(source.clip);
        played = true;
    }
}
