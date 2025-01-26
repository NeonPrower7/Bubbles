using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    private PauseMenu pm;

    void Start()
    {
        pm = pauseCanvas.GetComponent<PauseMenu>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pm.isActive)
            {
                pauseCanvas.SetActive(false);
                pm.isActive = false;
                Time.timeScale = 1f;
            }
            else
            {
                pauseCanvas.SetActive(true);
                pm.isActive = true;
                Time.timeScale = 0f;
            }
        }
    }
}
