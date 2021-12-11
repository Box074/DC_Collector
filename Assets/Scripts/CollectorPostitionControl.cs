using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorPostitionControl : MonoBehaviour
{
    public Collider2D col;
    public CollectorControl collector;
    public bool colliderOn;
    public bool testGround;
    public bool canSetRigidbody;
    public bool keepOnLand;
    public bool HitTop => hit.HasFlag(HitFlags.Top);
    public bool HitLand => hit.HasFlag(HitFlags.Land);
    public bool HitLeft => hit.HasFlag(HitFlags.Left);
    public bool HitRight => hit.HasFlag(HitFlags.Right);
    public HitFlags hit;
    public bool isSleep;
    public enum HitFlags{
        Right = 1, Left = 2, Top = 4, Land = 8
    }
    public void WakeUp()
    {
        isSleep = false;
    }
    public void Sleep(float time = 0)
    {
        isSleep = true;
        if(time != 0)
        {
            Invoke("WakeUp", time);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        CollectorControl control = other.gameObject.GetComponentInParent<CollectorControl>();
        if (control != null)
        {
            collector = control;
            control.postitionControl = this;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        OnTriggerEnter2D(other.collider);
    }
    private void Update()
    {
        hit = (HitFlags)0;
        if (col != null && collector != null && !isSleep)
        {
            if (collector.postitionControl == this)
            {
                if (!colliderOn)
                {
                    Vector2 max = col.bounds.max;
                    Vector2 min = col.bounds.min;
                    Vector3 cs = collector.transform.position;
                    cs.x = cs.x > max.x ? max.x : (cs.x < min.x ? min.x : cs.x);
                    if (testGround) cs.y = cs.y > max.y ? max.y : (cs.y < min.y ? min.y : cs.y);
                    collector.transform.position = cs;
                }
                else
                {
                    Vector2 max = col.bounds.max;
                    Vector2 min = col.bounds.min;
                    Vector3 cp = collector.transform.position;
                    Bounds cb = collector.col.bounds;
                    Vector2 rv = collector.rig.velocity;
                    if (cb.min.x < min.x)
                    {
                        cp.x += min.x - cb.min.x;
                        rv.x = rv.x < 0 ? 0 : rv.x;
                        hit |= HitFlags.Left;
                    }
                    else if (cb.max.x > max.x)
                    {
                        cp.x -= cb.max.x - max.x;
                        rv.x = rv.x > 0 ? 0 : rv.x;
                        hit |= HitFlags.Right;
                    }
                    if (testGround)
                    {
                        if (cb.min.y < min.y || keepOnLand)
                        {
                            cp.y += min.y - cb.min.y;
                            rv.y = rv.y < 0 ? 0 : rv.y;
                            hit |= HitFlags.Land;
                        }
                        else if (cb.max.y > max.y)
                        {
                            cp.y -= cb.max.y - max.y;
                            rv.y = rv.y >0 ? 0 : rv.y;
                            hit |= HitFlags.Top;
                        }
                    }
                    collector.transform.position = cp;
                    if(canSetRigidbody) collector.rig.velocity = rv;
                }

            }
        }
    }
}
