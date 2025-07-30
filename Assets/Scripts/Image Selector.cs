using UnityEngine;

public class ImageSelector : MonoBehaviour
{
    public GameObject imageA;
    public GameObject imageB;

    public void ShowImageA()
    {
        imageA.SetActive(true);
        imageB.SetActive(false);
    }

    public void ShowImageB()
    {
        imageA.SetActive(false);
        imageB.SetActive(true);
    }
}