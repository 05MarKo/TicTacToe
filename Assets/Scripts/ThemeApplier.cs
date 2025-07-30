using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ThemeSprites
{
    public List<Sprite> squareSprites; // One for each board button
    public Sprite xSprite;
    public Sprite oSprite;
    public Sprite mainMenuSprite;
    public Sprite resetSprite;
    public Sprite backgroundSprite;
}

public class ThemeApplier : MonoBehaviour
{
    public ThemeSprites sushiTheme;
    public ThemeSprites cupcakeTheme;

    public List<Image> squareButtons;
    public Image xIcon;
    public Image oIcon;
    public Button mainMenuButton;
    public Button resetButton;
    public Image backgroundImage;

    void Start()
    {
        if (ThemeManager.Instance == null)
        {
            Debug.LogWarning("No ThemeManager found!");
            return;
        }

        var theme = ThemeManager.Instance.GetCurrentTheme();
        if (theme == ThemeManager.ThemeType.Sushi)
            ApplyTheme(sushiTheme);
        else
            ApplyTheme(cupcakeTheme);
    }

    void ApplyTheme(ThemeSprites theme)
    {
        for (int i = 0; i < squareButtons.Count; i++)
        {
            squareButtons[i].sprite = theme.squareSprites[i];
        }

        xIcon.sprite = theme.xSprite;
        oIcon.sprite = theme.oSprite;
        mainMenuButton.image.sprite = theme.mainMenuSprite;
        resetButton.image.sprite = theme.resetSprite;
        backgroundImage.sprite = theme.backgroundSprite;
    }
}