using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyParts : MonoBehaviour
{
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    [SerializeField] private GameObject leftExtenderPoint;
    [SerializeField] private GameObject rightExtenderPoint;

    [SerializeField] private GameObject leftShoulder;
    [SerializeField] private GameObject rightShoulder;

    public GameObject GetLeftHand() { return leftHand; }
    public void SetLeftHand(GameObject _hand) { leftHand = _hand; }
    public GameObject GetRightHand() { return rightHand; }
    public void SetRightHand(GameObject _hand) { rightHand = _hand; }

    public GameObject GetLeftExtenderPoint() { return leftExtenderPoint; }
    public GameObject GetRightExtenderPoint() { return rightExtenderPoint; }

    public GameObject GetLeftShoulder() { return leftShoulder; }
    public GameObject GetRightShoulder() { return rightShoulder; }
}
