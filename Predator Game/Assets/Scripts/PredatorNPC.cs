using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class PredatorNPC : MonoBehaviour
{
    // Tracks the position and direction of the predator
    private Vector3 position;
    private Quaternion direction;

    // Keeps track of what state the predator is in.
    private PredatorMode predMode = PredatorMode.wander;

    private RaycastHit hitInfo;

    public float moveSpeed = 3f; 
    public float rotationSpeed = 5f; 
    public float visionLength = 6f;

    // The locations for respawning the prey
    private Vector3 respawnLoc1 = new Vector3(-16f, 1.05f, 8f);
    private Vector3 respawnLoc2 = new Vector3(16f, 1.05f, 9.5f);

    private int respawnPoint = 1;

    // The different possible states
    public enum PredatorMode
    {
        wander,
        chase,
        avoidWall,
    }


    public void OnTriggerEnter(Collider collider)
    {
        // Activates when collides with a prey.
        if (collider.gameObject.tag == "Prey")
        {
            // Alternates the respawn point that the prey respawns from after being killed.
            if (respawnPoint == 1)
            {
                collider.gameObject.transform.position = respawnLoc1;
                collider.gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                respawnPoint = 0;
            }
            else
            {
                collider.gameObject.transform.position = respawnLoc2;
                collider.gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                respawnPoint = 1;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        position = transform.position;
        direction = transform.rotation;

        // Locks the characters so that they will remain level on the floor and so that they only rotate left and right

        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Calls the method to find what state the predator should be in.
        predMode = stateFinder();

        // This is the finite state machine. Each state calls a specfic action.
        switch (predMode)
        {
            case PredatorMode.wander:
                Wandering();
                break;
            case PredatorMode.avoidWall:
                TurnFromWall();
                break;
            case PredatorMode.chase:
                Chasing();
                break;
            default:
                break;
        }
    }
    private void Wandering()
    {
        
        float randomRotation = Random.Range(-180f, 180f);

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
        predMode = PredatorMode.wander;

    }
    private void Chasing()
    {
        // Looks at the prey and moves towards it.
        transform.LookAt(hitInfo.transform);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private PredatorMode stateFinder()
    {
        int rayCount = 6;
        Debug.DrawRay(transform.position, transform.forward * visionLength, Color.red);

        // This handles the ray that goes directly in front of the character. The middle raycast is the one that handles looking for and avoiding walls.
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, visionLength))
        {
            // Gets the object that is hit by the front ray cast.
            GameObject hitObject = hitInfo.collider.gameObject;

            // If the object hit is of type predator the set the state to flee.
            if (hitObject.CompareTag("Prey"))
            {
                return PredatorMode.chase;
            }
            // Else if the object wasn't a predator, then if you are within 2 units of something avoid it by setting the state to avoidwall.
            else if (hitInfo.distance < 2)
            {
                return PredatorMode.avoidWall;
            }
        }

        // This loop handles the raycasts on the right side of the character. These side raycast only check for preys. They do not matter for avoiding walls.
        for (int i = 1; i < rayCount; i++) {
            // Distrubtes the rays evenly 
            float angle = i * 30f / rayCount;
            Debug.Log(angle);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            Debug.DrawRay(transform.position, direction * visionLength, Color.green);

            if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hitInfo, visionLength))
            {
                // Gets the object that is hit by the front ray cast.
                GameObject hitObject = hitInfo.collider.gameObject;

                // If the object hit is of type prey the set the state to chase.
                if (hitObject.CompareTag("Prey"))
                {
                    return PredatorMode.chase;
                }
            }
        }

        // This loop handles the raycasts on the left side of the character. These side raycast only check for prey. They do not matter for avoiding walls.
        for (int i = 1; i < rayCount; i++)
        {
            // Distrubtes the rays evenly 
            float angle = i * -30f / rayCount;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            Debug.DrawRay(transform.position, direction * visionLength, Color.green);

            if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hitInfo, visionLength))
            {
                // Gets the object that is hit by the front ray cast.
                GameObject hitObject = hitInfo.collider.gameObject;

                // If the object hit is of type prey the set the state to chase.
                if (hitObject.CompareTag("Prey"))
                {
                    return PredatorMode.chase;
                }
            }
        }

        // If the object hit is of type predator the set the state to wander.
        return PredatorMode.wander ;
    }

}


