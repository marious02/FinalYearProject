using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public float move = 2f;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float GetHorizontal = Input.GetAxis("Horizontal");
        float GetVertical = Input.GetAxis("Vertical");
        Vector2 MovementDirection = new Vector2(GetHorizontal, GetVertical).normalized;
        transform.Translate(MovementDirection * speed * Time.deltaTime);
        

    }
}
