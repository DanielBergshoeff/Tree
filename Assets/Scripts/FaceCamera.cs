using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 lookRotation = transform.position - Camera.main.transform.position;
        transform.rotation = Quaternion.LookRotation(lookRotation);
    }
}
