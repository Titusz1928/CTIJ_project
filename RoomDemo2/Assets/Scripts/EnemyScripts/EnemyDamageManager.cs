using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageManager : MonoBehaviour
{

    [SerializeField] private float minDamage = 20f; // Minimum possible damage
    [SerializeField] private float maxDamage = 100f; // Maximum possible damage

    // Start is called before the first frame update
    void Start()
    {
        IEnemy enemy = GetComponent<IEnemy>();
        if (enemy != null)
        {
            minDamage = enemy.MinPossibleDamage;
            // Randomize health within the enemy's defined range
            maxDamage = Random.Range(enemy.MinPossibleDamage, enemy.MaxPossibleDamage);
        }
        else
        {
            Debug.LogWarning("No IEnemy implementation found on this object.");
        }
    }

    public float getMinDamage => minDamage;

    public float getMaxDamage => maxDamage;

    // Update is called once per frame
    void Update()
    {
        
    }
}
