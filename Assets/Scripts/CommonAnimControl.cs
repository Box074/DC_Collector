using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CommonAnimControl : MonoBehaviour
{
    public SpriteAtlas atlas;
    public SpriteRenderer render;
    public float everyFrameTime;
    public int currentAnimFrame;
    public string currentAnim;
    public bool isStop;
    public bool loop;
    public bool autoPlay;
    public bool isPause;
    public bool r;
    float lastFrameTime;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (autoPlay)
        {
            isStop = false;
            currentAnimFrame = 0;
        }
    }
    void Start(){
        render.material = new Material(Shader.Find("Sprites/Default"));
    }
    protected virtual string GetSpriteName()
    {
        return $"{currentAnim}-=-{currentAnimFrame}-=-";
    }
    protected virtual void OnUpdateSprite()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isStop || isPause) return;
        if (Time.time - lastFrameTime < everyFrameTime) return;
        lastFrameTime = Time.time;
        string sn = GetSpriteName();
        Sprite sprite = atlas.GetSprite(sn);
        if (sprite == null)
        {
            if (!loop || currentAnimFrame == 0 || r)
            {
                isStop = true;
                currentAnimFrame = 0;
            }
            else
            {
                isStop = false;
                currentAnimFrame = 0;
                Update();
            }
            return;
        }
        render.sprite = sprite;
        if (r) currentAnimFrame--;
        else currentAnimFrame++;
        OnUpdateSprite();
    }

    public void Play(string name, bool stopAndPlay = true)
    {
        r = false;
        isPause = false;
        loop = false;
        isStop = false;
        currentAnim = name;
        currentAnimFrame = 0;
    }
    public void PlayR(string name, int frame)
    {
        Play(name);
        currentAnimFrame = frame;
        r = true;
    }
    public bool IsPlaying(string name)
    {
        return currentAnim == name && !isStop && !isPause;
    }

    public void PlayLoop(string name)
    {
        Play(name);
        loop = true;
    }
    public void Stop()
    {
        currentAnimFrame = 0;
        isStop = true;
        isPause = false;
        r = false;
    }
    public IEnumerator PlayWait(string name)
    {
        Play(name, true);
        isStop = false;
        yield return Wait();
    }
    public IEnumerator Wait()
    {
        while (!isStop && !loop) yield return null;
    }
    public IEnumerator Wait(string name)
    {
        while (IsPlaying(name)) yield return null;
    }
    public IEnumerator WaitToFrame(int tf)
    {
        while(currentAnimFrame < tf && !isStop) yield return null;
    }
    public void Pause()
    {
        isPause = true;
    }
    public void Play()
    {
        r = false;
        if (isPause)
        {
            isPause = false;
        }
        else
        {
            Play(currentAnim, true);
        }
    }
}
