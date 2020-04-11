using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour {
    /**
     * Source: https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
     */
    public float windForce = 0f;
    public float maxForce = 3f;
	// Use this for initialization
	void Start () {
        windForce = 0;
        InvokeRepeating("WindUpdate", 0f, 0.5f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void WindUpdate()
    {
        windForce = Random.Range(- maxForce, maxForce);
    }
}
