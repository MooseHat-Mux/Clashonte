using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        ThirdPersonController player = collision.gameObject.GetComponentInParent<ThirdPersonController>();

        if (player != null)
        {
            player.transform.position = GameManager.instance.startingPlayerPos.position;
        }
    }
}
