using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }
    
    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadScene(0);
        }

        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadScene(1);
        }

        else if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadScene(2);
        }
    }

    void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
