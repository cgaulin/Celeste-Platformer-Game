using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXHandler : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private PlayerController playerController;

    [Header("Footsteps Audio Info")]
    [SerializeField] private AudioSource footstepsAudioSource;

    [Header("Jump Audio Info")]
    [SerializeField] private AudioSource jumpAudioSource;

    private void Update()
    {
        FootstepsInGrassSFX();
        JumpingSFX();
    }

    private void FootstepsInGrassSFX()
    {
        if (playerController.GetMoveVector().x != 0f && playerController.IsGrounded())
        {
            footstepsAudioSource.enabled = true;
        }
        else
        {
            footstepsAudioSource.enabled = false;
        }
    }

    private void JumpingSFX()
    {
        if (playerController.CurrentPlayerVelocity().y > 0f && !playerController.IsGrounded())
        {
            jumpAudioSource.enabled = true;
        }
        else
        {
            jumpAudioSource.enabled = false;
        }
    }
}
