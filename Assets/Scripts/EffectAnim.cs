using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnim : CommonAnimControl
{
    protected override string GetSpriteName()
    {
        string spr = currentAnimFrame.ToString();
        if(spr.Length < 4)
        {
            spr = string.Concat(new string('0', 4 - spr.Length), spr);
        }
        return $"{currentAnim}_{spr}-=-0-=-";
    }
}
