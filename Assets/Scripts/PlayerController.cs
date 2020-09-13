using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject TrianglePrefab;
    public Image FadeInScreen;

    private GameObject triangleInteract;
    private GameObject interactingItem;
    private bool inBeetleRange = false;

    private float startTime = 0f;
    public bool FadingIn = false;
    private float fullFadeTime = 3f;

    private Color color;

    private void Start() {
        FadeIn(22f);
    }

    private void Update() {
        if (FadingIn) {
            float percent = 1f - (Time.time - startTime) / fullFadeTime;
            if (percent > 0f) {
                color.a = percent;
                FadeInScreen.color = color;
            }
            else {
                color.a = 0f;
                FadeInScreen.color = color;
                FadingIn = false;
            }
        }
    }

    private void FadeIn(float time) {
        FadeInScreen.enabled = true;
        startTime = Time.time;
        FadingIn = true;
        color = FadeInScreen.color;
        fullFadeTime = time;
    }

    private void OnTriangle() {
        if (inBeetleRange) {
            interactingItem.transform.rotation = Quaternion.identity;
            interactingItem.transform.position = interactingItem.transform.position + Vector3.up * 0.75f;
            interactingItem.GetComponent<SphereCollider>().enabled = false;
            inBeetleRange = false;
            NarrativeManager.Instance.AddNarrative(interactingItem.GetComponent<Narrative>());
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
            Destroy(other.gameObject.GetComponent<Collider>());
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Beetle") && inBeetleRange) {
            inBeetleRange = false;
            Destroy(triangleInteract);
        }
    }
}
