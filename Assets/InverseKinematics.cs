using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InverseKinematics : MonoBehaviour
{

    private List<Vector3> points;
    private LineRenderer lr;

    public bool drawDebug = false;

    public Transform target;
    public int nbIterations;

    public float constraintRadius;

    private void Start()
    {
        points = new List<Vector3>();
        lr = GetComponent<LineRenderer>();

        Selection.SetActiveObjectWithContext(target, this); //default selection on Play is the target
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView)); //default view on Play is the Scene View
    }

    private void ColorLine(Vector3 end)
    {
        if (Vector3.Distance(end, target.position) <= 0.1f)
        {
            lr.startColor = Color.green;
            lr.endColor = Color.green;
        }
        else
        {
            lr.startColor = Color.red;
            lr.endColor = Color.red;
        }
    }

    private void DrawAngles(List<Vector3> line)
    {
        for (int i = 0; i < line.Count-1; i++)
        {
            Vector3 dir = (line[i + 1] - line[i]).normalized;

            Vector3 tmpDir = Quaternion.Euler(constraintRadius, 0f, 0f) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.red);

            tmpDir = Quaternion.Euler(-constraintRadius, 0f, 0f) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.red);

            tmpDir = Quaternion.Euler(0f, constraintRadius, 0f) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.green);

            tmpDir = Quaternion.Euler(0f, -constraintRadius, 0f) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.green);

            tmpDir = Quaternion.Euler(0f, 0f, constraintRadius) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.blue);

            tmpDir = Quaternion.Euler(0f, 0f, -constraintRadius) * dir;
            Debug.DrawRay(line[i + 1], tmpDir, Color.blue);
        }
    }

    private void Update()
    {
        points.Clear();
        lr.positionCount = transform.childCount;

        int nb = 0;
        foreach (Transform t in transform)
        {
            points.Add(t.position);
            lr.SetPosition(nb++, t.position);
        }

        List<Vector3> res = new List<Vector3>(points);
        for (int i = 0; i < nbIterations; i++)
        {
            res = ForwardKinematics(BackwardKinematics(res), res);
        }

        nb = 0;
        foreach (Transform t in transform)
            t.position = res[nb++];

        ColorLine(res[res.Count-1]);

        if (drawDebug) DrawAngles(res);
    }

    private List<Vector3> BackwardKinematics(List<Vector3> ps)
    {
        List<Vector3> ps_prime = new List<Vector3>(ps);

        ps_prime[ps_prime.Count - 1] = target.position;
        for (int i = ps_prime.Count-2; i >= 0; i--)
        {
            Vector3 dir = (ps[i] - ps_prime[i + 1]).normalized;
            float length = Vector3.Distance(ps[i], ps[i+1]);
            ps_prime[i] = ps_prime[i + 1] + (length * dir);
        }

        return ps_prime;
    }

    private List<Vector3> ForwardKinematics(List<Vector3> ps_prime, List<Vector3> ps)
    {
        List<Vector3> ps_prime_prime = new List<Vector3>(ps_prime);

        ps_prime_prime[0] = ps[0];
        for (int i = 0; i < ps_prime_prime.Count - 1; i++)
        {
            Vector3 dir = (ps_prime[i + 1] - ps_prime_prime[i]).normalized;
            float length = Vector3.Distance(ps[i], ps[i + 1]);
            ps_prime_prime[i + 1] = ps_prime_prime[i] + (length * dir);
        }

        return ps_prime_prime;
    }

}
