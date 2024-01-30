using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour
{
    float moveSpeed = 5;

    // How much the wall will move
    public float moveDistance; 
    private Vector3 initialPosition;
    
    private bool movingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float moveDelta = moveSpeed * Time.deltaTime;

        // Move right or left alternating
        if (movingRight) {
            transform.Translate(transform.right * moveDelta);
        }
        else {
            transform.Translate(-transform.right * moveDelta);
        }

        // Switches which way you should move.
        if (Vector3.Distance(transform.position, initialPosition) >= moveDistance)
        {
            movingRight = !movingRight;
        }
            
    }
}
