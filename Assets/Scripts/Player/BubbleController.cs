using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    public float speed;
    public float destroyTime;

    private bool _timerStarted = false;
    private bool _configure = true;
    private Rigidbody2D _rb;
    private EnemyManager _enemy;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemy = FindObjectOfType<EnemyManager>();
    }

    void Update()
    {
        if (_configure)
        {
            _rb.AddForce(transform.right*speed, ForceMode2D.Impulse);
            _configure = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag("Player")) return;

        if (!_timerStarted) StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        _timerStarted = true;

        yield return new WaitForSeconds(destroyTime);

        _enemy.SetEnemiesTargetToPlayer();
        Destroy(gameObject);
    }
}
