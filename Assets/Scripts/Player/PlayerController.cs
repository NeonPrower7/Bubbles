using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float speed;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector2 _direction;

    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _direction = new Vector2(_horizontalInput, _verticalInput);

        _rb.velocity = _direction.normalized * speed;
    }
}
