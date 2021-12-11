using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorControl : MonoBehaviour
{
    public float fireballSpeed;
    public GameObject fireballSP;
    public GameObject fireballPF;
    public EffectAnim dashEffect;
    public CollectorPostitionControl postitionControl;
    public Action OnDeath;
    public GameObject target;
    public CollectorAnim animControl;
    public Collider2D col;
    public ColliderControl cc;
    public EffectAnim laserBeam;
    public EffectAnim laserLoad;
    public Rigidbody2D rig;
    public bool facingRight;
    public float dashSpeed;
    public float spinSpeed;
    public float walkSpeed;
    public bool isWalking;
    public bool canIdle;
    public bool canFall;
    public int drinkCount;
    public int maxDrinkCount;
    public int hp;
    public int hitCount;
    public int stunHitCount;
    public int maxHP;
    public int phase;
    public bool canDamage;
    public bool lastHit;
    public bool willTeleport;
    public Vector3 targetPos;
    public Vector3 heroPos;
    Coroutine testC;

    IEnumerator Stun()
    {
        if (hitCount < stunHitCount) yield break;
        postitionControl.colliderOn = true;
        hitCount = 0;
        animControl.PlayLoop("stun");
        yield return new WaitForSeconds(1.5f);
        yield return animControl.PlayWait("idle");
    }
    IEnumerator Fall(bool canLand = true)
    {
        if (postitionControl.HitLand || !canFall) yield break;
        rig.isKinematic = false;
        postitionControl.colliderOn = true;
        postitionControl.testGround = true;
        canIdle = false;
        rig.velocity = new Vector2(0, -10);
        yield return null;
        while (rig.velocity.y < 0 && !postitionControl.HitLand)
        {
            if (!animControl.IsPlaying("fall"))
            {
                animControl.PlayLoop("fall");
            }
            rig.velocity = new Vector2(0, -10);
            yield return null;
        }
        canIdle = true;
        postitionControl.keepOnLand = true;
        if (canLand)
        {
            animControl.Play("land");

            yield return animControl.Wait();
        }
        
    }
    IEnumerator Walk()
    {
        if (!isWalking) yield break;

        if (facingRight)
        {
            if (postitionControl.HitRight)
            {
                TurnLeft();
                yield break;
            }
        }
        else
        {
            if (postitionControl.HitLand)
            {
                TurnRight();
                yield break;
            }
        }
        postitionControl.colliderOn = true;
        postitionControl.testGround = true;
        canIdle = false;
        animControl.PlayLoop("walk");
        float speed = facingRight ? walkSpeed : -walkSpeed;
        while (animControl.IsPlaying("walk") && isWalking)
        {
            if (facingRight)
            {
                if (postitionControl.HitRight) break;
            }
            else
            {
                if (postitionControl.HitLeft) break;
            }
            rig.velocity = new Vector2(speed, 0);
            if (canFall)
            {
                yield return Fall();
                if (animControl.currentAnim == "land")
                {
                    animControl.PlayLoop("walk");
                }
            }
            yield return null;
        }
        if (animControl.IsPlaying("walk"))
        {
            animControl.loop = false;
            while (animControl.IsPlaying("walk"))
            {
                rig.velocity = new Vector2(speed, 0);
                yield return null;
            }
        }
        rig.velocity = Vector2.zero;
        canIdle = true;
    }
    IEnumerator Teleport()
    {
        if (!willTeleport) yield break;
        if (postitionControl) postitionControl.colliderOn = false;
        rig.isKinematic = true;
        col.enabled = false;
        yield return animControl.PlayWait("idleTeleport");
        rig.velocity = Vector2.zero;
        animControl.PlayLoop("teleport");
        float st = Time.time;
        float span = 0;
        while ((span = Time.time - st) <= 2.5f)
        {
            Color c = new Color(1, 1, 1, Mathf.Lerp(1, 0, span * 0.4f));
            if (target != null)
            {
                SpriteRenderer r = target.GetComponent<SpriteRenderer>();
                if (r != null)
                {
                    r.color = c;
                }
                else
                {
                    var hero_renderer = target.GetComponent("tk2dSprite");
                    if (hero_renderer != null)
                    {
                        PropertyInfo p = hero_renderer.GetType()
                            .GetProperty("color");
                        p.SetValue(p, c);
                    }
                }
            }
            animControl.render.color = c;
            yield return null;
        }
        animControl.render.color = Color.white;
        if (target != null)
        {
            SpriteRenderer r = target.GetComponent<SpriteRenderer>();
            if (r != null)
            {
                r.color = Color.white;
            }
            else
            {
                var hero_renderer = target.GetComponent("tk2dSprite");
                if (hero_renderer != null)
                {
                    PropertyInfo p = hero_renderer.GetType()
                        .GetProperty("color");
                    p.SetValue(p, Color.white);
                }
            }
        }

        postitionControl = null;
        transform.position = targetPos;
        if (target != null)
        {
            target.transform.position = heroPos;
        }
        yield return new WaitUntil(() => postitionControl == null);
        willTeleport = false;
        canFall = true;
        col.enabled = true;
        rig.isKinematic = false;
        yield return null;
        yield return Fall();
    }
    IEnumerator Drink()
    {

        if (hp > (int)(maxHP * 0.5f) || drinkCount > maxDrinkCount) yield break;
        postitionControl.colliderOn = false;
        canIdle = false;
        if (drinkCount < maxDrinkCount)
        {
            canDamage = false;
        }
        yield return animControl.PlayWait("idleDrink");
        drinkCount++;
        animControl.PlayLoop("drink");
        maxHP = maxHP + (maxHP / 2);
        int addhp = maxHP - hp;
        yield return new WaitForSeconds(3.5f);
        hp = hp + addhp;
        if (hp > maxHP) hp = maxHP;

        yield return animControl.PlayWait("drinkIdle");
        canDamage = true;
        animControl.Play("idle");
        canIdle = true;
    }
    public void TurnLeft()
    {
        facingRight = false;
        transform.localScale = new Vector3(-1, 1, 1);
    }
    public void TurnRight()
    {
        facingRight = true;
        transform.localScale = new Vector3(1, 1, 1);
    }
    public void TurnToTarget()
    {
        if (target == null) return;
        if (target.transform.position.x > transform.position.x)
        {
            TurnRight();
        }
        else
        {
            TurnLeft();
        }
    }

    IEnumerator Attack()
    {
        canIdle = false;
        postitionControl.colliderOn = false;
        for (int i = 0; i < UnityEngine.Random.Range(1, 3); i++)
        {
            TurnToTarget();
            rig.isKinematic = true;
            yield return animControl.PlayWait("idleAtkLoad");
            yield return animControl.PlayWait("atkLoad");

            yield return animControl.PlayWait("atk");
            rig.isKinematic = false;
        }
        canIdle = true;
    }
    IEnumerator Dash()
    {
        canFall = false;
        canIdle = false;

        postitionControl.colliderOn = true;

        yield return animControl.PlayWait("idleDashLoad");

        TurnToTarget();
        yield return animControl.PlayWait("dashLoad");
        rig.isKinematic = true;
        yield return null;


        animControl.PlayLoop("dash");
        dashEffect.gameObject.SetActive(true);
        rig.velocity = new Vector2(facingRight ? dashSpeed : -dashSpeed, 0);
        yield return null;
        while (rig.velocity.x != 0 && dashEffect.gameObject.activeSelf)
        {
            yield return null;
        }
        rig.velocity = Vector2.zero;
        rig.isKinematic = false;
        yield return animControl.PlayWait("dashIdle");
        canFall = true;
        canIdle = true;
    }
    IEnumerator Spin()
    {
        canIdle = false;
        canFall = false;
        postitionControl.colliderOn = true;
        postitionControl.testGround = true;
        yield return animControl.PlayWait("idleSpinLoad");
        yield return animControl.PlayWait("spinLoad");
        animControl.PlayLoop("spin");
        float st = Time.time;
        float spinTime = UnityEngine.Random.Range(3.5f, 5.5f);
        yield return null;
        while (Time.time - st < spinTime)
        {
            if (facingRight)
            {
                rig.velocity = new Vector2(spinSpeed, 0);
                if (postitionControl.HitRight)
                {
                    postitionControl.Sleep(0.15f);
                    TurnLeft();
                }
            }
            else
            {
                rig.velocity = new Vector2(-spinSpeed, 0);
                if (postitionControl.HitLeft)
                {
                    postitionControl.Sleep(0.15f);
                    TurnRight();
                }
            }

            yield return null;
        }
        rig.velocity = Vector2.zero;
        animControl.PlayR("idleSpinLoad", 16);
        yield return animControl.Wait("idleSpinLoad");
        canIdle = true;
        canFall = true;
    }
    IEnumerator Death()
    {
        postitionControl.colliderOn = false;
        canIdle = false;
        canFall = false;
        animControl.Stop();
        yield return animControl.PlayWait("lethalHit");
        yield return animControl.PlayWait("lethalHitDeathIdle");
        animControl.PlayLoop("deathIdle");
        OnDeath?.Invoke();
        Destroy(this);
    }
    IEnumerator Fireball(){
        postitionControl.keepOnLand = true;
        postitionControl.testGround = true;
        yield return animControl.PlayWait("idleBall");
        animControl.PlayLoop("ball");
        for(int i = 0; i< UnityEngine.Random.Range(10, 25); i++)
        {
            GameObject fb = Instantiate(fireballPF);
            if (!Application.isEditor) fb.layer = 11;
            fb.transform.position = fireballSP.transform.position;
            var fbc = fb.GetComponent<FireballControl>();
            var tp = target.transform.position - fb.transform.position;
            if(Mathf.Abs(tp.x) > Mathf.Abs(tp.y))
            {
                var x = Mathf.Abs(fireballSpeed / tp.x);
                tp.x = tp.x * x;
                tp.y = tp.y * x;
            }else{
                var x = Mathf.Abs(fireballSpeed / tp.y);
                tp.y = tp.y * x;
                tp.x = tp.x * x;
            }
            fbc.dir = tp;
            yield return new WaitForSeconds(0.15f);
        }
    }
    IEnumerator Laser()
    {
        postitionControl.colliderOn = true;
        postitionControl.testGround = true;
        rig.isKinematic = true;
        canIdle = false;
        animControl.Play("idleLaserLoad");
        yield return null;
        while (animControl.currentAnimFrame < 16) yield return null;
        rig.velocity = new Vector2(0, 1f);
        yield return animControl.Wait();
        rig.velocity = Vector2.zero;
        animControl.PlayLoop("laserLoad");
        laserLoad.gameObject.SetActive(true);
        for (int i = 0; i < UnityEngine.Random.Range(3, 5); i++)
        {
            float bt = Time.time;
            float wt = UnityEngine.Random.Range(0.15f, 0.25f);
            while (
                Mathf.Abs(transform.position.x - target.transform.position.x) > 0.5f ||
                transform.position.y - target.transform.position.y < 3
                )
            {
                if(Time.time - bt > 3) break;
                transform.position = Vector2.MoveTowards(transform.position, 
                target.transform.position + new Vector3(0, 3, 0),
                    0.1f);
                yield return null;
            }
            yield return new WaitForSeconds(wt);
            animControl.Play("laserBeam");
            yield return animControl.WaitToFrame(5);
            laserLoad.gameObject.SetActive(false);
            laserBeam.gameObject.SetActive(true);
            laserBeam.Play("fxLaserBeam");
            laserBeam.currentAnimFrame = 5;
            yield return null;
            yield return laserBeam.Wait();
            laserBeam.gameObject.SetActive(false);
            laserLoad.gameObject.SetActive(true);
            animControl.PlayLoop("laserLoad");
        }
        laserBeam.gameObject.SetActive(false);
        laserLoad.gameObject.SetActive(false);
        rig.isKinematic = false;
        canFall = true;
        yield return Fall(false);

        animControl.PlayR("idleLaserLoad", 18);

        yield return animControl.Wait();
        canIdle = true;
    }
    IEnumerator ChooseAtk0()
    {
        postitionControl.keepOnLand = true;
        int r = UnityEngine.Random.Range(0, 20);
        if (r < 12)
        {
            yield return Dash();
        }
        else if (r < 15 && !lastHit)
        {
            yield return Spin();
        }
        else if (r < 20)
        {
            if (Mathf.Abs(target.transform.position.x - transform.position.x) > 3) yield break;
            yield return Attack();
        }
    }
    IEnumerator ChooseAtk1()
    {
        postitionControl.keepOnLand = false;
        int r = UnityEngine.Random.Range(0, 20);
        if(r < 10) yield return Laser();
        else yield return Fireball();
        postitionControl.keepOnLand = true;
    }
    IEnumerator AttackChoose()
    {
        TurnToTarget();
        switch (phase)
        {
            case 0:
                yield return ChooseAtk0();
                break;
            case 1:
                yield return ChooseAtk1();
                break;
            default:
                if (UnityEngine.Random.Range(0, 3) <= 1 || lastHit)
                {
                    yield return ChooseAtk0();
                }
                else
                {
                    yield return ChooseAtk1();
                }
                break;
        }
        animControl.PlayLoop("idle");
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1.5f));
    }
    IEnumerator Test()
    {
        while (true)
        {
            yield return Fall();
            yield return Teleport();
            yield return Walk();
            yield return Drink();
            yield return AttackChoose();

            yield return null;
        }
    }
    // Update is called once per frame
    void OnEnable()
    {
        if(!Application.isEditor) gameObject.layer = 11;
    }
    public void WakeUp()
    {
        testC = StartCoroutine(Test());
    }
    void Update()
    {
        if (canIdle)
        {
            if (animControl.isStop)
            {
                animControl.PlayLoop("idle");
            }
        }
        if (hp <= 0 && drinkCount > maxDrinkCount)
        {
            if (!lastHit)
            {
                lastHit = true;
                hp = 1;
            }
            else if (hp != -1000)
            {
                hp = -1000;
                StopCoroutine(testC);
                StartCoroutine(Death());
            }
        }

        return;
    }
}
