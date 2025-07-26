using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingDrone : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float moveSpeed;
    public Vector3 axis;
    public float maxDistance;

    float angle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            OrbitAroundTarget();
    }

    void OrbitAroundTarget()
    {
        angle += moveSpeed * Time.deltaTime;
        float x = Mathf.Cos(angle) * maxDistance;
        float z = Mathf.Sin(angle) * maxDistance;

        transform.position = target.position + new Vector3(x, 0, z);
    }
}
