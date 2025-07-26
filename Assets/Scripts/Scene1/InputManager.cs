using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public bool isEdit;

    public TerrainManager terrainManager;
    public LayerMask blockLayer;
    Camera cam;

    SimulationManager simulationManager;
    CameraController camRig;
    RaycastHit currentHit;
    GameObject currentObject;
    GameObject highlightedObject;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        camRig = cam.gameObject.GetComponent<CameraController>();
        simulationManager = GameObject.Find("SimulationManager").GetComponent<SimulationManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            ToggleEditMode();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            camRig.ControlCamera();
            return;
        }

        TrackMouseRay();

        if (isEdit)
            EditMode();

        else
            SimulationMode();
    }

    void TrackMouseRay()
    {
        currentObject = null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out currentHit, 10000, blockLayer))
        {
            currentObject = currentHit.collider.gameObject;
        }
    }

    void EditMode()
    {
        if (currentObject != null)
        {
            if (highlightedObject == null || currentObject != highlightedObject)
            {
                ResetHightlight();
                HighlightBlock(currentObject);
            }

            if (Input.GetMouseButtonDown(0))
                terrainManager.CreateBlock(currentObject, currentHit.point);

            if (Input.GetMouseButtonDown(1))
                terrainManager.DeleteBlock(currentObject);
        }

        else if (highlightedObject != null)
        {
            ResetHightlight();
        }
    }
    void SimulationMode()
    {
        if (Input.GetMouseButtonDown(0) && currentObject != null)
        {
            simulationManager.SetDestination(terrainManager.WorldToGridPos(currentObject.transform.position)); // Or currentObject if needed
        }
    }

    public void ToggleEditMode()
    {
        isEdit = !isEdit;
    }
   
    void HighlightBlock(GameObject obj)
    {
        highlightedObject = obj;
        highlightedObject.GetComponent<Renderer>().material.color = Color.cyan;
    }

    void ResetHightlight()
    {
        if (highlightedObject == null)
            return;

        highlightedObject.GetComponent<Renderer>().material.color = Color.white;
        highlightedObject = null;
    }

}
