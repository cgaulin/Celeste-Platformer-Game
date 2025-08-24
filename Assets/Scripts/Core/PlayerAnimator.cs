using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private PlayerController playerController;
    private int currentState;

    private static readonly int Idle = Animator.StringToHash("PlayerOneIdle");
    private static readonly int Walk = Animator.StringToHash("PlayerOneWalk");
    private static readonly int Jump = Animator.StringToHash("PlayerOneJump");
    private static readonly int Fall = Animator.StringToHash("PlayerOneFall");
    private static readonly int Climb = Animator.StringToHash("PlayerOneClimb");
    private static readonly int ClimbIdle = Animator.StringToHash("PlayerOneClimbIdle");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        var state = GetState();

        if (state == currentState) return;
        anim.CrossFade(state, 0f, 0);
        currentState = state;
    }

    private int GetState()
    {
        if (playerController.IsGrounded())
        {
            return playerController.CurrentPlayerVelocity().x == 0f ? Idle : Walk;
        }
        else if (playerController.isClimbing)
        {
            return playerController.CurrentPlayerVelocity().y == 0f ? ClimbIdle : Climb;
        }

        return playerController.CurrentPlayerVelocity().y > 0f ? Jump : Fall;
    }
}
