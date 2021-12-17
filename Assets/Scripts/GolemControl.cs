using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemControl : MonoBehaviour
{
    public Collider2D col;
    public DamageHero dh;
    private bool isUp = false;
    private void OnEnable()
    {
        isUp = false;
        transform.localScale = Vector3.zero;
    }
    public void GOLEM_SETDAMAGE(int d)
    {
        dh.damageDealt = d;
    }
    public void GOLEM_UP()
    {
        if(!isUp) StartCoroutine(e_Up());
    }
    public void GOLEM_DOWN()
    {
        if(isUp) StartCoroutine(e_Down());
    }
    IEnumerator e_Up(){
        col.enabled = false;
        
        yield return null;
        Vector3 scale = new Vector3(1, 0, 1);
        gameObject.transform.localScale = scale;
        for(; scale.y < 0.99f ; scale.y += 0.3f)
        {
            gameObject.transform.localScale = scale;
            yield return new WaitForFixedUpdate();
        }
        gameObject.transform.localScale = Vector3.one;
        col.enabled = true;
        isUp = true;
    }
    IEnumerator e_Down(){
        col.enabled = false;
        yield return null;
        Vector3 scale = Vector3.one;
        gameObject.transform.localScale = scale;
        for (; scale.y > 0; scale.y -= 0.1f)
        {
            gameObject.transform.localScale = scale;
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.transform.localScale = Vector3.zero;
        isUp = false;
    }
}
