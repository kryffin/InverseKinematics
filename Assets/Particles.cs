using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{

    private List<GameObject> particles;

    public int nbParticles;
    public GameObject particlePrefab;
    public float speed;

    void Start()
    {
        particles = new List<GameObject>();

        for (int i = 0; i < nbParticles; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), 0f);
            particles.Add(Instantiate(particlePrefab, pos, Quaternion.identity, this.transform));
        }
    }

    private void UpdateParticles()
    {
        foreach (GameObject g in particles)
        {
            Vector3 motion = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            g.transform.Translate(motion * speed * Time.deltaTime);
        }
    }

    void Update()
    {
        UpdateParticles();
    }

}
