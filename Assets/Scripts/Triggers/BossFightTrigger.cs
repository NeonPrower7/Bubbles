using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] Transform boss;
    [SerializeField] Transform player;
    [SerializeField] AudioSource audio;
    [SerializeField] GameObject door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.transform.CompareTag("Player")) return;

        boss.gameObject.SetActive(true);
        boss.GetComponent<BossController>().PlayAnimation();
        player.GetComponent<PlayerController>().canThrowBubbles = false;
        audio.Play();
        door.SetActive(true);
        Destroy(gameObject);
    }
}
