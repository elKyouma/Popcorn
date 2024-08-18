using System.Collections;
using UnityEngine;

public class SoundBox : MonoBehaviour
{
    private AudioSource source;

    public void Play(AudioClip sound)
    {
        source = GetComponent<AudioSource>();
        source.PlayOneShot(sound);
        StartCoroutine(SoundBoxDestruction(sound.length));
    }

    IEnumerator SoundBoxDestruction(float time)
    {
        yield return new WaitForSeconds(time + 0.1f);
        Destroy(gameObject);
    }
}

