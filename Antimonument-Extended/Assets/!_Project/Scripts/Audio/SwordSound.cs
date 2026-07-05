using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSound : MonoBehaviour
{
    //[SerializeField] SoundManager soundManager;

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sword"))
        {
            soundManager.PlaySwordCutSound(true);
        }
            
    }*/

    public void PlaySound()
    {
        //soundManager.PlaySwordCutSound(true);
    }

   /* private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sword"))
            soundManager.PlaySwordCutSound(false);
    }*/
}
