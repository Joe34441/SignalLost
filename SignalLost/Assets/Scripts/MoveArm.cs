using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArm : MonoBehaviour
{
    [SerializeField] LineRenderer lineRendererRightArm;
    [SerializeField] LineRenderer lineRendererLeftArm;


    private bool drawRightArm = false;
    private bool drawLeftArm = false;
    private float armWidth = 1.0f;

    public void DrawRightArm(bool enabled) { drawRightArm = enabled; }
    public void DrawLeftArm(bool enabled) { drawLeftArm = enabled; }
    public void SetWidth(float width) { armWidth = width; }


    void Update()
    {
        if (drawRightArm)
        {
            lineRendererRightArm.enabled = true;
            ApplyWidth(armWidth);
            lineRendererRightArm.SetPosition(0, FindObjectOfType<PlayerBodyParts>().GetRightExtenderPoint().transform.position);
            lineRendererRightArm.SetPosition(1, FindObjectOfType<FPCharacterController>().GetFakeRightHand().transform.position);
        }
        else lineRendererRightArm.enabled = false;

        if (drawLeftArm)
        {
            lineRendererLeftArm.enabled = true;
            ApplyWidth(armWidth);
            lineRendererLeftArm.SetPosition(0, FindObjectOfType<PlayerBodyParts>().GetLeftExtenderPoint().transform.position);
            lineRendererLeftArm.SetPosition(1, FindObjectOfType<FPCharacterController>().GetFakeLeftHand().transform.position);
        }
        else lineRendererLeftArm.enabled = false;

    }

    private void ApplyWidth(float width)
    {
        lineRendererRightArm.startWidth = width;
        lineRendererRightArm.endWidth = width;
        lineRendererLeftArm.startWidth = width;
        lineRendererLeftArm.endWidth = width;
    }
}
