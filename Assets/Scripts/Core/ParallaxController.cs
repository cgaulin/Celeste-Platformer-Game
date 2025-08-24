using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private Transform cam; //Main Camera
    private Vector3 camStartPos;
    private float distance; //Distance between camera start position and current position

    private GameObject[] backgrounds;
    private Material[] mats;
    private float[] backSpeed;

    private float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount; //number of backgrounds
        mats = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mats[i] = backgrounds[i].GetComponent<Renderer>().material;
        }
        BackSpeedCalculate(backCount);
    }

    private void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) //finds the furthest background
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) //set the speed of the backgrounds
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x, cam.position.y, 0f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mats[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
