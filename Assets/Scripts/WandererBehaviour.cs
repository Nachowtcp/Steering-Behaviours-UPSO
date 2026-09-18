using UnityEngine;

public class WandererBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float steerStrength = 2f; // Qué tan rápido puede girar (menor = más suave)
    
    public float wanderRadius = 1.5f;
    public float wanderDistance = 2f;
    public float wanderJitter = 0.2f; // Mantenerlo bajito

    private Rigidbody2D rb;
    private float wanderAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wanderAngle = Random.Range(0f, 360f); // Arranca mirando a un lado aleatorio
    }

    void FixedUpdate()
    {
        // 1. Variamos el ángulo de a poco
        wanderAngle += Random.Range(-wanderJitter, wanderJitter);

        // 2. Calculamos el centro del círculo proyectado hacia adelante
        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity == Vector2.zero) currentVelocity = transform.up; // Empujón inicial si está quieto
        
        Vector2 circleCenter = currentVelocity.normalized * wanderDistance;

        // 3. Buscamos el punto en el borde del círculo usando el ángulo
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;
        
        // El punto objetivo final al que queremos ir
        Vector2 wanderTarget = circleCenter + displacement;

        // 4. STEERING CLÁSICO: Velocidad Deseada - Velocidad Actual
        Vector2 desiredVelocity = wanderTarget.normalized * moveSpeed;
        Vector2 steering = desiredVelocity - rb.linearVelocity;

        // Limitamos la fuerza de giro para que no sea un cambio brusco
        steering = Vector2.ClampMagnitude(steering, steerStrength);

        // 5. Aplicamos la fuerza suavemente a la velocidad
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