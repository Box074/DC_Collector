using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CollectorAnim : CommonAnimControl
{
	public ColliderControl cc;
    protected override void OnUpdateSprite()
    {
        cc.UpdateCollider();
    }
}
