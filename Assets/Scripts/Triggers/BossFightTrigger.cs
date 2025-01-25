using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] BossController boss;
    [SerializeField] GameObject door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(boss.PlayAnimation());
        door.SetActive(true);
        Destroy(gameObject);
    }
}
