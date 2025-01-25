using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartHandler(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }

    public void ExitHandler()
    {
        Application.Quit();
    }
}
