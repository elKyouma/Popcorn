using UnityEngine;
public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource mainAudioSource;
    [SerializeField] AudioClip[] musicPlayList;
    private int currentMusic = 0;

    [SerializeField] private GameObject soundBoxPrefab;
    private GameObject soundBox;

    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        mainAudioSource.clip = musicPlayList[currentMusic];
        mainAudioSource.Play();
    }

    public void PlaySound(AudioClip sound, Vector3 soundPosition)
    {
        soundBox = Instantiate(soundBoxPrefab, soundPosition, Quaternion.identity, gameObject.transform);
        soundBox.GetComponent<SoundBox>().Play(sound);
    }

    private void Update()
    {
        if (!mainAudioSource.isPlaying)
        {
            currentMusic = currentMusic + 1 > musicPlayList.Length ? musicPlayList.Length - 1 : currentMusic += 1;
            mainAudioSource.clip = musicPlayList[currentMusic];
            mainAudioSource.Play();
        }
    }

}
