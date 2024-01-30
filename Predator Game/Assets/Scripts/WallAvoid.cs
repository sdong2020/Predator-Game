using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAvoid : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Walls"))
        {
            Debug.Log("Hello");
            // Handle collision with walls or other trigger objects here
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
        }
    }
}
