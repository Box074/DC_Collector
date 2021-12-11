using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class HealthMaker : MonoBehaviour
{
    public int maxHP;
    public CollectorControl control;
    private Component hm = null;
    private FieldInfo hpf = null;
    private void OnEnable()
    {
        if (!Application.isEditor && hm == null)
        {
			Type hmt = null;
			hm = GetComponent(hmt);
            if(hm != null) return;
            foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
            {
                hmt = v.GetType("HealthManager");
                if (hmt != null) break;
            }
            if (hmt != null)
            {
                hpf = hmt.GetRuntimeField("hp");
                hm = gameObject.AddComponent(hmt);
                hpf.SetValue(hm, maxHP);
            }
        }
    }
    private void Update() 
    {
        if(hm == null) return;
        int chp = (int)hpf.GetValue(hm);
        int c = Mathf.Clamp(maxHP - chp, 0, maxHP);
        if(c < maxHP && control.canDamage)
        {
            control.hp -= c;
        }
        hpf.SetValue(hm, maxHP);
    }
}
