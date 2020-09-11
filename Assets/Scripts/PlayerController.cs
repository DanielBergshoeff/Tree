using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject TrianglePrefab;

    private GameObject triangleInteract;
    private GameObject interactingItem;
    private bool inBeetleRange = false;

    private void OnTriangle() {
        if (inBeetleRange) {
            interactingItem.transform.rotation = Quaternion.identity;
            interactingItem.transform.position = interactingItem.transform.position + Vector3.up * 0.75f;
            interactingItem.GetComponent<SphereCollider>().enabled = false;
            inBeetleRange = false;
            Destroy(triangleInteract);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Beetle") && !inBeetleRange) {
            inBeetleRange = true;
            triangleInteract = Instantiate(TrianglePrefab);
            triangleInteract.transform.position = other.transform.position + new Vector3(0f, 2f, 0f);

            interactingItem = other.gameObject;
        }
        if (other.CompareTag("Narrative")) {
            Narrative n = other.GetComponent<Narrative>();
            NarrativeManager.Instance.AddNarrative(n);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Beetle") && inBeetleRange) {
            inBeetleRange = false;
            Destroy(triangleInteract);
        }
    }
}
