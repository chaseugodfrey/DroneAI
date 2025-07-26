using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class DroneAgent : MonoBehaviour
{
    [Header("Stats")]
    public float baseHeight;
    public float minDistance;
    public float offsetFromPlayer;
    public float minDistanceTravelThreshold;
    public float rotationSpeed;

    [Range(1f, 5f)]
    public float followTightness;
    public Transform player;


    [Header("Floating Behaviour")]
    public float maxDistance;
    public float idleAnimTimer;
    public float checkInputTimer;
    public Vector2 floatTimerMinMax;
    float inputTimer;
    float floatTimer;
    float idleTimer;

    [Header("Live Stats")]
    public float distanceToPlayer;
    public Vector3 targetPosition;
    public float distanceToPosition;
    public float cost;
    public float tightness;


    [Header("Setup")]
    public OccupancyBox occupancyBox;
    public StarterAssets.ThirdPersonController playerController;
    public GameObject exclamationMark;
    StarterAssetsInputs playerInputs;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<ThirdPersonController>();
        playerInputs = player.GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        floatTimer = floatTimerMinMax.y;
        idleTimer = idleAnimTimer;
        inputTimer = checkInputTimer;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance();
        //CheckDistance();
        FollowTarget();
        RotateToTargetForward();
    }

    void UpdateDistance()
    {
        distanceToPlayer = Vector3.Distance(player.position, transform.position);
        distanceToPosition = Vector3.Distance(transform.position, targetPosition);
    }

    public void UpdateTargetPosition()
    {
        Vector3 new_target_pos = occupancyBox.GetAveragePosition() - player.transform.forward * offsetFromPlayer;
        if (Vector3.Distance(new_target_pos, targetPosition) > minDistanceTravelThreshold)
            targetPosition = new_target_pos;
    }

    public void CheckPlayerIdle()
    {
        if (playerController._speed < playerController.MoveSpeed)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer < 0)
            {
                animator.SetBool("isFloating", true);
                idleTimer = idleAnimTimer;
            }
        }

        else
            idleTimer = idleAnimTimer;
    }

    private Vector3 velocity;

    public void FollowTarget()
    {
        float mod = playerInputs.sprint ? 0.5f : 1f;

        float limit = 10;
        limit /= followTightness / mod;
        float tight = -0.005f * Mathf.Pow((followTightness / mod * distanceToPosition - 10), 3);

        if (distanceToPlayer < minDistance)
            tight = 0.1f;
        tight = Mathf.Clamp(tight, 0, limit);

        tightness = Mathf.Lerp(tightness, tight, Time.deltaTime);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, tightness);
    }


    void QueryPath()
    {
        Vector3 start = player.position;
        List<Vector3> gridBoundingBox;

    }

    void RotateToTargetForward()
    {
        Vector3 lookPosition = player.transform.position + player.transform.forward * 3.0f;
        Vector3 direction = lookPosition - transform.position;
        direction.y = 0; // Constrain to horizontal rotation

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float angle = Quaternion.Angle(transform.rotation, targetRotation); // Arc in degrees

        // Normalize angle to [0,1] based on a max angle (e.g. 180)
        float t = Mathf.Clamp01(angle / 90f); // adjust 90f to tune responsiveness
        float speedFactor = Mathf.Lerp(0.1f, 1f, t); // optionally scale how fast it can go

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedFactor * Time.deltaTime * rotationSpeed * 2.0f);
    }

    public void Floating()
    {
        if (playerController._speed >= playerController.MoveSpeed)
        {
            inputTimer -= Time.deltaTime;
            if (inputTimer < 0)
            {
                animator.SetBool("isFloating", false);
                return;
            }
        }

        else
        {
            inputTimer = checkInputTimer;
        }

        Vector3 input = new Vector3(playerInputs.move.x, 0, playerInputs.move.y);
        input = player.transform.TransformDirection(input);
        targetPosition += input * playerController.MoveSpeed * Time.deltaTime;

        floatTimer -= Time.deltaTime;
        if (floatTimer < 0)
        {
            floatTimer = Random.Range(floatTimerMinMax.x, floatTimerMinMax.y);
            targetPosition = occupancyBox.PickRandomActiveNode();
        }
    }

    IEnumerator ExclamationCoroutine()
    {
        exclamationMark.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        exclamationMark.gameObject.SetActive(false);
    }

    public void DisplayExclamation()
    {
        StartCoroutine(ExclamationCoroutine());
    }

    public float Tightness
    {
        get { return tightness; }
        set { tightness = value; }
    }

    public float RotationSpeed
    {
        get { return rotationSpeed; }
        set { rotationSpeed = value; }
    }

    public float BaseHeight
    {
        get { return baseHeight; }
        set { baseHeight = value; }
    }

    public float OffsetFromPlayer
    {
        get { return offsetFromPlayer; }
        set { offsetFromPlayer = value; }
    }
}
