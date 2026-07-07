using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosterSound : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;

    private void OnCollisionExit(Collision collision)
    {
        soundManager.PlayPosterSound(GetComponent<AudioSource>());
    }
}
