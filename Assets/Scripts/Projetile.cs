using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projetile : MonoBehaviour
{
    public GameObject projectile;

    private bool set = false;
    private Vector3 firePos;
    private Vector3 direction;
    private float speed;
    private float timeElapsed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!set)
            return;
        timeElapsed += Time.deltaTime;
        //print("Bullet direction : " + direction);
        transform.position = firePos + direction * speed * timeElapsed / 100.0f;
        // transform.position += Physics.gravity * (timeElapsed * timeElapsed) / 2.0f;
        // extra validation for cleaning the scene
        if (timeElapsed > 10.0f)
            Destroy(this.gameObject);// or set = false; and hide it
    }

    public void Set(Vector3 firePos, Vector3 direction, float speed)
    {
        this.firePos = firePos;
        this.direction = direction.normalized;
        this.speed = speed;
        transform.position = firePos;
        this.timeElapsed = 0;
        set = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag ("Player")) {
            // Debug.Log("Crash detected with " + collision.collider.name);
            Destroy(this.gameObject);
        }
    }
}
