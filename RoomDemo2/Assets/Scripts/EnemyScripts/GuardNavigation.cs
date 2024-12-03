using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardNavigation : MonoBehaviour
{
    public Transform Player; // Reference to the player
    private NavMeshAgent agent;

    // State variables
    private enum EnemyState { Guarding, Searching, Chasing }
    private EnemyState currentState = EnemyState.Guarding;

    [SerializeField] public Transform guardCenter; // Center point for guarding
    [SerializeField] private float guardRadius = 20f; // Radius for guarding area
    [SerializeField] private float activationDistance = 40f; // Distance to activate searching
    [SerializeField] private float chasingDistance = 20f; // Distance to activate chasing
    [SerializeField] private float guardSpeed = 2f; // Speed while guarding
    [SerializeField] private float chaseSpeed = 3.5f; // Speed while chasing

    private bool isGuarding = false;

    public TextMeshProUGUI stateText;

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
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        switch (currentState)
        {
            case EnemyState.Guarding:
                Guard();
                if (distanceToPlayer <= activationDistance)
                {
                    currentState = EnemyState.Searching;
                    UpdateStateText();
                }
                break;

            case EnemyState.Searching:
                SearchForPlayer(distanceToPlayer);
                if (distanceToPlayer <= chasingDistance)
                {
                    currentState = EnemyState.Chasing;
                    UpdateStateText();
                }
                else if (distanceToPlayer > activationDistance)
                {
                    currentState = EnemyState.Guarding;
                    UpdateStateText();
                }
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                if (distanceToPlayer > chasingDistance)
                {
                    currentState = EnemyState.Searching;
                    UpdateStateText();
                }
                break;
        }
    }

    private void Guard()
    {
        if (!isGuarding && agent.isOnNavMesh)
        {
            isGuarding = true;
            agent.speed = guardSpeed;
            agent.isStopped = false;
            StartCoroutine(GuardCoroutine());
        }
    }

    private IEnumerator GuardCoroutine()
    {
        // Generate a random point within the guard radius
        Vector3 randomDirection = Random.insideUnitSphere * guardRadius;
        randomDirection += guardCenter.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, guardRadius, NavMesh.AllAreas);
        agent.SetDestination(hit.position);

        // Wait until the agent reaches the destination
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Pause briefly before generating a new destination
        yield return new WaitForSeconds(2f);
        isGuarding = false;
    }

    private void SearchForPlayer(float distanceToPlayer)
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;

        // Calculate a random search point within the activation radius
        Vector3 randomDirection = Random.insideUnitSphere * activationDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, activationDistance, NavMesh.AllAreas);
        agent.SetDestination(hit.position);
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(Player.position); // Chase the player
    }

    private void UpdateStateText()
    {
        switch (currentState)
        {
            case EnemyState.Guarding:
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
        }
    }
}
