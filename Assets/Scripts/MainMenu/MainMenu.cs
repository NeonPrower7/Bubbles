using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadA(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
