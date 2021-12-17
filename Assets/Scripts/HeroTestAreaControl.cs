using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroTestAreaControl : MonoBehaviour
{
    public CollectorControl collectorControl;
    public CollectorAnim collectorAnim;
    public PhaseControl phaseControl;
    private bool wake = false;
    public int setPhaseTo = 0;
    public GameObject hero = null;
    private void OnEnable()
    {
        wake = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wake) return;
        if (other.gameObject.transform.root.gameObject.name == "Knight")
        {
            hero = other.gameObject;
        }
    }

    private void Update()
    {
        if (hero != null && !wake)
        {
            wake = true;
            StartCoroutine(WakeUp());
        }
    }

    private IEnumerator WakeUp()
    {
        yield return null;
        collectorControl.hm.hp = int.MaxValue;
        collectorControl.target = hero;
        collectorControl.canIdle = true;
        collectorAnim.PlayLoop("idle");
        phaseControl.currentPhase = setPhaseTo;
        float st = Time.time;
        while(Time.time - st < 5 && !collectorControl.firstHit) yield return null;
        yield return new WaitForSeconds(0.5f);
        collectorControl.WakeUp();
        gameObject.SetActive(false);
    }
}
