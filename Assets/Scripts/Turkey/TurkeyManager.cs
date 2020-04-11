using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurkeyManager : MonoBehaviour {

    public GameObject turkeyPrefab;
    public Transform turkeySpawn;
    public GameObject[] turkeys = new GameObject[5];
    // Use this for initialization
    void Start () {
		for (int i = 0; i < 5; i++)
        {
            turkeys[i] = (GameObject)Instantiate(turkeyPrefab, turkeySpawn.position + new Vector3(i * Random.Range(0f, 1f), 0f, 0f), turkeySpawn.rotation);
        }
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < 5; i++)
        {
            if (turkeys[i] == null) turkeys[i] = (GameObject)Instantiate(turkeyPrefab, turkeySpawn.position + new Vector3(i * Random.Range(0f, 1f), 0f, 0f), turkeySpawn.rotation);
        }
    }
}
