using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float attackDistance;
    [SerializeField] private float gravityScale;
    [SerializeField] private float jumpForce;
    [SerializeField] private float blockDuration;
    [SerializeField] private float attackPower;
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private float healthPoints;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
