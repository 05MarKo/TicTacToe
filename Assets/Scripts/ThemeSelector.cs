using UnityEngine;

public class ThemeSelector : MonoBehaviour
{
    public void SelectSushiTheme()
    {
        ThemeManager.Instance.SetTheme(ThemeManager.ThemeType.Sushi);
    }

    public void SelectCupcakeTheme()
    {
        ThemeManager.Instance.SetTheme(ThemeManager.ThemeType.Cupcake);
    }
}