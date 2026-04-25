using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] musicTracks;
    private AudioSource musicSource;

    void Awake()
    {
        // Singleton Pattern: Keeps the manager alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();

        AudioManager.Instance.PlayMusic("Music");
        
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicTracks, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        musicSource.clip = s.clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}