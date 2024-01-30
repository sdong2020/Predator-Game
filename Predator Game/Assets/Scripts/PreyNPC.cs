using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PredatorNPC;
using static PreyNPC;
using static UnityEngine.GraphicsBuffer;

public class PreyNPC : MonoBehaviour
{

    // Tracks the position and direction of the prey
    private Vector3 position;
    private Quaternion direction;

    // Keeps track of what state the prey is in.
    private PreyMode preyMode = PreyMode.wander;

    private RaycastHit hitInfo;
   
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float visionLength = 4;

    private int rayCount = 20;
    

    // The different possible states
    public enum PreyMode
    {
        wander,
        flee,
        avoidWall,
    }

    // Update is called once per frame
    void Update()
    {
        position = transform.position;
        direction = transform.rotation;

        // Locks the characters so that they will remain level on the floor and so that they only rotate left and right
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Calls the method to find what state the prey should be in.
        preyMode = stateFinder();

        // This is the finite state machine. Each state calls a specfic action.
        switch (preyMode)
        {
            case PreyMode.wander:
                Wandering();
                break;
            case PreyMode.avoidWall:
                TurnFromWall();
                break;
            case PreyMode.flee:
                Fleeing();
                break;
            default:
                break;
        }
    }
    private void Wandering()
    {
        
        float randomRotation = Random.Range(-120f, 120f);
        
        // This rotates the character left and right randomly and moves the character forward. Creates random wandering.
        transform.Rotate(Vector3.up * randomRotation * rotationSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

    }

    private void TurnFromWall()
    {
        // Gets the normal of the wall that is about to be hit
        Vector3 hitNormal = hitInfo.normal;

        // Reflects the characters direction across the normal
        Vector3 reflection = Vector3.Reflect(transform.forward, hitInfo.normal);

        // Rotate the character to face the reflection direction
        transform.rotation = Quaternion.LookRotation(reflection);

        // Exits the avoid wall stand and goes back to wandering.
        preyMode = PreyMode.wander;

    }
    private void Fleeing()
    {
        // Finds the the direction that is opposite of where the predator is.
        Vector3 directionToTarget = hitInfo.transform.position - transform.position;
        Quaternion rotationToLookAway = Quaternion.LookRotation(-directionToTarget);

        float smoothness = 5f; 

        // Rotates towards the current rotation using interpolation.
        Quaternion newRotation = Quaternion.Slerp(transform.rotation, rotationToLookAway, smoothness * Time.deltaTime);

        // Apply the new rotation to your character's transform.
        transform.rotation = newRotation;

        // Move away from the predator
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private PreyMode stateFinder()
    {
        Debug.DrawRay(transform.position, transform.forward * visionLength, Color.red);

        // This handles the ray that goes directly in front of the character. The middle raycast is the one that handles looking for and avoiding walls.
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, visionLength))
        {
            // Gets the object that is hit by the front ray cast.
            GameObject hitObject = hitInfo.collider.gameObject;

            // If the object hit is of type predator the set the state to flee.
            if (hitObject.CompareTag("Predator"))
            {
                return PreyMode.flee;
            }
            // Else if the object wasn't a predator, then if you are within 2 units of something avoid it by setting the state to avoidwall.
            else if (hitInfo.distance < 2)
            {
                return PreyMode.avoidWall;
            }
        }

        // This loop handles the raycasts on the right side of the character. These side raycast only check for predators. They do not matter for avoiding walls.
        for (int i = 1; i < rayCount; i++)
        {   
            // Distrubtes the rays evenly 
            float angle = i * 140f / rayCount;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            Debug.DrawRay(transform.position, direction * visionLength, Color.green);

            if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hitInfo, visionLength))
            {
                // Gets the object that is hit by the front ray cast.
                GameObject hitObject = hitInfo.collider.gameObject;

                // If the object hit is of type predator the set the state to flee.
                if (hitObject.CompareTag("Predator"))
                {
                    return PreyMode.flee;
                }
            }
        }

        // This loop handles the raycasts on the left side of the character. These side raycast only check for predators. They do not matter for avoiding walls.
        for (int i = 1; i < rayCount; i++)
        {
            // Distrubtes the rays evenly 
            float angle = i * -140f / rayCount;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            Debug.DrawRay(transform.position, direction * visionLength, Color.green);

            if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hitInfo, visionLength))
            {
                // Gets the object that is hit by the front ray cast.
                GameObject hitObject = hitInfo.collider.gameObject;

                // If the object hit is of type predator the set the state to flee.
                if (hitObject.CompareTag("Predator"))
                {
                    return PreyMode.flee;
                }
            }
        }

        // If there is no predator to flee from or anything to avoid, then the state is set into wander.
        return PreyMode.wander;
    }

}


