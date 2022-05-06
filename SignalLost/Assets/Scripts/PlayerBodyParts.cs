using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyParts : MonoBehaviour
{
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;


    public GameObject GetLeftHand() { return leftHand; }
    public void SetLeftHand(GameObject _hand) { leftHand = _hand; }
    public GameObject GetRightHand() { return rightHand; }
    public void SetRightHand(GameObject _hand) { rightHand = _hand; }


}
