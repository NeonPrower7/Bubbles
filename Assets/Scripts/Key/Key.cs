using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")) {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            inventory.AddKey();
            Destroy(gameObject);
        }
    }
}
