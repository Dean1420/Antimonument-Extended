using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ShelfSwitching : MonoBehaviour
{
    public Transform[] Decos;
    public BoxCollider closetCollider;

    public List<Transform>[] decoChildren;

    public int curDecoNum;

    void Start()
    {
        //init list
        decoChildren = new List<Transform>[Decos.Length];

        for (int i = 0; i < decoChildren.Length; i++)
        {
            decoChildren[i] = new List<Transform>();
        }

        curDecoNum = 0;

        //init closet
        for (int i = 0; i < Decos.Length; i++)
        {
            if (Decos[i] == null) continue;

            Decos[i].gameObject.SetActive(true);

            //get children of decos
            XRGrabInteractable[] grabComponents = Decos[i].GetComponentsInChildren<XRGrabInteractable>(true);

            foreach (XRGrabInteractable grabItem in grabComponents)
            {
                decoChildren[i].Add(grabItem.transform);
            }

            //get shelfs - none of their child have grab interactable
            foreach (Transform directChild in Decos[i])
            {
                bool hasRegisteredChild = false;

                Transform[] subChildren = directChild.GetComponentsInChildren<Transform>(true);
                foreach (Transform sub in subChildren)
                {
                    if (decoChildren[i].Contains(sub))
                    {
                        hasRegisteredChild = true;
                        break;
                    }
                }

                if (!hasRegisteredChild)
                {
                    decoChildren[i].Add(directChild);
                }
            }

            //set active false/true
            if (decoChildren[i].Count > 0)
            {
                for (int j = 0; j < decoChildren[i].Count; j++)
                {
                    decoChildren[i][j].gameObject.SetActive(i == 0);
                }
            }
        }

    }

    public void SwitchingLeft()
    {
        closetCollider.gameObject.SetActive(true);
        for (int i = 0; i < decoChildren[curDecoNum].Count; i++)
        {
            if (!closetCollider.bounds.Contains(decoChildren[curDecoNum][i].transform.position))
            {
                //Debug.Log($"{decoChildren[curDecoNum][i].name} is out of closet so it isn't switched");
                continue;
            }

            decoChildren[curDecoNum][i].gameObject.SetActive(false);
        }

        curDecoNum--;
        if (curDecoNum < 0)
            curDecoNum = Decos.Length - 1;

        for (int i = 0; i < decoChildren[curDecoNum].Count; i++)
        {
            decoChildren[curDecoNum][i].gameObject.SetActive(true);
        }
        closetCollider.gameObject.SetActive(false);
    }

    public void SwitchingRight()
    {
        closetCollider.gameObject.SetActive(true);
        for (int i = 0; i < decoChildren[curDecoNum].Count; i++)
        {
            if (!closetCollider.bounds.Contains(decoChildren[curDecoNum][i].transform.position))
            {
                //Debug.Log($"{decoChildren[curDecoNum][i].name} is out of closet so it isn't switched");
                continue;
            }

            decoChildren[curDecoNum][i].gameObject.SetActive(false);
        }

        curDecoNum++;
        if (curDecoNum == Decos.Length)
            curDecoNum = 0;

        for (int i = 0; i < decoChildren[curDecoNum].Count; i++)
        {
            decoChildren[curDecoNum][i].gameObject.SetActive(true);
        }
        closetCollider.gameObject.SetActive(false);
    }
}
