using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMover : MonoBehaviour
{
    //Rigidbody
    private Rigidbody rb;

    [SerializeField]
    private float maxSpeed = 2, acceleration = 50, deacceleration = 100; 
    [SerializeField]
    private float currentSpeed = 0;
    
    
    private Vector3 oldMovementInput;
    public Vector3 MovementInput { get; set; }

    void Awake()
    {
        // 3D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (MovementInput.magnitude > 0 && currentSpeed >= 0)
        {
            oldMovementInput = MovementInput;
            currentSpeed += acceleration * maxSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;
        }
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        
        // 3D 물리 속도 적용 (X, Y, Z 모두 반영)
        rb.velocity = oldMovementInput * currentSpeed;
    }
}