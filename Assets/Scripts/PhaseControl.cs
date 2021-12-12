using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseControl : MonoBehaviour
{
    [Serializable]
    public class PhaseTest
    {
        public int needDrinkCount;
        public int maxHealth;
    }
    public CollectorControl control;
    public PhaseTeleportPoint[] phasePos;
    public PhaseTest[] phaseTests;
    public int currentPhase;
    void Update()
    {
        if (phaseTests != null)
        {
            for (int i = 0; i < phaseTests.Length; i++)
            {
                var pt = phaseTests[i];
                if (control.drinkCount >= pt.needDrinkCount)
                {
                    if (control.hp <= pt.maxHealth)
                    {
                        if(control.phase < i) control.phase = i;
                    }
                }
            }
        }
        if (phasePos != null)
        {
            if (phasePos.Length != 0)
            {
                control.phase = Mathf.Clamp(control.phase, 0, phasePos.Length - 1);
                if (currentPhase != control.phase)
                {
                    currentPhase = control.phase;
                    control.willTeleport = true;
                    control.targetPos = phasePos[currentPhase].bossPos.transform.position;
                    control.heroPos = phasePos[currentPhase].heroPos.transform.position;
                }
            }
        }
    }
}
