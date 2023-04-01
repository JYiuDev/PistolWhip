using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipPivot : MonoBehaviour
{
    private Vector2 AimPosition;
    private Camera cam;
    private Vector3 mousePos;
    private Animator animator;
    private bool rotate = true;

    void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    void Update()
    {

        if(Input.GetKeyDown("Fire2"))
        {

        }

        if(!rotate)
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition); //translate screen mouse position to world
            Vector3 direction = (mousePos - transform.position).normalized;       //Vector from weapon to mouse

            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;  //Calculate rotation angle from vector

            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
    }

    //Stopping rotation when function is called to accomodate for whip attack
    public void stopRotate()
    {
        rotate = false;
    }

    public void startRotate()
    {
        rotate = true;
    }
}
