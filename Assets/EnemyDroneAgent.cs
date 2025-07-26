using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDroneAgent : MonoBehaviour
{
    [Header("Settings")]
    public bool isTestMode;
    public float maxDistanceFromPlayer;
    public float minDistanceToTarget;
    public float moveSpeed;
    public Vector2 restTimeMinMax;
    public LayerMask layers;
    float restTimer;

    bool turnOnTestMode;

    [Header("Live Stats")]
    public bool isMoving;
    public Vector3 targetPosition;
    public float distanceFromPlayer;
    public float distance3D;
    public float distance2D;

    [Header("Setup")]
    public Transform player;
    public FlowField flowField;

    Vector3 direction;
    Rigidbody rb;
    bool testDir;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (isTestMode)
            TurnOnTestMode();
        else
            ChooseRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance();
        if (isTestMode)
        {
            TurnOnTestMode();
        }

        else
        {
            TurnOffTestMode();
            LookAtPlayer();
        }

        if (isMoving)
            MoveToTarget();
        else
            Rest();
    }

    void TurnOnTestMode()
    {
        if (turnOnTestMode)
            return;

        ChooseTestPosition();
        turnOnTestMode = true;
    }

    void TurnOffTestMode()
    {
        if (!turnOnTestMode)
            return;

        ChooseRandomPosition();
        turnOnTestMode = false;
    }

    void LookAtPlayer()
    {
        transform.LookAt(player.position + Vector3.up, Vector3.up);
    }

    void UpdateDistance()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        distance3D = Vector3.Distance(transform.position, targetPosition);
        distance2D = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPosition.x, targetPosition.z));
    }

    void ChooseRandomPosition()
    {
        float x = Random.Range(-1.0f, 1.0f);
        float y = Random.Range(0.0f, 1.0f);
        float z = Random.Range(-1.0f, 1.0f);

        Vector3 pos = new Vector3(x, y, z);
        pos.Normalize();
        pos *= Random.Range(5.0f, maxDistanceFromPlayer);

        Vector3 dir = pos - transform.position;
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, layers))
        {
            if (hit.collider != null)
            {
                pos = hit.point - dir.normalized;
            }
        }

        isMoving = true;
        targetPosition = pos;
    }

    void MoveToTarget()
    {
        Vector3 dir = (targetPosition - transform.position);
        flowField.CalculateFlowField(dir);
        direction = flowField.CalculateAverageDirection();
        rb.velocity = moveSpeed * direction;

        if (distance3D <= minDistanceToTarget)
        {
            StartRest();
        }
    }

    void StartRest()
    {
        isMoving = false;
        rb.velocity = Vector3.zero;
        restTimer = Random.Range(restTimeMinMax.x, restTimeMinMax.y);
    }

    void Rest()
    {
        restTimer -= Time.deltaTime;
        if (restTimer < 0)
        {
            if (!isTestMode)
                ChooseRandomPosition();
            else
                ChooseTestPosition();
        }
    }

    void ChooseTestPosition()
    {
        Vector3[] positions = { Vector3.zero, Vector3.right * 20.0f };

        if (testDir)
            targetPosition = positions[0];
        else
            targetPosition = positions[1];

        testDir = !testDir;

        isMoving = true;
    }

    public float StoppingDistance
    {
        get { return minDistanceToTarget; }
        set { minDistanceToTarget = value; }
    }

    public float MaxPickDistance
    {
        get { return maxDistanceFromPlayer; }
        set { maxDistanceFromPlayer = value; }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }
}
