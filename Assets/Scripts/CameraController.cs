using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followTime = 0.2f;
    [SerializeField] private float yOffset = 1f;
    [SerializeField] private float screenShakeTime = 0.3f;

    private float screenShake;
    private float screenShakeVel;
    private Vector2 currVel;

    private void FixedUpdate()
    {
        transform.position += (Vector3)Random.insideUnitCircle * screenShake;
        screenShake = Mathf.SmoothDamp(screenShake, 0f, ref screenShakeVel, screenShakeTime);

        Vector3 targetPos = Vector2.SmoothDamp(transform.position, target.position + Vector3.up * yOffset, ref currVel, followTime);
        transform.position = targetPos + Vector3.forward * transform.position.z;
    }

    public static void ShakeScreen(float amount)
    {
        Camera.main.GetComponent<CameraController>().screenShake += amount;
    }
}
