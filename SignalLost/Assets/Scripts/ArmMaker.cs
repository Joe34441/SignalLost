using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMaker : MonoBehaviour
{
    [SerializeField] private GameObject startPosition = null;
    [SerializeField] private GameObject endPosition = null;

    [SerializeField] private float springStrength = 100f; //500
    [SerializeField] private float springDamper = 200f; //1500
    [SerializeField] private float springTolerance = 0.01f; //0.01
    //[SerializeField] private float springWidth = 0.25f; //0.25

    [SerializeField] private float armPointsScale = 0.25f;
    [SerializeField] private float armWidth = 0.25f;
    [SerializeField] private float distanceBetweenPoints = 1.0f;

    private List<GameObject> pointReferenceList = new List<GameObject>();

    LineRenderer lineRenderer;

    private int requiredPoints = 0;

    private float countTimer = 0.0f;
    private float countTotalTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        if (!lineRenderer) gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = armWidth;
        lineRenderer.endWidth = armWidth;

        MakeLinePointReferences();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLine();
    }

    private void MakeLinePointReferences()
    {
        float sizeOfGap = Vector3.Distance(startPosition.transform.position, endPosition.transform.position);
        requiredPoints = (int)(sizeOfGap / distanceBetweenPoints);

        for (int i = 0; i < requiredPoints; ++i)
        {
            //create GameObject
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "armPointReference " + i;
            obj.transform.parent = gameObject.transform;
            Vector3 distance = startPosition.transform.position - endPosition.transform.position;
            distance.x = distance.x / (i + 1);
            distance.y = distance.y / (i + 1);
            distance.z = distance.z / (i + 1);
            //obj.transform.localPosition = new Vector3(0, 0, distanceBetweenPoints * (i + 1));
            obj.transform.localPosition = startPosition.transform.position;
            obj.transform.localPosition += new Vector3(0, 0, distanceBetweenPoints * (i + 1));
            obj.transform.localScale = new Vector3(armPointsScale, armPointsScale, armPointsScale);
            obj.GetComponent<MeshRenderer>().enabled = false;

            pointReferenceList.Add(obj);
        }

        for (int i = 0; i < requiredPoints; ++i)
        {
            //setup left joint
            SpringJoint joint = pointReferenceList[i].AddComponent<SpringJoint>();
            joint.spring = springStrength;
            joint.damper = springDamper;
            joint.tolerance = springTolerance;
            //joint.enableCollision = true;
            //set connected body
            if (i == 0) joint.connectedBody = startPosition.GetComponent<Rigidbody>();
            else joint.connectedBody = pointReferenceList[i - 1].GetComponent<Rigidbody>();

        }

        SpringJoint abc = endPosition.AddComponent<SpringJoint>();
        abc.spring = springStrength;
        abc.damper = springDamper;
        abc.tolerance = springTolerance;
        abc.connectedBody = pointReferenceList[requiredPoints - 1].GetComponent<Rigidbody>();
        abc.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

    }

    private void UpdateLine()
    {
        if (countTimer >= countTotalTime)
        {
            startPosition.transform.position = FindObjectOfType<PlayerBodyParts>().GetRightHand().transform.position;
            lineRenderer.positionCount = pointReferenceList.Count;



            for (int i = 0; i < requiredPoints - 1; ++i)
            {
                startPosition.transform.position = pointReferenceList[i].transform.localPosition;
            }

            for (int i = 0; i < pointReferenceList.Count; ++i)
            {
                lineRenderer.SetPosition(i, pointReferenceList[i].transform.position);
            }

            countTimer = 0.0f;
        }
        else
        {
            countTimer += Time.deltaTime;
        }
    }
}