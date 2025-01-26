using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float speed;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector2 _direction;

    [Header("Distraction")]
    public bool canThrowBubbles;
    [SerializeField] GameObject cross;
    [SerializeField] TMP_Text counter;
    [SerializeField] GameObject bubble;
    public int maxBubbles;
    private int bubbleCounter;

    [Header("Death")]
    [SerializeField] GameObject deathMenu;

    private Rigidbody2D _rb;
    private EnemyManager _enemy;

    void Start()
    {
        canThrowBubbles = true;
        cross.SetActive(false);
        bubbleCounter = maxBubbles;
        counter.text = "Bubbles: " + bubbleCounter;
        _rb = GetComponent<Rigidbody2D>();
        _enemy = FindObjectOfType<EnemyManager>();
    }

    void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _direction = new Vector2(_horizontalInput, _verticalInput);

        _rb.velocity = _direction.normalized * speed;

        cross.SetActive(!canThrowBubbles);
        if (Input.GetKeyDown(KeyCode.Space) && canThrowBubbles) ThrowBubble();
    }

    private void ThrowBubble()
    {
        if(bubbleCounter > 0)
        {
            bubbleCounter--;
            counter.text = "Bubbles: " + bubbleCounter;
            Vector3 spawnPosition = transform.position + transform.right;
            GameObject bubbleObj = Instantiate(bubble, spawnPosition, transform.rotation);
            _enemy.SetEnemiesTargetToItem(bubbleObj);
        }
    }
    public void PickBubble()
    {
        bubbleCounter++;
        counter.text = "Bubbles: " + bubbleCounter;
    }
    public void Die()
    {
        deathMenu.SetActive(true);
        Destroy(gameObject);
    }
}
