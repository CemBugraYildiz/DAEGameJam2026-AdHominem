using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] public GameObject soundPrefab;
    [SerializeField] private AudioClip testClip;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

    }
    [ContextMenu("Test Play Sound")]

    public void TestPlaySound()
    {
        PlaySoundFXClip(testClip, 1);
    }

    public void PlaySoundFXClip(AudioClip audioClip, float volume)
    {
        // spawn gameObject
        GameObject obj = Instantiate(soundPrefab, transform.position, Quaternion.identity);

        AudioSource audioSource = obj.GetComponent<AudioSource>();

        // assign audioClip
        audioSource.clip = audioClip;

        // assign volume
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // get length of soundFX clip
        float clipLength = audioSource.clip.length;

        // destroy the clip after it's done playing
        Destroy(obj, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, float volume)
    {
        // assign random index
        int rand = Random.Range(0, audioClip.Length);


        PlaySoundFXClip(audioClip[rand], volume);
    }
}