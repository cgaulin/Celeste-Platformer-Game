using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float springForce;
    [SerializeField] private Animator anim;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player.GetComponent<Rigidbody2D>().velocity = Vector2.up * springForce;
            anim.SetBool("ReleaseSpring", true);
        }
    }

    public void ResetSpring()
    {
        anim.SetBool("ReleaseSpring", false);
    }
}
