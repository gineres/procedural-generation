using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectItem : MonoBehaviour
{
    GameSystem gameSystem;

    void Start()
    {
        gameSystem = FindObjectOfType<GameSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameSystem.AddItem(gameObject.transform.position, gameObject, true);
            Destroy(gameObject);
        }
    }
}
