using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project3Manager : MonoBehaviour
{
    public GameObject settings;
    public GameObject enemy;
    public StarterAssetsInputs inputs;

    public OccupancyBox playerBox;
    public OccupancyBox enemyBox;
    public FlowField flowField;

    bool settingsOpen;
    public void ToggleSettings()
    {
        settings.SetActive(!settings.activeSelf);
    }

    public void ToggleEnemy()
    {
        enemy.SetActive(!enemy.activeSelf);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            inputs.SetCursorState(!inputs.cursorLocked);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleSettings();

            if (settings.activeSelf)
                inputs.SetCursorState(false);

            else
                inputs.SetCursorState(true);

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            playerBox.SetDebug();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleEnemy();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            enemyBox.SetDebug();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            flowField.SetDebug();
        }
    }
}
