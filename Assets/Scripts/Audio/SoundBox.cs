using System.Collections;
using UnityEngine;

public class SoundBox : MonoBehaviour
{
    private AudioSource source;

    public void Play(AudioClip sound, float volume)
    {
        source = GetComponent<AudioSource>();
        source.volume = volume;
        StartCoroutine(SoundBoxDestruction(sound.length, sound));
    }

    IEnumerator SoundBoxDestruction(float time, AudioClip sound)
    {
        source.PlayOneShot(sound);
        yield return new WaitForSeconds(time + 1f);
        Destroy(gameObject);
    }
}

