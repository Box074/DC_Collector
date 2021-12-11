using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundControl : MonoBehaviour
{
    public GameObject[] d_group;
    public bool wakeUp;
    public void WakeGroupUp()
    {
        foreach(var v in d_group)
        {
            for(int i = 0;i< v.transform.childCount; i++)
            {
                v.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    void Update()
    {
        if(wakeUp)
        {
            wakeUp = false;
            WakeGroupUp();
        }
    }
}
