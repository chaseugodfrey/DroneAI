using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;
    public float distance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = target.position + (target.forward * -1) * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
    }
}
