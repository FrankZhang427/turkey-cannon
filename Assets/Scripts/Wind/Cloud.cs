using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour {

    Wind wind;
	// Use this for initialization
	void Start () {
		wind = GameObject.FindGameObjectWithTag("Environment").GetComponent<Wind>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 v = new Vector2(wind.windForce / 2.0f * Time.deltaTime , 0);
        gameObject.transform.Translate(v);
	}
}
