using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRootScript : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followSpeed;
    public float minFollowDistance;
    public Vector3 followOffset;

    [Space]

    [Header("Transform References")]
    public Transform follow;

    void Start()
    {
        //Detaches Transform
        transform.SetParent(transform.parent.parent);
    }

    void Update()
    {
        Vector3 targetPos = follow.transform.position + followOffset;
        if (Vector3.Distance(transform.position, targetPos) > minFollowDistance)
        {
            transform.position += Time.deltaTime * followSpeed * (targetPos - transform.position).normalized;
        }
    }
}
