using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashCrystal : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerController.currentDashCount == 1) { return; }
        if (other.CompareTag("Player"))
        {
            playerController.currentDashCount = 1;
            gameObject.SetActive(false);
        }
    }
}
