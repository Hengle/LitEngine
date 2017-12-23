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
            Normal,
        }
        public class UIAnimator : MonoBehaviour
        {
            public UIAniType Type = UIAniType.None;
            public string State;
            public bool IsPlaying { get; private set; }
            protected Animator mAnimator;
            protected System.Action mEndCallback;
            protected AnimatorClipInfo[] mClips = null;
            protected AnimatorStateInfo mState;
            protected bool mRestDate = true;

            public bool CanPlay { get; private set; }

            protected bool IsDone
            {
                get
                {
                    if(mRestDate)
                    {
                        mClips = mAnimator.GetCurrentAnimatorClipInfo(0);
                        mState = mAnimator.GetCurrentAnimatorStateInfo(0);
                        mRestDate = false;
                    }
                    if (mClips == null || mClips.Length == 0) return true;
                    
                    float ttime = Mathf.Clamp01(mState.normalizedTime);
                    return !mState.loop && ttime == 1f;
                }
            }
            private void Awake()
            {
                if (string.IsNullOrEmpty(State)) return;
                CanPlay = false;
                mAnimator = GetComponent<Animator>();
                if(mAnimator != null)
                {
                    if (mAnimator.enabled)
                        mAnimator.enabled = false;

                    int hashid = Animator.StringToHash(State);
                    if (!mAnimator.HasState(0, hashid))
                        mAnimator = null;
                    else
                        CanPlay = true;
                }
            }
            public void Init(System.Action _action)
            {
                mEndCallback = _action;   
            }

            public bool Play()
            {
                if (!CanPlay) return false;
                if (IsPlaying) return true;
                mAnimator.enabled = false;
                mAnimator.Stop();
                mAnimator.Rebind();
                mAnimator.Play(State, 0);
                mRestDate = true;
                SetEnable(true);
                return true;
            }

            public void Stop()
            {
                if (!CanPlay) return;         
                mAnimator.Stop();
                SetEnable(false);
            }

            public void GoToEnd()
            {
                Stop();
                if (mEndCallback != null)
                    mEndCallback();
            }

            protected void SetEnable(bool _active)
            {
                if (enabled == _active) return;
                IsPlaying = _active;
                enabled = _active;
            }

            protected bool LateUpdate()
            {
                if (!CanPlay)
                {
                    SetEnable(false);
                    return false;
                }
                if (!IsPlaying) return false;
                
                mAnimator.Update(Time.deltaTime);
                if (IsDone)
                    GoToEnd();
                return true;
            }

        }
        public class UIInterface : BehaviourInterfaceBase
        {
            public enum UISate
            {
                Normal = 0,
                Showing,
                Hidden,
            }
            protected UISate mState = UISate.Normal;
            protected Dictionary<UIAniType, UIAnimator> mAniMap;
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
                Animator tanitor = GetComponent<Animator>();
                if (tanitor != null)
                {
                    tanitor.Stop();
                    tanitor.Rebind();
                    tanitor.enabled = false;

                }

                UIAnimator[] tanimators = GetComponents<UIAnimator>();
                if (tanimators.Length > 0)
                {
                    mAniMap = new Dictionary<UIAniType, UIAnimator>();
                    for (int i = 0; i < tanimators.Length; i++)
                    {
                        switch (tanimators[i].Type)
                        {
                            case UIAniType.Hide:
                                tanimators[i].Init(OnHideAnimationEnd);
                                break;
                            case UIAniType.Show:
                                tanimators[i].Init(OnShowAnimationEnd);
                                break;
                            case UIAniType.Normal:
                                tanimators[i].Init(OnNormalAnimationEnd);
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
                mCurAni = null;
                if(mAniMap != null)
                    mAniMap.Clear();
                base.OnDestroy();
            }
            #endregion
            #region 调用脚本函数
            override public object CallScriptFunctionByNameParams(string _FunctionName, params object[] _prams)
            {
                if (mState != UISate.Normal) return null;
                return base.CallScriptFunctionByNameParams(_FunctionName, _prams);
            }
            #endregion
            #region Call
            public void PlaySound(AudioClip _audio)
            {
                PlayAudioManager.Play(_audio);
            }
            #region ani
            protected bool PlayUIAni(UIAniType _type)
            {
                if (mAniMap == null) return false;
                if (!mAniMap.ContainsKey(_type)) return false;
                if (!mAniMap[_type].CanPlay) return false;
                if (mCurAni != null && mCurAni.Type == _type && mCurAni.IsPlaying) return true;
                if(mCurAni != null)
                    mCurAni.Stop();
                mCurAni = mAniMap[_type];
                return mCurAni.Play();
            }
            override public void SetActive(bool _active)
            {
                if (_active)
                {
                    base.SetActive(true);
                    mState = UISate.Showing;
                    if (!PlayUIAni(UIAniType.Show))
                    {
                        OnShowAnimationEnd();
                    }
                }  
                else
                {
                    if (!gameObject.activeInHierarchy) return;

                    mState = UISate.Hidden;
                    if (!PlayUIAni(UIAniType.Hide))
                    {
                        base.SetActive(false);
                        OnHideAnimationEnd();
                    }
                         
                }              
            }

            protected void OnShowAnimationEnd()
            {
                PlayUIAni(UIAniType.Normal);
                mState = UISate.Normal;
                CallScriptFunctionByNameParams("OnShowAnimationEnd");
            }

            protected void OnHideAnimationEnd()
            {
                mState = UISate.Normal;
                CallScriptFunctionByNameParams("OnHideAnimationEnd");
                base.SetActive(false);
            }

            protected void OnNormalAnimationEnd()
            {
                CallScriptFunctionByNameParams("OnNormalAnimationEnd");
            }

            #endregion
            #endregion
        }
    }

     
}
