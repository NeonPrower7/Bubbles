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
    [SerializeField] TMP_Text counter;
    [SerializeField] GameObject bubble;
    public int maxBubbles;
    private int bubbleCounter;

    private Rigidbody2D _rb;
    private EnemyManager _enemy;

    void Start()
    {
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

        if (Input.GetKeyDown(KeyCode.Space)) ThrowBubble();
    }

    private void ThrowBubble()
    {
        if(bubbleCounter > 0)
        {
            bubbleCounter--;
            counter.text = "Bubbles: " + bubbleCounter;
            GameObject bubbleObj = Instantiate(bubble, transform.position, transform.rotation);
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
        Destroy(gameObject);
    }
}
