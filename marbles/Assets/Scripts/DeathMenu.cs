using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene("singleplayer", LoadSceneMode.Single);
    }

    public void Menu()
    {
        SceneManager.LoadScene("menu", LoadSceneMode.Single);
    }
}