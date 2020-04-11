using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {

    public GameObject cannonBallPrefab;
    public Transform cannonBallSpawn;
    public int cur_angle = 45;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // since we fliped the x-axis, the up and down are opposite (i.e. 0 degree for horizontal and 90 for vertical)
        if (Input.GetKey(KeyCode.DownArrow) && cur_angle <= 89) transform.localRotation = Quaternion.Euler(0, 0, ++cur_angle);

        if (Input.GetKey(KeyCode.UpArrow) && cur_angle >= 1) transform.localRotation = Quaternion.Euler(0, 0, --cur_angle);

        // control the fire by Space
        if (Input.GetKeyDown(KeyCode.Space)) Fire();
    }

    // method to fire a cannon ball
    void Fire()
    {
        // Create the cannon ball from the cannon ball Prefab
        var bullet = (GameObject)Instantiate(
           cannonBallPrefab,
           cannonBallSpawn.position,
           cannonBallSpawn.rotation);
    }
}
