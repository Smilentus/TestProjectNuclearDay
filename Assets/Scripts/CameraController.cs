using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerObject;

    public Vector3 cameraOffset;
    private float cameraMoveSpeed = 0.125f;

    void Awake()
    {
        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = playerObject.position + cameraOffset;
        
        Vector3 smoothPosition = Vector3.Lerp(
            this.transform.position,
            targetPosition,
            cameraMoveSpeed
            );

        transform.position = smoothPosition;
    }
}
