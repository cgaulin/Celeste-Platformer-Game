using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;

    private Gamepad pad;

    private string currentControlScheme;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        //is current control scheme a gamepad
        if (currentControlScheme == "Gamepad")
        {
            pad = Gamepad.current;

            if (pad != null)
            {
                //Start rumble
                pad.SetMotorSpeeds(lowFrequency, highFrequency);

                //Stop rumble
                StartCoroutine(StopRumble(duration, pad));
            }
        }


    }

    private IEnumerator StopRumble(float duration, Gamepad pad)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //once duration is over
        pad.SetMotorSpeeds(0f, 0f);
    }

    public void SwitchControls(PlayerInput input)
    {
        currentControlScheme = input.currentControlScheme;
    }
}
