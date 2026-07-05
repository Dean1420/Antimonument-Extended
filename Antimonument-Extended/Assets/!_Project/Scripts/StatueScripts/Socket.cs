using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Socket : MonoBehaviour
{

    public TextMeshPro TitleText;
    [FormerlySerializedAs("GrabbableStatue")] public Statue Statue;
    public StatueValues StatueValues;
    // Start is called before the first frame update
    public void Init(StatueValues statue)
    {
        StatueValues = statue;
        
        Statue = Instantiate(statue.GrabbableStatuePrefab, transform);
        Statue.transform.localPosition = statue.SocketSpawnPoint;
        Statue.transform.localRotation = Quaternion.Euler(statue.SocketSpawnRotation);
        
        Statue.StatueValues =  statue;
        TitleText.text = statue.Title;
    }

    public void Reset()
    {
        Statue.transform.localPosition = StatueValues.SocketSpawnPoint;
        Statue.transform.localRotation = Quaternion.Euler(StatueValues.SocketSpawnRotation);
        
        // Statue.StatueValues =  StatueValues;
        TitleText.text = StatueValues.Title;
    }

    public void Despawn()
    {
        StatueValues = null;
        Statue = null;
    }
}
