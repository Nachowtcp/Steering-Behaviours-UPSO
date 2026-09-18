using UnityEngine;

public class ChaserBehavior : MonoBehaviour
{
    public Transform player;
    public float maxSpeed = 3.5f;
    public float steerStrength = 5f;

    // Distancias para cambiar de estado
    public float chaseDistance = 5f; // Distancia a la que te detecta y empieza el Seek
    public float loseDistance = 7f;  // Distancia a la que te pierde y vuelve al Wander

    // Distancias para el Arrive
    public float slowDownRadius = 2f; // Cuándo empieza a frenar
    public float stopRadius = 0.5f;   // Dónde frena por completo

    // Variables para el Wander
    public float wanderRadius = 1.5f;
    public float wanderDistance = 2f;
    public float wanderJitter = 0.2f;

    private Rigidbody2D rb;
    private float wanderAngle;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wanderAngle = Random.Range(0f, 360f);

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 desiredVelocity = Vector2.zero;

        // Máquina de estados simple: decidimos si persigue o deambula
        if (!isChasing && distanceToPlayer < chaseDistance)
        {
            isChasing = true; // Empieza a perseguir
        }
        else if (isChasing && distanceToPlayer > loseDistance)
        {
            isChasing = false; // Te perdió, vuelve a deambular
        }

        if (isChasing)
        {
            // COMPORTAMIENTO SEEK + ARRIVE
            Vector2 toPlayer = player.position - transform.position;
            float distance = toPlayer.magnitude;

            if (distance < stopRadius)
            {
                desiredVelocity = Vector2.zero; // Frena por completo
            }
            else
            {
                float speed = maxSpeed;
                if (distance < slowDownRadius)
                {
                    // Frenado progresivo (Arrive) a medida que se acerca
                    speed = maxSpeed * (distance / slowDownRadius);
                }
                desiredVelocity = toPlayer.normalized * speed;
            }
        }
        else
        {
            // COMPORTAMIENTO WANDER
            wanderAngle += Random.Range(-wanderJitter, wanderJitter);
            
            Vector2 currentVelocity = rb.linearVelocity;
            if (currentVelocity == Vector2.zero) currentVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            
            Vector2 circleCenter = currentVelocity.normalized * wanderDistance;
            Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;
            Vector2 wanderTarget = circleCenter + displacement;

            desiredVelocity = wanderTarget.normalized * maxSpeed;
        }

        // STEERING FINAL
        Vector2 steering = desiredVelocity - rb.linearVelocity;
        steering = Vector2.ClampMagnitude(steering, steerStrength);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity + steering * Time.fixedDeltaTime, maxSpeed);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Vector2 wallNormal = collision.contacts[0].normal;
        rb.linearVelocity = wallNormal * maxSpeed;
        wanderAngle = Mathf.Atan2(wallNormal.y, wallNormal.x);
    }
}
