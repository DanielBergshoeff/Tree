using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }

    private AudioSource myAudioSource;

    private List<Narrative> narratives;

    private Narrative currentNarrative;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        narratives = new List<Narrative>();
        myAudioSource = GetComponent<AudioSource>();
    }

    private void RemoveOldNarrative() {
        currentNarrative = null;
        CheckForNewNarrative();
    }

    private void CheckForNewNarrative() {
        if(narratives.Count > 0) {
            currentNarrative = narratives[0];
            narratives.RemoveAt(0);
            myAudioSource.PlayOneShot(currentNarrative.Audio);

            Invoke("RemoveOldNarrative", currentNarrative.Audio.length + 1f);
        }
    }

    public void AddNarrative(Narrative narrative) {
        narratives.Add(narrative);

        if (currentNarrative == null)
            CheckForNewNarrative();
    }
}
