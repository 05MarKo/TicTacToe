using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIFader : MonoBehaviour
{
    public static UIFader Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Singleton pattern to ensure one instance
    }

    public void FadeImage(Image image, float targetAlpha, float duration)
    {
        StartCoroutine(FadeAlpha(image, targetAlpha, duration));
    }

    public void FadeText(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        StartCoroutine(FadeTextAlpha(text, targetAlpha, duration));
    }

    private IEnumerator FadeAlpha(Image image, float targetAlpha, float duration)
    {
        float startAlpha = image.color.a;
        float time = 0f;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            Color c = image.color;
            c.a = alpha;
            image.color = c;
            time += Time.deltaTime;
            yield return null;
        }

        Color final = image.color;
        final.a = targetAlpha;
        image.color = final;
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        float startAlpha = text.color.a;
        float time = 0f;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            Color c = text.color;
            c.a = alpha;
            text.color = c;
            time += Time.deltaTime;
            yield return null;
        }

        Color final = text.color;
        final.a = targetAlpha;
        text.color = final;
    }

    public void FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, bool makeInteractable = true)
    {
        StartCoroutine(FadeCanvasGroupCoroutine(cg, targetAlpha, duration, makeInteractable));
    }

    private IEnumerator FadeCanvasGroupCoroutine(CanvasGroup cg, float targetAlpha, float duration, bool makeInteractable)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = targetAlpha;
        cg.interactable = makeInteractable && targetAlpha > 0.5f;
        cg.blocksRaycasts = makeInteractable && targetAlpha > 0.5f;
    }
}