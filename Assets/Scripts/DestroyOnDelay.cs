using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDelay : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume;

    private void Start()
    {
        SoundManager.Instance.PlaySound(clip, transform.position, volume);
        Destroy(gameObject, clip.length);
    }
}
