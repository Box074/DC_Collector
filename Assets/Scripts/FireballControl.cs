using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballControl : MonoBehaviour
{
    public Vector2 dir;
    public Rigidbody2D rig;
    private void FixedUpdate(){
        rig.velocity = dir;
    }
    private void OnEnable() {
        Invoke("DestroyThis", 1.5f);
    }
    private void DestroyThis()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == 8)
        {
            Destroy(gameObject);
        }
    }
}
