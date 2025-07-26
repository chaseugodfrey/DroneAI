using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentController : MonoBehaviour
{

    Camera cam;
    LayerMask blockLayer;
    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        blockLayer = LayerMask.GetMask("Block");
        agent = GetComponent<NavMeshAgent>();
    }

    void SetTargetLocation()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit currentHit;

        if (Physics.Raycast(ray, out currentHit, 10000, blockLayer))
        {
            agent.SetDestination(currentHit.point);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SetTargetLocation();
        }
    }
}
