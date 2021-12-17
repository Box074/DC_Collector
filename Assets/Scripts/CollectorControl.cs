using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorControl : MonoBehaviour
{
    public float fireballSpeed;
    public AudioSource hitAudio;
    public DamageHero dh;
    public Vector4 hitOffset;
    public AudioPlayer audioPlayer;
    public GameObject golemsG;
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
    public HealthManager hm;
    public bool facingRight;
    public float dashSpeed;
    public float spinSpeed;
    public float walkSpeed;
    public bool isWalking;
    public bool canIdle;
    public bool canFall;
    public bool IsLastPhase => drinkCount >= maxDrinkCount;
    public int drinkCount;
    public int maxDrinkCount;
    public int hp;
    public int hitCount;
    public int stunHitCount;
    public int maxHP;
    public int phase;
    public bool lastHit;
    public bool willTeleport;
    public bool canDamage;
    public bool canLastHit;
    public bool firstHit;
    public bool em_showHitEffect;
    public Vector3 targetPos;
    public Vector3 heroPos;
    bool enterLastPhase;
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
        if (postitionControl.HitLand || !canFall || postitionControl.keepOnLand) yield break;
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
            animControl.mainRenderer.color = c;
            yield return null;
        }
        animControl.mainRenderer.color = Color.white;
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
        NoDamage();
        postitionControl.colliderOn = false;
        canIdle = false;
        if (!IsLastPhase)
        {
            canDamage = false;
        }
        audioPlayer.Play(audioPlayer.potion_grab);
        yield return animControl.PlayWait("idleDrink");
        drinkCount++;
        audioPlayer.PlayLoop(audioPlayer.potion_drink_loop);
        animControl.PlayLoop("drink");
        maxHP = maxHP + (maxHP / 4);

        int addhp = maxHP - hp;
        yield return new WaitForSeconds(3.5f);
        audioPlayer.Stop();
        hp = hp + addhp;
        if (hp > maxHP) hp = maxHP;

        yield return animControl.PlayWait("drinkIdle");
        canDamage = true;
        animControl.PlayLoop("idle");
        if(IsLastPhase && !enterLastPhase)
        {
            enterLastPhase = true;
            golemsG.BroadcastMessage("GOLEM_SETDAMAGE", 99999, SendMessageOptions.DontRequireReceiver);
            yield return new WaitForSeconds(0.75f);
            yield return Stomp();
        }
        canIdle = true;
    }
    public void TurnLeft()
    {
        facingRight = false;
        var s = transform.localScale;
        s.x = -Mathf.Abs(s.x);
        transform.localScale = s;
    }
    public void TurnRight()
    {
        facingRight = true;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
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
            audioPlayer.Play(audioPlayer.energy_charge);
            animControl.ftScale = 0.35f;
            yield return animControl.PlayWait("idleAtkLoad");
            yield return animControl.PlayWait("atkLoad");
            SetDamage(2);
            animControl.ftScale = 0.5f;
            audioPlayer.Play(audioPlayer.energy_release);
            yield return animControl.PlayWait("atk");
            NoDamage();
            animControl.ftScale = 1;
            rig.isKinematic = false;
        }
        canIdle = true;
    }
    IEnumerator Dash()
    {
        canFall = false;
        canIdle = false;

        postitionControl.colliderOn = true;
        animControl.ftScale = 0.5f;
        yield return animControl.PlayWait("idleDashLoad");

        TurnToTarget();
        yield return animControl.PlayWait("dashLoad");
        animControl.ftScale = 1;
        rig.isKinematic = true;
        yield return null;

        audioPlayer.Play(audioPlayer.dash_release);
        SetDamage(1);
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
        NoDamage();
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
        audioPlayer.Play(audioPlayer.spin_charge);
        yield return animControl.PlayWait("idleSpinLoad");
        SetDamage(0.5f);
        yield return animControl.PlayWait("spinLoad");
        animControl.PlayLoop("spin");
        audioPlayer.PlayLoop(audioPlayer.spin_release);
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
        animControl.ftScale = 0.5f;
        audioPlayer.Stop();
        NoDamage();
        animControl.PlayR("idleSpinLoad", 16);
        yield return animControl.Wait("idleSpinLoad");
        animControl.ftScale = 1;
        canIdle = true;
        canFall = true;
    }
    IEnumerator Death()
    {
        NoDamage();
        rig.velocity = Vector2.zero;
        animControl.ftScale = 4.5f;
        rig.isKinematic = true;
        postitionControl.colliderOn = false;
        canIdle = false;
        canFall = true;
        animControl.Stop();
        col.enabled = false;
        golemsG.BroadcastMessage("GOLEM_DOWN", SendMessageOptions.DontRequireReceiver);
        audioPlayer.Play(audioPlayer.die);
        yield return animControl.PlayWait("lethalHit");
        animControl.ftScale = 2;
        hm.isDead = true;
        yield return animControl.PlayWait("lethalHitDeathIdle");
        animControl.PlayLoop("deathIdle");
        OnDeath?.Invoke();
    }
    IEnumerator Fireball()
    {
        postitionControl.keepOnLand = true;
        postitionControl.testGround = true;
        yield return animControl.PlayWait("idleBall");
        animControl.PlayLoop("ball");
        for (int i = 0; i < UnityEngine.Random.Range(5, 14); i++)
        {
            audioPlayer.Play(audioPlayer.fire_release);
            GameObject fb = Instantiate(fireballPF);
            if (!Application.isEditor) fb.layer = 11;
            fb.transform.position = fireballSP.transform.position;
            var fbc = fb.GetComponent<FireballControl>();
            var tp = target.transform.position - fb.transform.position;
            if (Mathf.Abs(tp.x) > Mathf.Abs(tp.y))
            {
                var x = Mathf.Abs(fireballSpeed / tp.x);
                tp.x = tp.x * x;
                tp.y = tp.y * x;
            }
            else
            {
                var x = Mathf.Abs(fireballSpeed / tp.y);
                tp.y = tp.y * x;
                tp.x = tp.x * x;
            }
            fbc.dir = tp;
            yield return new WaitForSeconds(0.25f);
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
        rig.velocity = new Vector2(0, 3f);
        yield return animControl.Wait();
        rig.velocity = Vector2.zero;
        animControl.PlayLoop("laserLoad");
        SetDamage(1);
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
                if (!postitionControl.HitTop && !postitionControl.HitLeft && !postitionControl.HitRight)
                    bt = Time.time;
                if (Time.time - bt > 0.75f) break;
                transform.position = Vector2.MoveTowards(transform.position,
                target.transform.position + new Vector3(0, 3, 0),
                    0.2f);
                yield return null;
            }
            yield return new WaitForSeconds(wt);
            animControl.Play("laserBeam");
            yield return animControl.WaitToFrame(5);
            audioPlayer.Play(audioPlayer.laser_release);
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
        NoDamage();
        yield return Fall(false);

        animControl.PlayR("idleLaserLoad", 18);

        yield return animControl.Wait();
        canIdle = true;
    }
    IEnumerator SPAWN_FFB(float delay)
    {
        yield return new WaitForSeconds(delay);
        float x = UnityEngine.Random.Range(postitionControl.col.bounds.min.x,
            postitionControl.col.bounds.max.x);
        float y = postitionControl.col.bounds.max.y + 5;
        var p = new Vector2(x, y);
        var fb = Instantiate(fireballPF);
        fb.transform.position = p;
        var fbc = fb.GetComponent<FireballControl>();
        fbc.isFalling = true;
        fbc.dir = new Vector2(0, -5);
    }
    IEnumerator Stomp()
    {
        dh.damageDealt = drinkCount + 1;
        animControl.ftScale = 1.5f;
        postitionControl.keepOnLand = false;
        postitionControl.colliderOn = true;
        SetDamage(0.5f);
        animControl.Play("stomp");
        yield return animControl.WaitToFrame(16);
        rig.velocity = new Vector2(0, 5);
        yield return animControl.WaitToFrame(28);
        rig.velocity = new Vector2(0, -8);
        yield return null;
        postitionControl.colliderOn = true;
        while (!postitionControl.HitLand) yield return null;
        postitionControl.keepOnLand = true;
        golemsG.BroadcastMessage("GOLEM_UP", SendMessageOptions.DontRequireReceiver);
        for(int i = 0; i< UnityEngine.Random.Range(1,4); i++)
        {
            StartCoroutine(SPAWN_FFB(UnityEngine.Random.Range(0.15f, 1)));
        }
        NoDamage();
        yield return new WaitForSeconds(0.75f);
        yield return animControl.PlayWait("stompIdle");
        if (!IsLastPhase)
        {
            animControl.PlayLoop("idle");
            yield return new WaitForSeconds(1.5f);
            golemsG.BroadcastMessage("GOLEM_DOWN", SendMessageOptions.DontRequireReceiver);
        }
        animControl.ftScale = 1;
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
        int r = UnityEngine.Random.Range(0, 30);
        if (r <= 10) yield return Laser();
        else if (r <= 20) yield return Fireball();
        else yield return Stomp();
        postitionControl.keepOnLand = true;
    }
    IEnumerator AttackChoose()
    {
        TurnToTarget();
        switch (drinkCount)
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
        NoDamage();
        animControl.PlayLoop("idle");
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1.5f));
    }
    IEnumerator Test()
    {
        while (true)
        {
            NoDamage();
            postitionControl.keepOnLand = true;
            yield return Walk();
            yield return Drink();
            yield return AttackChoose();

            yield return null;
        }
    }
    // Update is called once per frame
    void OnEnable()
    {
        hm.hp = int.MaxValue;
        canDamage = true;
    }
    public void WakeUp()
    {
        testC = StartCoroutine(Test());
    }
    private void HideHitEffect()
    {
        var mat = animControl.mainRenderer.material;
        mat.SetColor("_AColor", Color.black);
        mat.SetVector("_MPos", new Vector4(0, 0, 0, 1));
    }
    private void HitEffect()
    {
        var mat = animControl.mainRenderer.material;
        mat.SetColor("_AColor", Color.red);
        mat.SetVector("_MPos", hitOffset);
        hitAudio.Play();
        Invoke("HideHitEffect", 0.125f);
    }
    private void UpdateHP()
    {
        int h = int.MaxValue - hm.hp;
        if (canDamage && h > 0)
        {
            firstHit = true;
            hp -= h;
            HitEffect();
        }
        hm.hp = int.MaxValue;
    }
    public void SetDamage(float scale)
    {
        dh.damageDealt = Mathf.CeilToInt((drinkCount + 1) * scale);
    }
    public void NoDamage()
    {
        dh.damageDealt = 0;
    }
    private void EditModeTest()
    {
        if (em_showHitEffect)
        {
            em_showHitEffect = false;
            HitEffect();
        }
    }
    void Update()
    {
        if (Application.isEditor) EditModeTest();
        UpdateHP();
        if (canIdle)
        {
            if (animControl.isStop)
            {
                animControl.PlayLoop("idle");
            }
        }
        if (hp <= 0 && drinkCount > maxDrinkCount)
        {
            if (!lastHit && postitionControl.HitLand)
            {
                lastHit = true;
                hp = 1;
            }
            else if (hp != -1000 && !hm.isDead)
            {
                hp = -1000;
                canDamage = false;
                StopCoroutine(testC);
                StartCoroutine(Death());
            }
        }

        return;
    }
}
