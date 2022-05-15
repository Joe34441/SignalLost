using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<GameObject> destinations = new List<GameObject>();
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private bool returnJourney = false;

    private GameObject player;

    private int currentDestinationIndex;
    private int nextDestinationDifference = 1;

    private float startTime = 0.0f;
    private float journeyLength;

    private Vector3 startPos;
    private Vector3 endPos;

    // Update is called once per frame
    void Update()
    {
        if (startTime == 0)
        {
            startTime = Time.time;

            if (currentDestinationIndex >= destinations.Count - 1)
            {
                if (returnJourney) nextDestinationDifference *= -1;
                else currentDestinationIndex = 0;
            }
            if (currentDestinationIndex < 0 ||
                currentDestinationIndex == 0 && nextDestinationDifference == -1 ||
                currentDestinationIndex == destinations.Count - 1 && nextDestinationDifference == 1)
            {
                nextDestinationDifference *= -1;
            }

            startPos = destinations[currentDestinationIndex].transform.position;
            endPos = destinations[currentDestinationIndex + nextDestinationDifference].transform.position;
            journeyLength = Vector3.Distance(startPos, endPos);

        }

        float distanceCovered = (Time.time - startTime) * moveSpeed;
        float fractionOfJourney = distanceCovered / journeyLength;

        gameObject.transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);

        if (gameObject.transform.position == endPos)
        {
            currentDestinationIndex += nextDestinationDifference;
            startTime = 0.0f;
        }
    }
}
