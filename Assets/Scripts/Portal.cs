using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    GameSystem gameSystem;

    GameObject caveCamera;
    Transform cameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        caveCamera = GameObject.Find("Cave Camera");
        gameSystem = FindObjectOfType<GameSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        cameraTransform = caveCamera.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.CompareTag("Exit"))
            {
                Debug.Log(other.gameObject.transform.position);
                other.gameObject.transform.position = gameSystem.entrancePortal.transform.position + new Vector3(10,10,10);
                Debug.Log(other.gameObject.transform.position);
            } else
            {
                Debug.Log(other.gameObject.transform.position);
                gameSystem.entrancePortal = gameObject;
                other.gameObject.transform.position = cameraTransform.transform.position;
                Debug.Log(other.gameObject.transform.position);
            }
            
        }
    }
}
