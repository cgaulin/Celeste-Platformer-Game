using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawnPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetRespawnPoint(transform.position);
        }
    }
}
