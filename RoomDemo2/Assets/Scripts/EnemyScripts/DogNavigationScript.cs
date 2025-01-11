using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class DogNavigationScript : MonoBehaviour, IEnemy
{
    public Transform Player; // Reference to the player
    private NavMeshAgent agent;

    public float MinPossibleHealth => 40f;
    public float MaxPossibleHealth => 60f;


    public float MinPossibleDamage => 20f; // Minimum possible damage
    public float MaxPossibleDamage => 45f;

    // State variables
    private enum EnemyState { Wandering, Searching, Chasing, Investigating }
    private EnemyState currentState = EnemyState.Wandering;

    [SerializeField] float activationDistance = 50f; // Base activation distance
    [SerializeField] float chasingDistance = 30f;    // Base chasing distance
    [SerializeField] float wanderingTime = 4f;      // Time to wander before stopping
    [SerializeField] float searchingTime = 4f;      // Time to search before stopping
    [SerializeField] float investigateSpeed = 4f; // Speed while investigating
    [SerializeField] float wanderSpeed = 3f;        // Speed while wandering
    [SerializeField] float chaseSpeed = 5f;       // Speed while chasing

    private bool isWandering = false;
    private bool isSearching = false;

    public TextMeshProUGUI stateText;


    private GameObject currentAttractionObject; // Reference to the active attraction object
    [SerializeField] float attractionCheckRadius = 80f; // Radius to check for attraction objects



    void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player").transform;
        }
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;

        UpdateStateText(); // Initialize state text at the start
    }

    private void Update()
    {
        // Check if the battle canvas is active
        if (GameManager.BattleCanvas != null && GameManager.BattleCanvas.activeSelf)
        {
            // Stop the enemy movement
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Ensure no movement
            }
            return; // Prevent further updates to enemy behavior
        }

        // Resume movement if the battle canvas is not active
        if (agent.isOnNavMesh && agent.isStopped)
        {
            agent.isStopped = false;
        }


        // Adjust distances based on player's detection multiplier
        float adjustedActivationDistance = activationDistance * PlayerMovement.DetectionMultiplier;
        float adjustedChasingDistance = chasingDistance * PlayerMovement.DetectionMultiplier;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        // Check for nearby attraction objects
        if (currentState != EnemyState.Chasing && currentState != EnemyState.Investigating)
        {
            currentAttractionObject = FindClosestAttractionObject();
            if (currentAttractionObject != null)
            {
                Debug.Log("Switching to Investigating state");
                Debug.Log($"Attraction object found: {currentAttractionObject.name}");
                currentState = EnemyState.Investigating;
                UpdateStateText();
            }
        }

        switch (currentState)
        {
            case EnemyState.Wandering:
                //Debug.Log("State: Wandering");
                Wander();
                if (distanceToPlayer <= adjustedActivationDistance)
                {
                    Debug.Log("Switching to Searching state");
                    currentState = EnemyState.Searching;
                    UpdateStateText();
                }
                break;

            case EnemyState.Searching:
                // Debug.Log("State: Searching");
                SearchForPlayer(distanceToPlayer);
                if (distanceToPlayer <= adjustedChasingDistance)
                {
                    Debug.Log("Switching to Chasing state");
                    SceneMusicController musicController = FindObjectOfType<SceneMusicController>();
                    AudioManager.Instance.PlaySoundEffect(musicController.gameoverSound);
                    currentState = EnemyState.Chasing;
                    UpdateStateText();
                }
                else if (distanceToPlayer > adjustedActivationDistance)
                {
                    Debug.Log("Switching to Wandering state");
                    currentState = EnemyState.Wandering;
                    UpdateStateText();
                }
                break;

            case EnemyState.Chasing:
                // Debug.Log("State: Chasing");
                ChasePlayer();
                if (distanceToPlayer > adjustedChasingDistance)
                {
                    Debug.Log("Switching to Searching state");
                    currentState = EnemyState.Searching;
                    UpdateStateText();
                }
                break;

            case EnemyState.Investigating:
                //Debug.Log("State: Investigating");
                Investigate();
                if (distanceToPlayer <= adjustedChasingDistance)
                {
                    Debug.Log("Switching to Chasing state");
                    currentState = EnemyState.Chasing;
                    UpdateStateText();
                }
                else if (distanceToPlayer <= adjustedActivationDistance)
                {
                    Debug.Log("Switching to Searching state");
                    currentState = EnemyState.Searching;
                    UpdateStateText();
                }
                else if (currentAttractionObject == null)
                {
                    Debug.Log("Attraction object destroyed, switching to Wandering state");
                    currentState = EnemyState.Wandering;
                    UpdateStateText();
                }
                break;
        }
    }

    public string getCurrentState()
    {
        return currentState.ToString(); // Return the current state as a string
    }

    private void Wander()
    {
        if (!isWandering && agent.isOnNavMesh)
        {
            isWandering = true;
            agent.speed = wanderSpeed;
            agent.isStopped = false;
            StartCoroutine(WanderCoroutine());
        }
    }

    private IEnumerator WanderCoroutine()
    {
        // Pick a random direction to wander in
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);
        agent.SetDestination(hit.position);

        // Wait for a few seconds before wandering again
        yield return new WaitForSeconds(wanderingTime);

        isWandering = false;
        agent.isStopped = true;
        yield return new WaitForSeconds(3f); // Stop for 3 seconds
    }

    private void SearchForPlayer(float distanceToPlayer)
    {
        if (!agent.isOnNavMesh) return;

        if (!isSearching)
        {
            isSearching = true;
            agent.isStopped = false;

            // Calculate direction towards player
            Vector3 directionToPlayer = (Player.position - transform.position).normalized;

            // Apply a random angle to simulate searching
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 rotatedDirection = Quaternion.Euler(0f, randomAngle, 0f) * directionToPlayer;

            // Move in the new direction for searching
            agent.SetDestination(transform.position + rotatedDirection * 5f);

            // Wait for searching time before re-evaluating
            StartCoroutine(SearchCoroutine());
        }
    }

    private IEnumerator SearchCoroutine()
    {
        yield return new WaitForSeconds(searchingTime);

        // After searching, stop for a brief moment
        agent.isStopped = true;
        yield return new WaitForSeconds(3f);

        isSearching = false;
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(Player.position); // Chase the player
    }

    private void Investigate()
    {
        if (currentAttractionObject == null)
        {
            currentState = EnemyState.Wandering;
            UpdateStateText();
            return;
        }

        agent.isStopped = false;
        agent.speed = investigateSpeed;
        agent.SetDestination(currentAttractionObject.transform.position);

        if (Vector3.Distance(transform.position, currentAttractionObject.transform.position) < 1f)
        {
            Destroy(currentAttractionObject); // Remove the attraction object after reaching it
            currentAttractionObject = null;
            currentState = EnemyState.Wandering; // Return to wandering after investigation
            UpdateStateText();
        }
    }

    private GameObject FindClosestAttractionObject()
    {
        //Debug.Log("Checking for attraction objects...");
        Collider[] colliders = Physics.OverlapSphere(transform.position, attractionCheckRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("AttractionObject"))
            {
                Debug.Log($"Attraction object detected: {collider.gameObject.name}");
                return collider.gameObject;
            }
        }
        //Debug.Log("No attraction objects found.");
        return null;
    }

    private void UpdateStateText()
    {
        switch (currentState)
        {
            case EnemyState.Wandering:
                stateText.text = "";
                stateText.color = Color.white;
                break;

            case EnemyState.Searching:
                stateText.text = "?";
                stateText.color = Color.yellow;
                break;

            case EnemyState.Chasing:
                stateText.text = "!";
                stateText.color = Color.red;
                break;

            case EnemyState.Investigating:
                stateText.text = "?";
                stateText.color = Color.yellow;
                break;
        }
    }
}
