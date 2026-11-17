using UnityEngine;

// A trigger volume that tells the MusicBranchManager which branch the player is in.
[RequireComponent(typeof(BoxCollider))]
public class MusicZoneTrigger : MonoBehaviour
{
    public MusicBranchManager manager;
    [Tooltip("Index into the manager's tracks; -1 fades the music out")]
    public int branchIndex = -1;
    [Tooltip("Return to the manager's start branch as soon as the player leaves")]
    public bool revertOnExit = false;

    void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) manager.SetBranch(branchIndex);
    }

    void OnTriggerExit(Collider other)
    {
        if (revertOnExit && other.CompareTag("Player")) manager.SetBranch(manager.startBranch);
    }
}
