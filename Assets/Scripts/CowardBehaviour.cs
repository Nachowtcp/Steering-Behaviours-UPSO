using UnityEngine;

public class CowardBehavior : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float steerStrength = 4f; // Gira un poco más rápido que el mago para poder escapar
    public float fleeDistance = 4f;

    public float wanderRadius = 1.5f;
    public float wanderDistance = 2f;
    public float wanderJitter = 0.2f;

    private Rigidbody2D rb;
    private float wanderAngle;

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
        Vector2 desiredVelocity;

        if (distanceToPlayer < fleeDistance)
        {
            // COMPORTAMIENTO FLEE
            // La dirección deseada es alejarse del jugador
            Vector2 fleeDirection = transform.position - player.position;
            desiredVelocity = fleeDirection.normalized * moveSpeed;
        }
        else
        {
            // COMPORTAMIENTO WANDER (Idéntico al del Mago)
            wanderAngle += Random.Range(-wanderJitter, wanderJitter);
            
            Vector2 currentVelocity = rb.linearVelocity;
            if (currentVelocity == Vector2.zero) currentVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            
            Vector2 circleCenter = currentVelocity.normalized * wanderDistance;
            Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;
            Vector2 wanderTarget = circleCenter + displacement;

            desiredVelocity = wanderTarget.normalized * moveSpeed;
        }

        // STEERING CLÁSICO
        Vector2 steering = desiredVelocity - rb.linearVelocity;
        steering = Vector2.ClampMagnitude(steering, steerStrength);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity + steering * Time.fixedDeltaTime, moveSpeed);
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 1. Obtenemos la normal del choque (un vector que apunta directo hacia afuera de la pared)
        Vector2 wallNormal = collision.contacts[0].normal;
        
        // 2. Sobrescribimos la velocidad momentáneamente para empujarlo en esa dirección
        rb.linearVelocity = wallNormal * moveSpeed;

        // 3. Ajustamos el ángulo del Wander para que el círculo apunte hacia donde rebotó.
        // Mathf.Atan2 convierte una dirección (X e Y) en su ángulo correspondiente.
        wanderAngle = Mathf.Atan2(wallNormal.y, wallNormal.x);
    }
}