using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Unity.VisualScripting.Member;

public class MusicMgr : Singleton<MusicMgr>
{
    [Header("关联的音效物体")]
    private GameObject backGrundMusic;
    private GameObject globalSound;

    [Header("音乐字段")]
    //改引用，实例跟着变；改值变量呢
    private float bKValue= 1;
    private float soundValue = 1;
     
    /// <summary>
    ///  UI访问属性接口
    /// </summary>
    public float BkValue
    {
        get { return bKValue; }
        set 
        {
            float valLimit = Mathf.Clamp01 (value);
            if(Mathf.Approximately (bKValue, valLimit)) return;
            bKValue = valLimit;
            foreach(var bgm in bGMDic)
            {
                bgm.Value.volume = valLimit;
            }
        }
    }
    public float SoundValue
    {
        get { return soundValue; }
        set
        {
            float valLimit = Mathf.Clamp01 (value);
            if(Mathf.Approximately(soundValue, valLimit)) return;
            foreach(var sound in soundDic)
            {
                sound.Value.volume = valLimit;
            }
            foreach(var list in soundInformDic)
            {
                foreach(var sound in list.Value)
                {
                    sound.volume = valLimit;
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if(backGrundMusic == null)
        {
            backGrundMusic = new GameObject("BackGrundMusic");
        }
        if(globalSound == null)
        {
            globalSound = new GameObject ("GlobalSound");
        }
        DontDestroyOnLoad (backGrundMusic);
        DontDestroyOnLoad (globalSound); 
    }
    public Dictionary<string, AudioSource> bGMDic = new Dictionary<string, AudioSource>();
    
    public Dictionary<string, AudioSource> soundDic = new Dictionary<string, AudioSource>();
    //一个dic保存分发出去的组件，用于通知
    public Dictionary<string, List<AudioSource>> soundInformDic = new Dictionary<string, List<AudioSource>> ();

    public void PlayBKMusic(string name)//不足：不能移除bgm 
    {
        
        if(bGMDic.ContainsKey (name))
        {
            foreach(var bgm in bGMDic)
            {
                if(bgm.Key != name)
                {
                    bgm.Value.enabled = false;
                }
            }
            bGMDic[name].Play();
        }
        else
        {
            ResMgr.Instance.LoadAsync<AudioClip> ("Music/Bgm/" + name, (audioClip) =>
            {
                if(bGMDic.ContainsKey (name))
                {
                    return;
                }
                else
                {//新加入一个bgm要让之前的bgm失效,首次创建
                    foreach(var bgm in bGMDic)
                    {
                        if(bgm.Key != name)
                        {
                            bgm.Value.enabled = false;
                        }
                    }
                    AudioSource music = backGrundMusic.AddComponent<AudioSource> ();
                    this.bGMDic.Add (name, music);
                    bGMDic[name].clip = audioClip;
                    bGMDic[name].volume = bKValue;

                    bGMDic[name].playOnAwake = false;
                    bGMDic[name].loop = true;
                    bGMDic[name].Play();
                }
            });
        }
    }

    public void AddSound(string name,bool isLoop,float is3D, GameObject obj)
    {//外部添加音效方法
        
        if(soundDic.ContainsKey (name))
        {
            //第二次在obj上添加audio，复制到这个audio中
            AudioSource objAudio = obj.AddComponent<AudioSource>();
            CopySourceParams (soundDic[name], objAudio);
            soundInformDic[name].Add (objAudio);
        }
        else
        {
            ResMgr.Instance.LoadAsync<AudioClip> ("Music/Sound/" + name, (audioClip) =>
            {
                if(soundDic.ContainsKey (name))
                {
                    AudioSource objAudio = obj.AddComponent<AudioSource> ();
                    CopySourceParams (this.soundDic[name], objAudio);
                    soundInformDic[name].Add(objAudio);
                }
                else
                {
                    AudioSource sound = globalSound.AddComponent<AudioSource>();
                    this.soundDic.Add(name, sound);
                    //参数，可扩展
                    soundDic[name].clip = audioClip;
                    soundDic[name].loop = isLoop;
                    soundDic[name].spatialBlend = is3D;
                    //固定属性
                    soundDic[name].volume = soundValue;
                    soundDic[name].playOnAwake = false;

                    AudioSource objAudio = obj.AddComponent<AudioSource> ();
                    CopySourceParams (this.soundDic[name], objAudio);
                    soundInformDic.Add (name,new List<AudioSource> () { objAudio });
                }
                //有返回值委托，发送端既发送了信息又接受了信息
            });
        }
    }

    private void CopySourceParams(AudioSource from, AudioSource to)
    {//外部添加音效专用组件信息转移方法
        to.clip = from.clip;
        to.volume = from.volume;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
        to.spatialBlend = from.spatialBlend;
        to.playOnAwake = from.playOnAwake;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    //public void ChangeBKValue(float value)
    //{

    //}

    //public void ChangeSoundValue(float value)
    //{

    //}
    protected void OnDestroy()
    {
        
        if(backGrundMusic != null) Destroy (backGrundMusic);
        if(globalSound != null) Destroy (globalSound);
        // 清空字典，释放引用
        bGMDic.Clear ();
        soundDic.Clear ();
        soundInformDic.Clear ();
    }
}
