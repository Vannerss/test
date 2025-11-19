using System;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public Transform muzzle;          // Where pellets spawn
    public float shootInterval = 1f;  // Time between shots
    public float pelletSpeed = 20f;
    public float pelletLifetime = 3f;
    public float pelletSize = 0.2f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= shootInterval)
        {
            timer = 0f;
            Shoot();
        }
    }
    
    void Shoot()
    {
        // Create pellet
        GameObject pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pellet.transform.position = muzzle.position;
        pellet.transform.rotation = muzzle.rotation;
        pellet.transform.localScale = Vector3.one * pelletSize;

        // Add Rigidbody
        Rigidbody rb = pellet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Launch forward
        rb.linearVelocity = muzzle.forward * pelletSpeed;

        // Cleanup
        Destroy(pellet, pelletLifetime);
    }
}