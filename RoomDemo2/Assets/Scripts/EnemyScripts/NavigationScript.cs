using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NavigationScript : MonoBehaviour
{
    public Transform Player;  // Reference to the player
    private NavMeshAgent agent;

    // State variables
    private enum EnemyState { Wandering, Searching, Chasing }
    private EnemyState currentState = EnemyState.Wandering;

    [SerializeField] float activationDistance = 20f;  // Distance within which the agent will start searching for the player
    [SerializeField] float chasingDistance = 10f;     // Distance within which the agent will chase the player
    [SerializeField] float wanderingTime = 5f;       // Time to wander before stopping (set to 5 seconds for demo)
    [SerializeField] float searchingTime = 5f;       // Time to search before stopping (set to 5 seconds for demo)
    [SerializeField] float wanderSpeed = 2f;         // Speed while wandering
    [SerializeField] float chaseSpeed = 3.5f;        // Speed while chasing

    private bool isWandering = false;
    private bool isSearching = false;

    public TextMeshProUGUI stateText;

    void Start() { 

        if (Player == null){
            Player = GameObject.FindWithTag("Player").transform;
        }
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;


        UpdateStateText();  // Initialize state text at the start
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        switch (currentState)
        {
            case EnemyState.Wandering:
                Wander();
                if (distanceToPlayer <= activationDistance)
                {
                    Debug.Log("searching...");
                    currentState = EnemyState.Searching;  // Switch to searching if the player is close enough
                    UpdateStateText();  // Update state display
                }
                break;

            case EnemyState.Searching:
                SearchForPlayer(distanceToPlayer);
                if (distanceToPlayer <= chasingDistance)
                {
                    Debug.Log("chasing...");
                    currentState = EnemyState.Chasing;  // Switch to chasing if the player is close enough
                    UpdateStateText();  // Update state display
                }
                else if (distanceToPlayer > activationDistance)
                {
                    Debug.Log("wandering...");
                    currentState = EnemyState.Wandering;  // Go back to wandering if the player is far enough
                    UpdateStateText();  // Update state display
                }
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                if (distanceToPlayer > chasingDistance)
                {
                    currentState = EnemyState.Searching;  // Go back to searching if the player moves far enough
                    UpdateStateText();  // Update state display
                }
                break;
        }
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

        // Wait for a few seconds before wandering again based on the wanderingTime value
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

            // Apply a random angle to the direction to simulate searching
            float randomAngle = Random.Range(-45f, 45f); // Random angle between -45 and 45 degrees
            Vector3 rotatedDirection = Quaternion.Euler(0f, randomAngle, 0f) * directionToPlayer;

            // Move in the new direction for searching
            agent.SetDestination(transform.position + rotatedDirection * 5f); // Move towards the rotated direction

            // Wait for the specified searchingTime before stopping and re-evaluating
            StartCoroutine(SearchCoroutine());
        }
    }

    private IEnumerator SearchCoroutine()
    {
        // Wait for the specified searching time before deciding to move again
        yield return new WaitForSeconds(searchingTime);

        // After searching, stop and wait for a brief moment (e.g., 3 seconds)
        agent.isStopped = true;
        yield return new WaitForSeconds(3f); // Stop for 3 seconds

        // After stopping, decide whether to change direction or continue searching
        if (Random.value < 0.5f)
        {
            // Change direction randomly
            Debug.Log("changing direction");
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
        }
        else
        {
            // Otherwise, continue searching in the same direction
            SearchForPlayer(Vector3.Distance(transform.position, Player.position));
        }

        // Allow the agent to move again after waiting
        isSearching = false;
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(Player.position);  // Chase the player
    }

    private void UpdateStateText()
    {
        switch (currentState)
        {
            case EnemyState.Wandering:
                stateText.text = "Wandering";
                break;
            case EnemyState.Searching:
                stateText.text = "Searching";
                break;
            case EnemyState.Chasing:
                stateText.text = "Chasing";
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision detected with player.");

            // Get the SceneStateManager instance and save the current scene state
            SceneStateManager sceneManager = SceneStateManager.Instance;

            // Collect all enemy transforms
            List<Transform> enemies = new List<Transform>();
            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemies.Add(enemy.transform);
            }

            // Save the state (player position and enemy positions) to the singleton
            sceneManager.currentSceneState.SaveState(collision.transform, enemies);

            // Log the saved data for verification
            Debug.Log("Saved Player Position: " + sceneManager.currentSceneState.playerPosition);
            for (int i = 0; i < sceneManager.currentSceneState.enemyPositions.Count; i++)
            {
                Debug.Log("Saved Enemy " + i + " Position: " + sceneManager.currentSceneState.enemyPositions[i]);
            }

            // Load the battle scene
            SceneManager.LoadScene("BattleScene"); // Assuming your battle scene is called "BattleScene"

/*            // After loading, load the state from the singleton (just for testing here)
            sceneManager.currentSceneState.LoadState(collision.transform, enemies);

            // Log the loaded data for verification
            Debug.Log("Loaded Player Position: " + collision.transform.position);
            for (int i = 0; i < enemies.Count; i++)
            {
                Debug.Log("Loaded Enemy " + i + " Position: " + enemies[i].position);
            }*/

        }
    }


}
