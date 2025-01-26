using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] BossController boss;
    [SerializeField] AudioSource audio;
    [SerializeField] GameObject door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(boss.PlayAnimation());
        audio.Play();
        door.SetActive(true);
        Destroy(gameObject);
    }
}
