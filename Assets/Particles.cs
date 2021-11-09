using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{

    private List<Particle> _particles;

    public int NbParticles;
    public GameObject ParticlePrefab;

    public Sprite PeepoHappy;
    public Sprite PeepoSad;

    public float GravityStrength;
    public float h; //interaction radius
    public float k; //pressure scale
    public float rho_zero; //target rho

    public class Particle
    {
        public GameObject GameObject;
        public Vector2 PreviousPosition;
        public Vector2 Velocity;
        public float Mass;
        public Color Color;

        public Particle(GameObject gameObject)
        {
            this.GameObject = gameObject;
            this.PreviousPosition = gameObject.transform.position;
            this.Velocity = Vector2.zero;
            this.Mass = 0f;
            this.Color = Color.white;
        }

        public Vector2 GetPosition()
        {
            return GameObject.transform.position;
        }

        public void SetPosition(Vector2 pos)
        {
            GameObject.transform.position = pos;
        }
    }

    void Start()
    {
        _particles = new List<Particle>();

        for (int i = 0; i < NbParticles; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), 0f);
            _particles.Add(new Particle(Instantiate(ParticlePrefab, pos, Quaternion.identity, this.transform)));
        }
    }

    private List<Particle> Neighbors (Particle particle)
    {
        List<Particle> neighbors = new List<Particle>();

        foreach (Particle p in _particles)
        {
            if (Vector2.Distance(particle.GetPosition(), p.GetPosition()) < h && p.GetPosition() != particle.GetPosition())
                neighbors.Add(p);
        }

        return neighbors;
    }

    private void DoubleDensityRelaxation()
    {
        foreach (Particle p in _particles)
        {
            float rho = 0;

            List<Particle> neighbors = Neighbors(p);

            //compute density
            foreach (Particle n in neighbors)
            {
                float r = Vector2.Distance(p.GetPosition(), n.GetPosition());
                float q = r / h;
                if (q < 1f)
                    rho = rho + Mathf.Pow((1 - q), 2f);
            }

            //compute pressure
            float P = k * (rho - rho_zero);
            Vector2 dx = Vector2.zero;

            foreach (Particle n in neighbors)
            {
                float r = Vector2.Distance(p.GetPosition(), n.GetPosition());
                float q = r / h;
                if (q < 1f)
                {
                    //apply displacement
                    Vector2 D = Mathf.Pow(Time.deltaTime, 2f) * (P * (1f - q)) * (n.GetPosition() - p.GetPosition()).normalized;
                    n.SetPosition(n.GetPosition() + (D / 2f));
                    dx = dx - (D / 2f);
                }
            }
            p.SetPosition(p.GetPosition() + dx);
        }
    }

    private void SimulateParticles()
    {
        //apply gravity
        foreach (Particle p in _particles)
            p.Velocity = p.Velocity + (Time.deltaTime * (Vector2.down * GravityStrength));

        //modify velocities with pairwise viscosity impulses
        //ApplyViscosity();

        foreach (Particle p in _particles)
        {
            //save previous position
            p.PreviousPosition = p.GetPosition();
            //advance to predicted position
            p.SetPosition(p.GetPosition() + (Time.deltaTime * p.Velocity));
        }

        //add and remove springs, change rest lengths
        //AdjustSprings();

        //modify positions according to springs, double density relaxation, and collisions
        //ApplySpringDisplacement();
        DoubleDensityRelaxation();
        //ResolveCollisions();

        //use previous position to compute next velocity
        foreach (Particle p in _particles)
            p.Velocity = (p.GetPosition() - p.PreviousPosition) / Time.deltaTime;

        //display
        foreach (Particle p in _particles)
        {
            p.GameObject.GetComponent<SpriteRenderer>().flipX = p.Velocity.x < 0f;
            if (Neighbors(p).Count > 0)
                p.GameObject.GetComponent<SpriteRenderer>().sprite = PeepoHappy;
            else
                p.GameObject.GetComponent<SpriteRenderer>().sprite = PeepoSad;
        }
    }

    void Update()
    {
        SimulateParticles();
    }

    private void OnDrawGizmos()
    {
        if (_particles == null) return;

        foreach (Particle p in _particles)
        {
            Gizmos.DrawWireSphere(p.GetPosition(), h);
        }
    }

}
