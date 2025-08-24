using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoEffect : MonoBehaviour
{
    private float timeBetweenSpawns;
    public float startTimeBetweenSpawns;

    public GameObject echoJump, echoFall;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerController.IsFacingRight() && playerController.GetMoveVector().y > 0f)
        {
            if (playerController.isClimbing && playerController.sprRenderer.flipX)
            {
                SpawnLeftJump();
            }
            else
            {
                SpawnRightJump();
            }
        }
        else if (playerController.IsFacingRight() && playerController.GetMoveVector().y <= 0f)
        {
            if (playerController.isClimbing && playerController.sprRenderer.flipX)
            {
                SpawnLeftFall();
            }
            else
            {
                SpawnRightFall();
            }
        }

        if (!playerController.IsFacingRight() && playerController.GetMoveVector().y > 0f)
        {
            if (playerController.isClimbing && playerController.sprRenderer.flipX)
            {
                SpawnRightJump();
            }
            else
            {
                SpawnLeftJump();
            }
        }
        else if (!playerController.IsFacingRight() && playerController.GetMoveVector().y <= 0f)
        {
            if (playerController.isClimbing && playerController.sprRenderer.flipX)
            {
                SpawnRightFall();
            }
            else
            {
                SpawnLeftFall();
            }
        }

    }

    private void SpawnRightJump()
    {
        if (timeBetweenSpawns <= 0f && playerController.IsDashing())
        {
            //Spawn echo game object
            GameObject instance = Instantiate(echoJump, transform.position, Quaternion.identity);
            Destroy(instance, 3f);
            timeBetweenSpawns = startTimeBetweenSpawns;
        }
        else
        {
            timeBetweenSpawns -= Time.deltaTime;
        }
    }

    private void SpawnRightFall()
    {
        if (timeBetweenSpawns <= 0f && playerController.IsDashing())
        {
            //Spawn echo game object
            GameObject instance = Instantiate(echoFall, transform.position, Quaternion.identity);
            Destroy(instance, 3f);
            timeBetweenSpawns = startTimeBetweenSpawns;
        }
        else
        {
            timeBetweenSpawns -= Time.deltaTime;
        }
    }

    private void SpawnLeftJump()
    {
        if (timeBetweenSpawns <= 0f && playerController.IsDashing())
        {
            //Spawn echo game object
            GameObject instance = Instantiate(echoJump, transform.position, Quaternion.Euler(0f, 180f, 0f));
            Destroy(instance, 3f);
            timeBetweenSpawns = startTimeBetweenSpawns;
        }
        else
        {
            timeBetweenSpawns -= Time.deltaTime;
        }
    }

    private void SpawnLeftFall()
    {
        if (timeBetweenSpawns <= 0f && playerController.IsDashing())
        {
            //Spawn echo game object
            GameObject instance = Instantiate(echoFall, transform.position, Quaternion.Euler(0f, 180f, 0f));
            Destroy(instance, 3f);
            timeBetweenSpawns = startTimeBetweenSpawns;
        }
        else
        {
            timeBetweenSpawns -= Time.deltaTime;
        }
    }
}
