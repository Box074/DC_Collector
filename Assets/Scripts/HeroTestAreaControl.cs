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
        collectorControl.target = hero;
        phaseControl.currentPhase = setPhaseTo;
        yield return collectorAnim.PlayWait("laugh");
        collectorControl.WakeUp();
        gameObject.SetActive(false);
    }
}
