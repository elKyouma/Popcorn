using UnityEngine;
using UnityEngine.UI;

public class BackgroundFader : MonoBehaviour
{
    public static BackgroundFader Instance;
    public Image backgroundImage; 
    public float fadeDuration = 1.0f; 


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        SetAlpha(0);
        backgroundImage = GetComponent<Image>();
    }

    public void FadeIn(float targetAlpha = 0.7f)
    {
        backgroundImage.enabled = true;
        //PrimeTween.Tween.Custom(backgroundImage.color.a, targetAlpha, fadeDuration, SetAlpha);
        LeanTween.value(gameObject, SetAlpha, backgroundImage.color.a, targetAlpha, fadeDuration).setIgnoreTimeScale(true);
    }

    public void FadeOut()
    {
        backgroundImage.enabled = false;
        //PrimeTween.Tween.Custom(backgroundImage.color.a, targetAlpha, fadeDuration, SetAlpha);

        LeanTween.value(gameObject, SetAlpha, backgroundImage.color.a, 0, fadeDuration).setIgnoreTimeScale(true);
    }

    private void SetAlpha(float alpha)
    {
        Color color = backgroundImage.color;
        color.a = alpha;
        backgroundImage.color = color;
    }
}
