using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
   public void PlayGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
