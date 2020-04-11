using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turkey : MonoBehaviour
{

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
    float slope = 5 / 3.5f;
    // scene information 
    public Vector2 dim = new Vector2(11, 5);

    // Environemnt
    Wind wind;

    // some cosntants
    public float eps = 0.01f;
    public float gravity = -0.03f; // somehow normal gravity doesnt work
    public float initVelocity = -10f;
    public int initDirection = 1;
    public float restitution = 0.75f;

    // data used for verlet integration
    public Transform head;
    public Transform eye;
    public Transform body;
    public Transform legs;

    public Transform[] points;
    public LineRenderer[] lines;
    public float[] lineConstraints;
    public float[] angleConstraints;

    float orientation; // turkey orientation(angle) wrt world (may not be usefull)

    // old position and current position
    Vector3[] p;
    Vector3[] oldP;
    // placeholder for intermediate calculations in verlet
    float[] vx;
    float[] vy;
    Vector2 v; // for external force or speed gain from collision

    // bounding sphere (may not be usefull)
    float radius = 0.5f;
    int direction = 1;
    // some flags
    public bool reset = true; // reset to initial veloctiy
    public bool freeze = false;// freeze update if needed
    public bool flying = false; // turkey is not on the ground
    public bool rightSide = false; // turkey on the right side of the mountain
    public bool onTop = false; // turkey on the top of the mountain
    public bool leftSide = false; // turkey on the left side of the mountain

    // Use this for initialization
    void Start()
    {
        wind = GameObject.FindGameObjectWithTag("Environment").GetComponent<Wind>();
        // v = new Vector2(initVelocity, 0f);
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
        head = transform.Find("Head");
        eye = transform.Find("Eye");
        body = transform.Find("Body");
        legs = transform.Find("Legs");
        points = new Transform[19];
        p = new Vector3[19];
        oldP = new Vector3[19];
        vx = new float[19];
        vy = new float[19];
        v = new Vector2(0, 0);
        lines = new LineRenderer[18];
        lineConstraints = new float[19];
        angleConstraints = new float[19];
        
        InitTurkey();
        if (Random.Range(0f, 1f) > 0.5f)
        {
            initDirection = -1;
            TurnTurkey(-1);
        }
        InvokeRepeating("Leap", Random.Range(1f, 10f), 10f);
    }

    // Update is called once per frame
    void Update()
    {
        if (points[2].position.y > -dim.y + 0.75 && (points[2].position.x > 0.5f || points[2].position.x < -0.5f))
        {
            freeze = false;
            flying = true;
        }
        // terrain constrains on the right
        if (points[11].position.x > dim.x) Destroy(gameObject);
        if (!freeze)
        {
            MoveTurkey();
            // hit left wall with head
            if (points[1].position.x  < -dim.x + 0.5f)
            {
                v = new Vector2(0, 0);
                direction = -1;
                reset = true;
                TurnTurkey(-1);
                MoveTurkey();
            }
            // hit left wall with back
            if (points[10].position.x <= -dim.x + 0.5f || points[9].position.x <= -dim.x + 0.5f)
            {
                reset = true;
                direction = -1;
                v = new Vector2(0, 0);
            }
            // hit left mountain
            if (!flying && points[1].position.x > vertices[0].x  && points[1].position.x < vertices[0].x)
            {
                v = new Vector2(0, 0);
                direction = 1;
                reset = true;
                TurnTurkey(1);
                MoveTurkey();
            }
            // hit right mountain
            if (!flying && points[1].position.x > vertices[3].x  -1 && points[1].position.x < vertices[3].x  )
            {
                v = new Vector2(0, 0);
                direction = -1;
                reset = true;
                TurnTurkey(-1);
                MoveTurkey();
            }
            // hit ground 
            if (flying && vy[18] <= 0 && points[2].position.y <= -dim.y + 1.5 )
            {
                flying = false;
                reset = true;
                v = new Vector2(0, 0);
            }

            Constraints();
        }

        // resolve the cases on the mountain

        // left side of the mountain
        if (Vector2.Distance(points[12].position, vertices[1]) + Vector2.Distance(points[12].position, vertices[0]) <= Vector2.Distance(vertices[1], vertices[0]) + eps)
        {
            freeze = true;
            leftSide = true;
            flying = false;
            if (direction == -1) TurnTurkey(1);
        }

        // if on the left side
        if (leftSide)
        {
            v = new Vector2(0, 0);
            for (int i = 0; i < 19; i++)
            {
                vx[i] = direction * initVelocity * Time.deltaTime / 20;
                vy[i] = vx[i] * slope;
                points[i].position = new Vector3(points[i].position.x + vx[i], points[i].position.y + vy[i], 0);
            }
            // get back to the ground
            if (!flying && points[15].position.y <= 1)
            {
                leftSide = false;
                freeze = false;
                reset = true;
                flying = false;
                for (int i = 0; i < 19; i++)
                {
                    p[i].x = points[i].position.x;
                    p[i].y = points[i].position.y;
                }
            }
        }

        // on the top of the mountain
        if (Vector2.Distance(points[17].position, vertices[1]) + Vector2.Distance(points[17].position, vertices[2]) <= Vector2.Distance(vertices[1], vertices[2]) + eps)
        {
            freeze = true;
            onTop = true;
            flying = false;
            if (direction == 1) TurnTurkey(-1);
        }

        if (onTop)
        {

            for (int i = 0; i < 19; i++)
            {
                vx[i] = direction * initVelocity * Time.deltaTime / 20;
                points[i].position = new Vector3(points[i].position.x + vx[i], points[i].position.y, 0);
            }
            if (!flying && points[15].position.x > vertices[2].x)
            {
                rightSide = true;
                onTop = false;
                flying = true;
                freeze = false;
            }
            if (!flying && points[15].position.x < vertices[1].x)
            {
                leftSide = true;
                onTop = false;
                flying = true;
                freeze = false;
            }
        }
        // right side of the mountain
        if (Vector2.Distance(points[10].position, vertices[2]) + Vector2.Distance(points[10].position, vertices[3]) <= Vector2.Distance(vertices[3], vertices[2]) + eps)
        {
            freeze = true;
            rightSide = true;
            flying = false;
            if (direction == 1)
            {
                TurnTurkey(-1);
            }
        }

        if (rightSide)
        {
            for (int i = 0; i < 19; i++)
            {
                vx[i] = direction * initVelocity * Time.deltaTime / 20;
                vy[i] = -vx[i] * slope;
                points[i].position = new Vector3(points[i].position.x + vx[i], points[i].position.y + vy[i], 0);
            }
            if (points[15].position.y <= 1)
            {
                rightSide = false;
                freeze = false;
                reset = true;
                flying = false;
                for (int i = 0; i < 19; i++)
                {
                    p[i].x = points[i].position.x;
                    p[i].y = points[i].position.y;
                }
            }
        }

        RenderTurkey();
    }

    void InitTurkey()
    {
        direction = initDirection;
        // initialize points
        for(int i = 0; i < 19; i++)
        {
            if (i < 8) points[i] = head.Find("Point" + i);
            else if (i < 14) points[i] = body.Find("Point" + i);
            else if (i < 18) points[i] = legs.Find("Point" + i);
            else points[i] = eye;
            p[i] = points[i].position;
            oldP[i] = points[i].position;
        }


        // initialize lines that rendered on screen
        for (int i = 0; i < 18; i++)
        {
            lines[i] = points[i].gameObject.AddComponent<LineRenderer>();
            lines[i].SetPosition(0, points[i].position);
            if (i < 13) lines[i].SetPosition(1, points[i + 1].position);
            else if (i == 13) lines[i].SetPosition(1, points[0].position);
            else if (i == 14) lines[i].SetPosition(1, points[11].position);
            else if (i == 15) lines[i].SetPosition(1, points[14].position);
            else if (i == 16) lines[i].SetPosition(1, points[12].position);
            else if (i == 17) lines[i].SetPosition(1, points[16].position);
            lines[i].startWidth = 0.02f;
            lines[i].endWidth = 0.02f;
            lines[i].startColor = Color.black;
            lines[i].endColor = Color.black;
            lines[i].positionCount = 2;
            lines[i].numCapVertices = 1;

            // set length constraints
            lineConstraints[i] = Vector3.Distance(lines[i].GetPosition(0), lines[i].GetPosition(1));
        }
        lineConstraints[18] = (points[18].position - points[5].position).magnitude;
        for (int i = 0; i < 18; i++)
        {
            if (i == 0) angleConstraints[i] = Vector3.Angle(points[13].position - points[0].position, points[1].position - points[0].position);
            else if (i < 13) angleConstraints[i] = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            else if (i == 13) angleConstraints[i] = Vector3.Angle(points[12].position - points[13].position, points[0].position - points[13].position);
            else if (i == 14) angleConstraints[i] = 80f;
            else if (i == 16) angleConstraints[i] = 80f;
            else angleConstraints[i] = 160f;
        }
        angleConstraints[18] = 90f;
    }
    void RenderTurkey()
    {
        // update lines that rendered on screen
        for (int i = 0; i < 18; i++)
        {
            lines[i].SetPosition(0, points[i].position);
            if (i < 13) lines[i].SetPosition(1, points[i + 1].position);
            else if (i == 13) lines[i].SetPosition(1, points[0].position);
            else if (i == 14) lines[i].SetPosition(1, points[11].position);
            else if (i == 15) lines[i].SetPosition(1, points[14].position);
            else if (i == 16) lines[i].SetPosition(1, points[12].position);
            else if (i == 17) lines[i].SetPosition(1, points[16].position);
            lines[i].startWidth = 0.025f;
            lines[i].endWidth = 0.052f;
            lines[i].startColor = Color.red;
            lines[i].endColor = Color.red;
            lines[i].material.color = Color.red;
            lines[i].positionCount = 2;
            lines[i].numCapVertices = 1;

        }
    }

    void Leap()
    {
        // leap upward
        flying = true;
        reset = true;
        for (int i = 0; i < 9; i++)
        {
            vy[i] = - 0.9f * initVelocity * Time.deltaTime;
        }
        vy[18] = -0.9f * initVelocity * Time.deltaTime;
    }

    void MoveTurkey()
    {
        // move the turkey using verlet integration

        float windForce = (eye.position.y > 0f + eps) ? wind.windForce * 0.04f : 0.0f;
        for (int i = 0; i < 19; i++)
        {
            if (i > 7 && i < 18) continue;
            if (reset)
            {
                vx[i] = direction * initVelocity * Time.deltaTime * 0.1f;
                if (!flying) vy[i] = 0.0f;
            }
            else
            {
                vx[i] = points[i].position.x - oldP[i].x + v.x;
                vy[i] = points[i].position.y - oldP[i].y + v.y;
            }
            p[i].x = points[i].position.x + vx[i] + windForce * Time.deltaTime;
            if (flying)
            {
                vy[i] = vy[i] + gravity * Time.deltaTime * 5;
                p[i].y = points[i].position.y + vy[i];
            }
            oldP[i].x = points[i].position.x;
            oldP[i].y = points[i].position.y;
            points[i].position = new Vector3(p[i].x, p[i].y, 0);
        }
        reset = false;
    }

    void Constraints()
    {
        // length constraints between connected points
        LenConstraint(8, 7);
        LenConstraint(9, 8);
        LenConstraint(13, 0);
        LenConstraint(12, 13);
        LenConstraint(11, 12);
        LenConstraint(10, 11);
        LenConstraint(9, 10);
        LenConstraint(8, 9);
        LenConstraint(14, 11);
        LenConstraint(15, 14);
        LenConstraint(16, 12);
        LenConstraint(17, 16);

        //// angle constraints between 2 adjacent lines
        AngleConstraint(7, 6, 7, 8, 8, -1);
        AngleConstraint(8, 7, 8, 9, 9, 1);
        AngleConstraint(9, 8, 9, 10, 10, 1);
        AngleConstraint(0, 13, 0, 1, 13, -1);
        AngleConstraint(13, 12, 13, 0, 12, -1);
        AngleConstraint(12, 11, 12, 13, 11, -1);
        AngleConstraint(11, 10, 11, 12, 10, -1);
        AngleConstraint(10, 9, 10, 11, 9, -1);
        AngleConstraint(9, 8, 9, 10, 8, -1);
        AngleConstraint(14, 14, 11, 12, 14, 1);
        AngleConstraint(15, 15, 14, 11, 15, 1);
        AngleConstraint(16, 16, 12, 11, 16, -1);
        AngleConstraint(17, 17, 16, 12, 17, 1);

    }
    

    void LenConstraint(int index0, int index1)
    {
        float max = lineConstraints[index0] * (1 + eps /2);
        float min = lineConstraints[index0] * (1 - eps /2);
        float distance = Vector3.Distance(points[index1].position, points[index0].position);
        if (distance < min)
        {
            while (distance < min)
            {
                points[index0].position = Vector3.MoveTowards(points[index0].position, points[index1].position, -0.01f);
                distance = Vector3.Distance(points[index1].position, points[index0].position);
            }
        }
        if (distance > max)
        {
            while (distance > max)
            {
                points[index0].position = Vector3.MoveTowards(points[index0].position, points[index1].position, 0.01f);
                distance = Vector3.Distance(points[index1].position, points[index0].position);
            }
        }
    }

    void AngleConstraint(int at, int a, int b, int c, int move, int dir)
    {
        float max = angleConstraints[at] * (1 + eps / 2);
        float min = angleConstraints[at] * (1 - eps / 2);
        float angle = Vector3.Angle(points[a].position - points[b].position, points[c].position - points[b].position);
        if (angle < min)
        {
            while (angle < min)
            {
                points[move].RotateAround(points[b].position, new Vector3(0, 0, dir * direction), 1f);
                angle = Vector3.Angle(points[a].position - points[b].position, points[c].position - points[b].position);
            }
        }
        if (angle > max)
        {
            while (angle > max)
            {
                points[move].RotateAround(points[b].position, new Vector3(0, 0, -dir * direction), 1f);
                angle = Vector3.Angle(points[a].position - points[b].position, points[c].position - points[b].position);
            }
        }
    }
    void TurnTurkey(int dir)
    {
        for (int i = 0; i < 19; i++)
        {
            points[i].position = new Vector3(points[i].position.x + 2 * (points[12].position.x - points[i].position.x), points[i].position.y, 0); ;
        }
        reset = true;
        direction = dir;
    }

    public void CollideWillBall(Vector2 velocity)
    {
        v.x = velocity.x * 0.000111f ;
        v.y = velocity.y * 0.000111f ;
    }
}
