using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitControl : MonoBehaviour
{
    public CollectorControl control;
    int lastHp = 0;
    void Update()
    {
        if(control.hp < lastHp)
        {
            control.hitCount++;
        }
        lastHp = control.hp;
    }
}
