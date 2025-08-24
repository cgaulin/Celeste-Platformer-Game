using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashCrystalManager : MonoBehaviour
{
    private void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).gameObject.activeInHierarchy)
            {
                StartCoroutine(RespawnDashCrystalRoutine(i));
            }
        }
    }

    private IEnumerator RespawnDashCrystalRoutine(int index)
    {
        yield return new WaitForSeconds(1f);
        transform.GetChild(index).gameObject.SetActive(true);
    }
}
