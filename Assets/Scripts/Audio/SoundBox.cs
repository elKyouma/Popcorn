using System.Collections;
using UnityEngine;

public class SoundBox : MonoBehaviour
{
    private AudioSource source;

    public void Play(AudioClip sound)
    {
        source = GetComponent<AudioSource>();
        StartCoroutine(SoundBoxDestruction(sound.length, sound));
    }

    IEnumerator SoundBoxDestruction(float time, AudioClip sound)
    {
        source.PlayOneShot(sound);
        yield return new WaitForSeconds(time + 1f);
        Destroy(gameObject);
    }
}

