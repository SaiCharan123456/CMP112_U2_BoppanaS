using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))] 

public abstract class PickUp : MonoBehaviour {

    protected virtual void Awake() {

        WaitAfterSpawning();
        // Ensure trigger
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }


    // A coroutine to wait for 2 seconds after spawning
    private IEnumerator WaitAfterSpawning()
    {
        yield return new WaitForSeconds(2);
    }


    // Trigger detection
    private void OnTriggerEnter(Collider other) {

        if (!other.CompareTag("Player"))
        {
            return;
        }

        OnPickUp(other.gameObject);
    }

    // Abstract method to define pick-up behavior
    protected abstract void OnPickUp(GameObject player);
} 