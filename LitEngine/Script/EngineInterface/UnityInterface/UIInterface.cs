using UnityEngine;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class UIInterface : BehaviourInterfaceBase
        {
            [System.Serializable]
            public class UIAnimation
            {
                public AnimationClip AniClip;
                public bool isReverse = false;
                public float Speed = 1;
                [System.NonSerialized][HideInInspector]
                protected Animation mAnimation;
                protected AnimationEvent[] mEvents = new AnimationEvent[1];
                protected float mStartTime = 0;

                public void Init(Animation _ani,string _funname, UIInterface _ui)
                {
                    if (AniClip == null) return;
                    AniClip.legacy = true;
                    AniClip.wrapMode = WrapMode.Once;
                    mAnimation = _ani;

                    mEvents = new AnimationEvent[1];
                    mEvents[0] = new AnimationEvent();
                    mEvents[0].functionName = _funname;
                    mEvents[0].objectReferenceParameter = _ui;
                    
                    if (isReverse)
                    {
                        mEvents[0].time = 0;
                        mStartTime = AniClip.length;
                        Speed *= -1;
                    }
                    else
                    {
                        mEvents[0].time = AniClip.length;
                        mStartTime = 0;
                    }

                    if (mAnimation.GetClip(AniClip.name) == null)
                        mAnimation.AddClip(AniClip, AniClip.name);
                }

                protected void ResetCurClip()
                {
                    mAnimation.Stop();
                    AniClip.events = mEvents;
                    mAnimation[AniClip.name].speed = Speed;
                    mAnimation[AniClip.name].time = mStartTime;
                }

                public void Play()
                {
                    if (mAnimation == null || AniClip == null) return;
                    ResetCurClip();
                    mAnimation.Play(AniClip.name);
                }
            }
            public enum UIAniState
            {
                None = 0,
                ShowAni,
                HideAni,
            }

            [SerializeField]
            public UIAnimation UIShowAni = new UIAnimation();
            [SerializeField]
            public UIAnimation UIHideAni = new UIAnimation();
            protected UIAniState mState = UIAniState.None;
            protected Animation animat;
            #region 脚本初始化以及析构
            public UIInterface()
            {

            }

            override protected void InitParamList()
            {
                base.InitParamList();
            }

            override public void ClearScriptObject()
            {
                base.ClearScriptObject();
            }
            #endregion
            #region Unity 
            protected override void Awake()
            {
                base.Awake();

                animat = gameObject.AddComponent<Animation>();
                animat.playAutomatically = false;
                animat.cullingType = AnimationCullingType.AlwaysAnimate;
                animat.animatePhysics = false;
                animat.wrapMode = WrapMode.Default;

                UIHideAni.Init( animat, "HideAniCallBack", this);
                UIShowAni.Init( animat, "ShowAniCallBack", this);

            }
            override protected void OnDisable()
            {
                base.OnDisable();
            }

            override protected void OnEnable()
            {
                base.OnEnable();
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            #endregion
            #region Call

            public void PlaySound(AudioClip _audio)
            {

            }
            public void PlaySoundByName(string _assets)
            {

            }
            #region ani
            override public void SetActive(bool _active)
            {
                if (gameObject.activeSelf == _active) return;

                if (_active)
                {
                    base.SetActive(_active);
                    UIShowAni.Play();
                }  
                else
                {
                    if (UIHideAni.AniClip != null)
                        UIHideAni.Play();
                    else
                        base.SetActive(_active);
                }              
            }

            public void ShowAniCallBack()
            {
                CallScriptFunctionByName("ShowAniCallBack");
            }

            public void HideAniCallBack()
            {
                CallScriptFunctionByName("HideAniCallBack");
                gameObject.SetActive(false);
            }
            #endregion
            #endregion
        }
    }

     
}
