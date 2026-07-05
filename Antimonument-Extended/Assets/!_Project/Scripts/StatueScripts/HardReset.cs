using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HardReset : MonoBehaviour
{

    // public GameObject StatLoewe;
    // public GameObject StatSchnecke;
    // public GameObject StatJuengling;
    //
    // public Transform StatTransLoewe;
    // public Transform StatTransSchnecke;
    // public Transform StatTransJuengling;

    public GameObject Cam;
    public Transform CamTrans;

    public GameObject[] Flowers;
    public Transform[] FlowerTrans;

    public GameObject[] Assets;
    public Transform[] AssetsTrans;
    public GameObject[] AssetsPrefab;
    public ReflectionProbe Probe;
    //[SerializeField] TexturePainter TexPaint;



    

    void OnTriggerEnter(Collider col)
    {
        var statue = col.GetComponent<Statue>();
        if (statue != null)
        {
            ResetStatue(statue.StatueValues);
        }

    }

    private void ResetStatue(StatueValues statue)
    {
        // StatLoewe.transform.position = StatTransLoewe.position;
        // StatSchnecke.transform.position = StatTransSchnecke.position;
        // StatJuengling.transform.position = StatTransJuengling.position;
        //
        // StatLoewe.transform.eulerAngles = StatTransLoewe.eulerAngles;
        // StatSchnecke.transform.eulerAngles = StatTransSchnecke.eulerAngles;
        // StatJuengling.transform.eulerAngles = StatTransJuengling.eulerAngles;

        if(StatueSpawnController.Singleton.RequestStatueReset(statue))
        {
            // RenderSettings.skybox = TexPaint.SkyBoxHub;
            DynamicGI.UpdateEnvironment();
            Probe.RenderProbe();
            Reset(Cam, CamTrans);
            //TexPaint.ResetAll();
            Resources.UnloadUnusedAssets();
        }
        
        ResetFlowers();
        ResetAssets();
        DeleteShields();
    }


    public void DeleteShields()
    {
        GameObject[] NewShield = GameObject.FindGameObjectsWithTag("Interactable");
        for (int i = 0; i < NewShield.Length; i++)
        {
            if(NewShield[i].name == "NewProtestShield")
            {
                Destroy(NewShield[i]);
            }
            
        }
    }

    private void Reset(GameObject obj, Transform trans)
    {
        obj.transform.position = trans.position;
        obj.transform.eulerAngles = trans.eulerAngles;
    }

    private void ResetFlowers()
    {
        for (int i = 0; i < Flowers.Count(); i++)
        {
            Flowers[i].transform.position = FlowerTrans[i].position;
            Flowers[i].transform.eulerAngles = FlowerTrans[i].eulerAngles;
        }
    }

    private void ResetAssets()
    {
        for (int i = 0; i < Assets.Count(); i++)
        {
            if (Assets[i].name == "Decoration")
            {
                for (int j = 0; j < Assets[i].transform.childCount; j++)
                {
                    if (Assets[i].transform.GetChild(j).GetComponentInChildren<XRGrabInteractable>() == null)
                    {
                        continue;
                    }
                    Transform child = Assets[i].transform.GetChild(j);
                    Transform prefabChild = AssetsPrefab[i].transform.GetChild(j);

                    var grabs = child.GetComponentsInChildren<XRGrabInteractable>();
                    var prefabGrabs = prefabChild.GetComponentsInChildren<XRGrabInteractable>();

                    for (int k = 0; k < Mathf.Min(grabs.Length, prefabGrabs.Length); k++)
                    {
                        grabs[k].transform.SetLocalPositionAndRotation(
                            prefabGrabs[k].transform.localPosition,
                            prefabGrabs[k].transform.localRotation);
                    }
                }
            } else
            {
                Assets[i].transform.localPosition = AssetsTrans[i].localPosition;
                Assets[i].transform.eulerAngles = AssetsTrans[i].eulerAngles;
            }
        }
    }

    public void ResetAddOns()
    {
        ResetAssets();
        ResetFlowers();
    }

}
