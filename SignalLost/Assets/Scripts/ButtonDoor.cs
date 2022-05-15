using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDoor : MonoBehaviour
{

    [SerializeField] GameObject door;
    [SerializeField] GameObject referencePoint;
    [SerializeField] private float moveSpeed = 0.25f;
    [SerializeField] string handTag = "FakeHand";

    private bool activate = false;
    private bool isActivated = false;

    private float startTime = 0.0f;
    private float journeyLength;

    private Vector3 startPos;
    private Vector3 endPos;

    // Update is called once per frame
    void Update()
    {
        if (activate && !isActivated)
        {
            isActivated = true;

            startTime = Time.time;

            startPos = door.transform.position;
            endPos = referencePoint.transform.position;

            journeyLength = Vector3.Distance(startPos, endPos);
        }
        else if (activate)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            door.transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);

            if (door.transform.position == endPos) activate = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(handTag))
        {
            if (!isActivated) activate = true;
        }
    }
}
