using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float stageMaxX = 10f;
    public float stageMinX = -10f;
    private GameObject player2;

    private void Start()
    {
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.D))
        {
            MoveRightPressed();
        }
        if (Input.GetKey(KeyCode.A))
        {
            MoveLeftPressed();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var pushForce = UnityEngine.Random.Range(200, 800);
            player2.transform.Translate(Vector3.right * Time.deltaTime * pushForce);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            MoveRightReleased();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            MoveLeftReleased();
        }

    }



    public Action onMoveRightPressed;
    public void MoveRightPressed()
    {
        if (onMoveRightPressed != null)
        {
            onMoveRightPressed();
        }
    }

    public Action onMoveLeftPressed;
    public void MoveLeftPressed()
    {
        if (onMoveLeftPressed != null)
        {
            onMoveLeftPressed();
        }
    }

    public Action onMoveLeftReleased;
    public void MoveLeftReleased()
    {
        if (onMoveLeftReleased != null)
        {
            onMoveLeftReleased();
        }
    }

    public Action onMoveRightReleased;
    public void MoveRightReleased()
    {
        if (onMoveRightReleased != null)
        {
            onMoveRightReleased();
        }
    }
}
