using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestalSound : MonoBehaviour
{
    //[SerializeField] SoundManager soundManager;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Interactable"))
        {
            //soundManager.PlayPutStatueDownSound(GetComponent<AudioSource>());
        }
    }
}
