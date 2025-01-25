using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieOnStart : MonoBehaviour
{
    void Start()
    {
        if (Application.isPlaying)
            DestroyImmediate(gameObject);
    }
}
