using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBehaviourScript : MonoBehaviour
{
    [SerializeField] Transform path; // Ссылка на объект со всеми точками передвижения
    [SerializeField] Transform player; // Ссылка на объект игрока
    [SerializeField] LayerMask enemyLayer;
    public float speed; // Скорость движения врага
    public float chaseSpeed; // Скорость преследования игрока
    public float chaseDistance; // Радиус обнаружения игрока
    public float killDistance; // Дистанция для убийства игрока

    private Transform[] waypoints; // Точки пути для движения врага
    private RaycastHit2D hit;
    private int currentWaypointIndex = 0;
    private bool isChasing = false;

    void Awake()
    {
        // Собираем все точки передвижения
        waypoints = new Transform[path.childCount];
        for(int i = 0; i < path.childCount; i++)
        {
            waypoints[i] = path.GetChild(i).transform;
        }
    }

    void Update()
    {
        TurnToPlayer();
        SearchForPlayer();

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

    private void TurnToPlayer()
    {
        Vector2 change = player.position - transform.position;
        float rotation = Mathf.Atan2(change.x, change.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }

    private void SearchForPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            Debug.DrawRay(transform.position, transform.up * chaseDistance, Color.red);
            hit = Physics2D.Raycast(transform.position, transform.up, chaseDistance, enemyLayer);
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Player")) isChasing = true;
                else if (hit.transform.CompareTag("Target")) isChasing = true;
                else isChasing = false;
            }
        }
        else isChasing = false;
    }

    private void ChasePlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }


    private void KillPlayer()
    {
        // Действия при убийстве игрока (например, перезагрузка сцены)
        Debug.Log("Player Killed");
        Destroy(player.gameObject);
        // Здесь можно добавить логику для перезагрузки сцены или уменьшения здоровья игрока
    }

    // Визуализация зоны обнаружения в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
