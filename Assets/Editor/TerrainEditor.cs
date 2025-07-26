using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeHighlighter))]
public class CubeHighlighterEditor : Editor
{
    //void OnSceneGUI()
    //{
    //    CubeHighlighter highlighter = (CubeHighlighter)target;
    //    Transform t = highlighter.transform;

    //    // Create ray from mouse in scene view
    //    Event e = Event.current;
    //    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

    //    // Define cube faces
    //    Vector3[] normals = {
    //        Vector3.up, Vector3.down, Vector3.left,
    //        Vector3.right, Vector3.forward, Vector3.back
    //    };

    //    float minDist = float.MaxValue;
    //    int hovered = -1;

    //    for (int i = 0; i < 6; i++)
    //    {
    //        Vector3 normal = t.TransformDirection(normals[i]);
    //        Vector3 center = t.position + normal * 0.5f;

    //        Plane facePlane = new Plane(normal, center);
    //        if (facePlane.Raycast(ray, out float dist))
    //        {
    //            Vector3 hitPoint = ray.GetPoint(dist);
    //            Vector3 localHit = t.InverseTransformPoint(hitPoint);

    //            // Check if hitPoint is inside the face bounds
    //            if (Mathf.Abs(localHit.x) <= 0.5f && Mathf.Abs(localHit.y) <= 0.5f && Mathf.Abs(localHit.z) <= 0.5f)
    //            {
    //                if (dist < minDist)
    //                {
    //                    minDist = dist;
    //                    hovered = i;
    //                }
    //            }
    //        }
    //    }

    //    highlighter.hoveredFace = hovered;

    //    if (hovered != -1)
    //    {
    //        Vector3 faceNormal = t.TransformDirection(normals[hovered]);
    //        Vector3 faceCenter = t.position + faceNormal * 0.5f;
    //        Vector3 size = t.localScale;

    //        // Draw a highlighted face
    //        Handles.color = Color.yellow;
    //        Vector3 up = Vector3.Cross(faceNormal, Vector3.right);
    //        if (up == Vector3.zero)
    //            up = Vector3.forward;
    //        Vector3 right = Vector3.Cross(up, faceNormal);

    //        up = up.normalized * size.y * 0.5f;
    //        right = right.normalized * size.x * 0.5f;

    //        Vector3[] quad = new Vector3[4]
    //        {
    //            faceCenter - up - right,
    //            faceCenter - up + right,
    //            faceCenter + up + right,
    //            faceCenter + up - right
    //        };

    //        Handles.DrawSolidRectangleWithOutline(quad, new Color(1, 1, 0, 0.25f), Color.yellow);

    //        // input

    //        // Detect mouse click on the face
    //        if (e.type == EventType.MouseDown && e.button == 0)
    //        {
    //            // Register for Undo
    //            Undo.RecordObject(highlighter, "Clicked Cube Face");

    //            Debug.Log($"Clicked on face {hovered} of object '{highlighter.name}'");

    //            // Do something: e.g., change color of the object or face
    //            highlighter.GetComponent<Renderer>().sharedMaterial.color = Random.ColorHSV();

    //            e.Use(); // Consume the event
    //        }
    //    }
    //}
}
