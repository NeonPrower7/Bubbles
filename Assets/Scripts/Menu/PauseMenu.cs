using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool isActive = false;

    public void ContinueHandler()
    {
        isActive = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OptionsHandler()
    {

    }

    public void MainMenuHandler()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}
