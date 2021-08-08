using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialToTheBeat : MonoBehaviour
{
    public Material subject;
    public AudioSource audioSource;

    public ProtoDanceJuggler songInfoSource;

    protected float timeSamplesPerBeat;

    public Color originalColor;
    public Color beatColor;
    // Start is called before the first frame update
    void Start()
    {
        timeSamplesPerBeat = 60f / songInfoSource.BeatsPerMinute * audioSource.clip.frequency;
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            var colorvariable = Mathf.Cos(Mathf.PI * 2 * ( (audioSource.timeSamples % timeSamplesPerBeat) / timeSamplesPerBeat ) );
            colorvariable = (colorvariable + 1) * .5f;
            subject.color = Color.Lerp(originalColor, beatColor, colorvariable);
        }
        
    }
}
