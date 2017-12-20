using UnityEngine;
using System.Collections.Generic;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public enum UIAniType
        {
            None = 0,
            Show,
            Hide,
            ScriptFun,
        }
        public class AnimatorGroup
        {
            public Animator animator;
            public int Count { get; private set; }
            private AnimatorGroup()
            {

            }
            public AnimatorGroup(Animator _ator)
            {
                if(_ator == null)
                    throw new System.NullReferenceException("AnimatorGroup,初始化参数不可为null.");
                animator = _ator;
                animator.enabled = false;
            }
            public void Retain()
            {
                Count++;
                if (!animator.enabled)
                    animator.enabled = true;
            }
            public void Release()
            {
                Count--;
                if (Count <= 0)
                {
                    Count = 0;
                    animator.enabled = false;
                }
                   
            }

            public void ReleaseLoop()
            {
                Count--;
                if (Count <= 0)
                    Count = 0;
            }

            public bool enabled
            {
                get { return animator.enabled;}
            }
        }
        public class UIAnimator : MonoBehaviour
        {
            public UIAniType Type = UIAniType.None;
            public string State;
            public bool isReverse = false;
            public string ScrintFun;
            public bool IsPlaying { get; private set; }
            protected AnimatorGroup mAnimator;
            protected System.Action mEndCallback;
            protected bool mPlayEnd = true;
            protected bool mStoped = false;
            
            public float playbackTime
            {
                get
                {
                    AnimatorStateInfo state = mAnimator.animator.GetCurrentAnimatorStateInfo(0);
                    return Mathf.Clamp01(state.normalizedTime);
                }
            }

            public bool IsDone
            {
                get
                {
                    return playbackTime == 1f;
                }
            }
            private void Awake()
            {
                
            }
            public void Init(AnimatorGroup _anigroup,System.Action _action)
            {
                if (string.IsNullOrEmpty(State)) return;
                mAnimator = _anigroup;
                mEndCallback = _action;
            }

            public bool Play()
            {
                if (IsPlaying) return true;
                if ( mAnimator == null || mAnimator.animator == null) return false;
                mPlayEnd = false;
                SetEnable(true);
                mAnimator.animator.Stop();
                mAnimator.animator.Rebind();
                mAnimator.animator.Play(State, 0);
                IsPlaying = true;
                return true;
            }

            public void Stop()
            {
                if (!IsPlaying) return;
                mAnimator.animator.Stop();
                IsPlaying = false;
                SetEnable(false);
                if (mEndCallback != null)
                    mEndCallback();
            }

            public void SetEnable(bool _active)
            {
                if (mAnimator == null || mAnimator.animator == null) return;
                if (enabled == _active) return;
                enabled = _active;
                if (_active)
                    mAnimator.Retain();
                else
                    mAnimator.Release();
            }

            public bool Update()
            {
                if (mPlayEnd) return false;
                
                if (IsDone)
                    Stop();

                return true;
            }

        }
        public class UIInterface : BehaviourInterfaceBase
        {
            protected Dictionary<UIAniType, UIAnimator> mAniMap;
            protected AnimatorGroup mAniGroup;
            protected UIAnimator mCurAni;
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

                UIAnimator[] tanimators = GetComponents<UIAnimator>();
                if(tanimators.Length > 0)
                {
                    mAniGroup = new AnimatorGroup(GetComponent<Animator>());
                    mAniMap = new Dictionary<UIAniType, UIAnimator>();
                    for(int i = 0;i< tanimators.Length; i++)
                    {
                        switch(tanimators[i].Type)
                        {
                            case UIAniType.Hide:
                                tanimators[i].Init(mAniGroup, HideAniCallBack);
                                break;
                            case UIAniType.Show:
                                tanimators[i].Init(mAniGroup, ShowAniCallBack);
                                break;
                            case UIAniType.ScriptFun:
                                System.Action tback = null;
                                if(mCodeTool != null)
                                    tback = mCodeTool.GetCSLEDelegate<System.Action>(tanimators[i].ScrintFun, mScriptType, ScriptObject);                          
                                tanimators[i].Init(mAniGroup, tback);
                                break;
                        }
                        tanimators[i].enabled = false;
                        mAniMap.Add(tanimators[i].Type, tanimators[i]);
                    }
                }
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
                mAniMap.Clear();
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
            protected bool PlayUIAni(UIAniType _type)
            {
                if (!mAniMap.ContainsKey(_type)) return false;
                if (mCurAni != null) mCurAni.Stop();
                mCurAni = mAniMap[_type];
                return mCurAni.Play();
            }
            override public void SetActive(bool _active)
            {
                if (gameObject.activeInHierarchy == _active) return;
                if (_active)
                {
                    base.SetActive(_active);
                    PlayUIAni(UIAniType.Show);
                }  
                else
                {
                    
                    if (!PlayUIAni(UIAniType.Hide))
                    {
                        base.SetActive(_active);
                        HideAniCallBack();
                    }
                         
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
