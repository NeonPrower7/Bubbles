using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBehaviourScript : MonoBehaviour
{
public Transform[] waypoints; // Точки пути для движения врага
    public float speed = 2f; // Скорость движения врага
    public float chaseSpeed = 4f; // Скорость преследования игрока
    public float chaseDistance = 5f; // Радиус обнаружения игрока
    public Transform player; // Ссылка на объект игрока
    public float killDistance = 0.5f; // Дистанция для убийства игрока

    private int currentWaypointIndex = 0;
    private bool isChasing = false;

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            // Начинаем преследовать игрока
            isChasing = true;
        }
        else
        {
            // Продолжаем двигаться по траектории
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        // Проверка на убийство игрока
        if (Vector3.Distance(transform.position, player.position) <= killDistance)
        {
            KillPlayer();
        }
    }

    void Patrol()
    {
        // Движение по траектории
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void ChasePlayer()
    {
        // Преследование игрока
        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    void KillPlayer()
    {
        // Действия при убийстве игрока (например, перезагрузка сцены)
        Debug.Log("Player Killed");
        // Здесь можно добавить логику для перезагрузки сцены или уменьшения здоровья игрока
    }

    // Визуализация зоны обнаружения в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
