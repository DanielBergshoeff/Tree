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
        narratives = new List<Narrative>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckForNewNarrative() {
        if(narratives.Count > 0) {
            currentNarrative = narratives[0];
            myAudioSource.PlayOneShot(currentNarrative.Audio);
        }
    }

    public void AddNarrative(Narrative narrative) {
        narratives.Add(narrative);
    }
}
