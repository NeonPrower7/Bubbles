using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviourScript : MonoBehaviour
{
    [SerializeField] Transform path; // Ссылка на объект со всеми точками передвижения
    [SerializeField] LayerMask enemyLayer;
    public float speed; // Скорость движения врага
    public float chaseSpeed; // Скорость преследования игрока
    public float chaseDistance; // Радиус обнаружения игрока

    private Transform[] waypoints; // Точки пути для движения врага
    private Transform target;
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
        if(target != null) SearchForTarget();

        if (isChasing)
        {
            ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }

    private void SearchForTarget()
    {
        if (Vector3.Distance(transform.position, target.position) <= chaseDistance)
        {
            TurnToTarget();
            Debug.DrawRay(transform.position, transform.up * chaseDistance, Color.red);
            hit = Physics2D.Raycast(transform.position, transform.up, chaseDistance, enemyLayer);
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Target")) isChasing = true;
                else if (hit.transform.CompareTag("Player")) isChasing = true;
                else isChasing = false;
            }
        }
        else isChasing = false;
    }

    private void TurnToTarget()
    {
        Vector2 change = target.position - transform.position;
        float rotation = Mathf.Atan2(change.x, change.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }


    private void ChaseTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, chaseSpeed * Time.deltaTime);
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

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget.transform;
    }

    // Визуализация зоны обнаружения в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
