using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;
    public AudioSource backgroundSouce;
    public AudioSource eventSouce;

    public AudioClip eatClip;
    public AudioClip winClip;
    public AudioClip failClip;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance!=this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playClip(AudioClip c)
    {
        eventSouce.clip = c;
        eventSouce.Play();
    }

    public void StopBackground()
    {
        backgroundSouce.Stop();
    }

    public void StartBackground()
    {
        if (!backgroundSouce.isPlaying)
        {
            backgroundSouce.Play();
            backgroundSouce.loop = true;
        }
    }
}
