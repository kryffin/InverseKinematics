using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{

    private List<Particle> _particles;

    public int NbParticles;
    public GameObject ParticlePrefab;

    public Sprite PeepoHappy;
    public Sprite PeepoSad;

    [Header("Gravity Strength")]
    [Range(-10f, 10f)]
    public float g;

    [Header("Interaction Radius")]
    [Range(0f, 15f)]
    public float h; //interaction radius

    [Header("Density")]
    [Range(-5f, 5f)]
    public float k; //pressure scale

    [Header("Target Rho")]
    [Range(0f, 10f)]
    public float rho_zero; //target rho

    [Header("DEBUG")]

    public bool DEBUG_DRAW_BOX = true;
    public bool DEBUG_DRAW_RANGES = false;

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
            Vector3 pos = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0f);
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

    private void ResolveCollisions()
    {
        foreach (Particle p in _particles)
        {
            if (p.GetPosition().y > 4.5f)
            {
                p.SetPosition(new Vector2(p.GetPosition().x, p.PreviousPosition.y));
                p.PreviousPosition = new Vector2(p.PreviousPosition.x, p.PreviousPosition.y + (p.GetPosition().y - p.PreviousPosition.y));
                p.Velocity.y = -p.Velocity.y;
            }

            if (p.GetPosition().y < -4.5f)
            {
                p.SetPosition(new Vector2(p.GetPosition().x, p.PreviousPosition.y));
                p.PreviousPosition = new Vector2(p.PreviousPosition.x, p.PreviousPosition.y + (p.PreviousPosition.y - p.GetPosition().y));
                p.Velocity.y = -p.Velocity.y;
            }

            if (p.GetPosition().x > 4.5f)
            {
                p.SetPosition(new Vector2(p.PreviousPosition.x, p.GetPosition().y));
                p.PreviousPosition = new Vector2(p.PreviousPosition.x + (p.GetPosition().x - p.PreviousPosition.x), p.PreviousPosition.y);
                p.Velocity.x = -p.Velocity.x;
            }

            if (p.GetPosition().x < -4.5f)
            {
                p.SetPosition(new Vector2(p.PreviousPosition.x, p.GetPosition().y));
                p.PreviousPosition = new Vector2(p.PreviousPosition.x + (p.PreviousPosition.x - p.GetPosition().x), p.PreviousPosition.y);
                p.Velocity.x = -p.Velocity.x;
            }
        }
    }

    private void SimulateParticles()
    {
        //apply gravity
        foreach (Particle p in _particles)
            p.Velocity = p.Velocity + (Time.deltaTime * (Vector2.down * g));

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
        ResolveCollisions();

        //use previous position to compute next velocity
        foreach (Particle p in _particles)
            p.Velocity = (p.GetPosition() - p.PreviousPosition) / Time.deltaTime;

        //display
        foreach (Particle p in _particles)
        {
            p.GameObject.GetComponent<SpriteRenderer>().flipX = p.Velocity.x > 0f;
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

        if (DEBUG_DRAW_RANGES)
        {
            foreach (Particle p in _particles)
            {
                Gizmos.DrawWireSphere(p.GetPosition(), h);
            }
        }

        if (DEBUG_DRAW_BOX)
        {
            Gizmos.DrawLine(new Vector3(-4.5f, 4.5f), new Vector3(-4.5f, -4.5f));
            Gizmos.DrawLine(new Vector3(-4.5f, -4.5f), new Vector3(4.5f, -4.5f));
            Gizmos.DrawLine(new Vector3(4.5f, -4.5f), new Vector3(4.5f, 4.5f));
            Gizmos.DrawLine(new Vector3(-4.5f, 4.5f), new Vector3(4.5f, 4.5f));
        }
    }

}
