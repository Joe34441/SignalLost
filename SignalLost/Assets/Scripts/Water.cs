using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] string handTag = "FakeHand";

    [SerializeField] GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(handTag))
        {
            player.GetComponent<FPCharacterController>().WaterHit(other.gameObject.transform.position);
        }
    }
}
