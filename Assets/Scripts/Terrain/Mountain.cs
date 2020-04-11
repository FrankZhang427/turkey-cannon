using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/**
 * Based on the idea from https://www.youtube.com/watch?v=gmuHI_wsOgI
 */
public class Mountain : MonoBehaviour {
    public Mesh mesh;
    public Material material;
    public float proportion = 10f; // displacement proportional to the line segment length
    public int num_rec = 3; // number of times besection happens on one side
    // Use this for initialization
    void Start () {
        mesh = new Mesh();

        // 4 starting vertices + 14 bisection vertices (7 on each side) i.e. bisection 3 times
        // next bisection iteration would be 15 bisection vertices on each side
        
        int num_bisection = IntPow(2, num_rec) - 1 ;
        int N = 4 + 2 * num_bisection;
        Vector2[] vertices = new Vector2[N];
        Vector2[] uv = new Vector2[N];

        // starting vertices
        vertices[0] = new Vector2(-4f, -5f);
        vertices[num_bisection + 1] = new Vector2(-0.5f, 0f);
        vertices[num_bisection + 2] = new Vector2(0.5f, 0f);
        vertices[N - 1] = new Vector2(4f, -5f);

        // midpoint bisection
        Vector2[] left = Bisection(vertices[0], vertices[num_bisection + 1], num_rec);
        Vector2[] right = Bisection(vertices[num_bisection + 2], vertices[N - 1], num_rec);

        for (int i = 0; i < num_bisection; i++)
        {
            vertices[i + 1] = left[i];
            vertices[num_bisection + 3 + i] = right[i];
        }
        for (int i = 0; i < N; i++)
        {
            uv[i] = vertices[i];
        }
        
        // get the triangles
        Triangulator triangulator = new Triangulator(vertices);
        int[] triangles = triangulator.Triangulate();

        Vector3[] mesh_vertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            mesh_vertices[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
        }

        // work with mesh
        mesh.Clear();
        mesh.vertices = mesh_vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GameObject othergameObject = new GameObject("Mountain", typeof(MeshFilter), typeof(MeshRenderer));
        othergameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        othergameObject.transform.parent = gameObject.transform;
        othergameObject.GetComponent<MeshFilter>().mesh = mesh;
        othergameObject.GetComponent<MeshRenderer>().material = material;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // recursive midpoint besection
    private Vector2[] Bisection(Vector2 v0, Vector2 v1, int n)
    {
        Vector2[] vertices = new Vector2[IntPow(2, n) - 1];
        Vector2 v2 = BisectionHelper(v0, v1);
        int mid = (IntPow(2, n) - 1) / 2;
        vertices[mid] = v2;
        n--;
        if (n > 0)
        {
            Vector2[] left = Bisection(v0, v2, n);
            Vector2[] right = Bisection(v2, v1, n);
            for (int i = 0; i < IntPow(2, n) - 1; i++)
            {
                vertices[i] = left[i];
                vertices[mid + 1 + i] = right[i];
            }
        }
        return vertices;
    }
    private Vector2 BisectionHelper(Vector2 v0, Vector2 v1)
    {
        Vector2 l = v1 - v0;
        float len = l.magnitude;
        l.Normalize();
        Vector2 normal;
        if (Random.Range(0, 1) < 0.5) normal = new Vector2(l.y, -l.x); // clockwise rotate 90
        else normal = new Vector2(-l.y, l.x);                          // c.c.w rotate 90
        float displacement = Random.Range(0.01f, len / proportion);
        Vector2 midpoint = (v0 + v1) / 2.0f;
        midpoint += displacement * normal;
        return midpoint;
    }

    // pretty inefficient but it's ok for small 'power'
    private int IntPow(int a, int pow)
    {
        int ret = 1;
        while (pow > 0)
        {
            ret *= a;
            pow--;
        }
        return ret;
    }
}
