using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] GameObject player;
    private List<MobBehaviourScript> enemies = new List<MobBehaviourScript>();

    void Start()
    {
        // Находим все врагов на сцене
        enemies.AddRange(FindObjectsOfType<MobBehaviourScript>());
        SetEnemiesTargetToPlayer();
    }

    // Метод, который вызывается, чтобы изменить цель всех врагов на предмет
    public void SetEnemiesTargetToItem(GameObject item)
    {
        foreach (var enemy in enemies)
        {
            enemy.SetTarget(item);
        }
    }

    // Метод, который вызывается, чтобы вернуть врагов к преследованию игрока
    public void SetEnemiesTargetToPlayer()
    {
        foreach (var enemy in enemies)
        {
            enemy.SetTarget(player);
        }
    }
}