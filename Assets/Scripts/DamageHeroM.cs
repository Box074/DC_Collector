using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DamageHeroM : MonoBehaviour
{
    public int damageDealt;
    public int hazardType;
    public bool shadowDashHazard;
    private Component dh = null;
    private void OnEnable()
    {
        if (!Application.isEditor && dh == null)
        {
            Type dht = null;
            foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
            {
                dht = v.GetType("HealthManager");
                if (dht != null) break;
            }
            if (dht != null)
            {
                var d = dht.GetRuntimeField("damageDealt");
                var ht = dht.GetRuntimeField("hazardType");
                var sd = dht.GetRuntimeField("shadowDashHazard");
                dh = gameObject.AddComponent(dht);
                d.SetValue(dh, damageDealt);
                ht.SetValue(dh, hazardType);
                sd.SetValue(dh, shadowDashHazard);
            }
        }
    }
}
