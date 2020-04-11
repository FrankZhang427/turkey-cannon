using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {
    // terrain information 
    Vector2[] vertices = new Vector2[]
    {
        new Vector2(-4f, -5f),    // bottom left
        new Vector2(-0.5f, 0f),   // top left
        new Vector2(0.5f, 0f),    // top right
        new Vector2(4f, -5f)      // bottom right
    };
    Vector2[] tangents = new Vector2[3];
    Vector2[] normals = new Vector2[3];
    float[] dist = new float[3];
    // scene information 
    public Vector2 dim = new Vector2(11, 5);

    // current position p, current velocity v
    public Vector2 p;
    public Vector2 v;
    // initial speed (1D)
    public float initSpeed = 13f;

    // other constants
    public float gravity = -9.8f;
    public float mass = 1.0f;
    public float radius = 0.5f;
    public float restitution = 0.7f;
    private float eps = 0.01f;
    // barrel angle theta
    public float theta;

    public Wind wind;

    TurkeyManager turkeyManager;
    GameObject[] turkeys;
    // Use this for initialization
    void Start () {
		wind = GameObject.FindGameObjectWithTag("Environment").GetComponent<Wind>();
        theta = GameObject.FindGameObjectWithTag("Barrel").GetComponent<Cannon>().cur_angle;
        v = new Vector2(-initSpeed * Mathf.Cos(Mathf.Deg2Rad * (90 - theta)), initSpeed * Mathf.Sin(Mathf.Deg2Rad * (90 - theta)));
        dist[0] = Vector2.Distance(vertices[0], vertices[1]);
        dist[1] = Vector2.Distance(vertices[1], vertices[2]);
        dist[2] = Vector2.Distance(vertices[2], vertices[3]);
        for (int i = 0; i < 3; i++)
        {
            dist[i] = Vector2.Distance(vertices[i], vertices[i + 1]);
            tangents[i] = vertices[i + 1] - vertices[i];
            tangents[i].Normalize();
            normals[i] = new Vector2(-tangents[i].y, tangents[i].x);
        }
        turkeyManager = GameObject.Find("TurkeyManager").GetComponent<TurkeyManager>();
        turkeys = turkeyManager.turkeys;
    }
	
	// Update is called once per frame
	void Update () {

        p = gameObject.transform.position;
        // destroy cannon balls that out of bound
        if (p.x > dim.x || p.x < -dim.x || p.y < -dim.y || v.magnitude < eps)
        {
            Destroy(gameObject);
        }

        // Wall collision
        if (p.x < -dim.x + 0.5f + radius)
        {
            v.x -= (1 + restitution) * v.x;
            gameObject.transform.Translate(v * Time.deltaTime, Space.World);
            p = gameObject.transform.position;
        }

        // Mountain collision 
        // left and right mountain cliff
        float d1d2;
        for (int i = 0; i < 3; i++)
        {
            d1d2 = Vector2.Distance(p, vertices[i]) + Vector2.Distance(p, vertices[i + 1]);
            if (d1d2 <= dist[i] + eps && d1d2 >= dist[i] - eps)
            {
                v -= (1 + restitution) * Vector2.Dot(v, normals[i]) * normals[i];
                gameObject.transform.Translate(v * Time.deltaTime, Space.World);
                p = gameObject.transform.position;
            }
        }

        // apply gravity
        v.y += gravity * Time.deltaTime;
        // apply wind 
        if (p.y > 0.01f) v.x += wind.windForce / mass * Time.deltaTime;

        gameObject.transform.Translate(v * Time.deltaTime, Space.World);
        p = gameObject.transform.position;
        // check collision with turkeys
        for (int i = 0; i < turkeys.Length; i++)
        {
            GameObject obj = turkeys[i];
            Turkey turkey = obj.GetComponent<Turkey>();
            Vector2 turkeyPosi;
            for (int j = 0; j < 8; j++)
            {
                turkeyPosi = new Vector2(turkey.points[j].position.x, turkey.points[j].position.y);
                if (Vector2.Distance(p, turkeyPosi) <= radius)
                {
                    turkey.CollideWillBall(v);
                    Destroy(gameObject);
                    break;
                }
            }
            turkeyPosi = new Vector2(turkey.points[18].position.x, turkey.points[18].position.y);
            if (Vector2.Distance(p, turkeyPosi) <= radius)
            {
                turkey.CollideWillBall(v);
                Destroy(gameObject);
                break;
            }
        }
    }
}
