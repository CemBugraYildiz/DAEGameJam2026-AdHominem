using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, float volume)
    {
        // spawn gameObject
        AudioSource audioSource = Instantiate(soundFXObject, transform.position, Quaternion.identity);

        // assign audioClip
        audioSource.clip = audioClip;

        // assign volume
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // get length of soundFX clip
        float clipLength = audioSource.clip.length;

        // destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }
}