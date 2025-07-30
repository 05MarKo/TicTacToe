using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    public enum ThemeType { Sushi, Cupcake }
    public ThemeType currentTheme = ThemeType.Sushi;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps this object alive between scenes
        }
        else
        {
            Destroy(gameObject); // ensures only one exists
        }
    }

    public void SetTheme(ThemeType theme)
    {
        currentTheme = theme;
    }

    public ThemeType GetCurrentTheme()
    {
        return currentTheme;
    }
}
