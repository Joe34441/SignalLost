using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMaker : MonoBehaviour
{
    [SerializeField] private float lengthOfArm;
    [SerializeField] private float distanceBetweenPoints;
    [SerializeField] private float pointScale = 0.1f;
    [SerializeField] private float armWidth = 0.1f;

    private List<Vector3> pointPosList = new List<Vector3>();
    private List<GameObject> pointReferenceList = new List<GameObject>();
    private int numOfPoints = 0;

    LineRenderer lineRenderer;



    private bool proceed = false;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        if (!lineRenderer) gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = armWidth;
        lineRenderer.endWidth = armWidth;

        if (lengthOfArm > distanceBetweenPoints) DoStuff();
    }

    // Update is called once per frame
    void Update()
    {
        if (proceed) UpdateLine();
    }

    private void DoStuff()
    {
        int requiredPoints = (int)(lengthOfArm / distanceBetweenPoints);

        for (int i = numOfPoints; i < requiredPoints; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.parent = gameObject.transform;
            obj.transform.localPosition = new Vector3(0.0f, 0.0f, distanceBetweenPoints * i);
            obj.transform.localScale = new Vector3(pointScale, pointScale, pointScale);
            obj.GetComponent<MeshRenderer>().enabled = false;
            obj.AddComponent<HingeJoint>();
            pointReferenceList.Add(obj);
            numOfPoints++;
        }

        for (int i = 0; i < numOfPoints; ++i)
        {
            if (i < numOfPoints - 1)
            {
                if (i == 0)
                {
                    //line end pos
                    pointReferenceList[i].GetComponent<HingeJoint>().connectedBody = pointReferenceList[i + 1].GetComponent<Rigidbody>();
                    pointReferenceList[i].GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    pointReferenceList[i].GetComponent<HingeJoint>().connectedBody = pointReferenceList[i + 1].GetComponent<Rigidbody>();
                }
            }
            else
            {
                //line start pos
                pointReferenceList[i].GetComponent<Rigidbody>().isKinematic = true;
            }

            pointPosList.Add(pointReferenceList[i].transform.position + gameObject.transform.position);
        }

        proceed = true;
    }

    private void UpdateLine()
    {
        lineRenderer.positionCount = pointPosList.Count;
        for (int i = 0; i < pointPosList.Count; ++i)
        {
            lineRenderer.SetPosition(i, pointReferenceList[i].transform.localPosition);
        }
    }
}
