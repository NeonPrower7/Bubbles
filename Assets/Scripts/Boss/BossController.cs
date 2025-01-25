using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState { Inactive, Ready, Waiting, Dashing, Recharge, Die, Off }

public class BossController : MonoBehaviour
{

    [SerializeField] Transform player;

    public BossState state;

    [Header("Attack")]
    public float rechargeTime;
    public float attackDelayTime;
    public float speed;

    [Header("Die")]
    [SerializeField] GameObject key;

    private Rigidbody2D _rb;
    private int columnCounter = 4;

    void Awake()
    {
        state = BossState.Inactive;
        key.SetActive(false);
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (state == BossState.Ready) StartCoroutine(Attack());
        else if (state == BossState.Waiting) Aiming();
    }

    public IEnumerator PlayAnimation()
    {
        state = BossState.Ready;
        yield return null;
    }

    private void Aiming()
    {
        Vector2 change = player.position - transform.position;
        float rotation = Mathf.Atan2(change.x, change.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }

    private IEnumerator Attack()
    {
        state = BossState.Waiting;
        yield return new WaitForSeconds(rechargeTime);
        state = BossState.Dashing;
        yield return new WaitForSeconds(attackDelayTime);
        _rb.velocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _rb.velocity = Vector3.zero;
        transform.position = new Vector3(0, 21, 0);
        state = BossState.Ready;
        if (other.transform.tag == "Column")
        {
            Destroy(other.gameObject);
            columnCounter--;
            if(columnCounter <= 0) Die();
        }
    }

    private void Die()
    {
        state = BossState.Die;
        key.SetActive(true);
        Destroy(gameObject);
    }
}
