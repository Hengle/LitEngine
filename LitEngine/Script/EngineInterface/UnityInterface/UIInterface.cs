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
           
            public bool CanPlay { get; private set; }
            public float playbackTime
            {
                get
                {
                    return Mathf.Clamp01(mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                }
            }

            public bool Loop
            {
                get
                {
                    return mAnimator.GetCurrentAnimatorStateInfo(0).loop;
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

            public void SetEnable(bool _active)
            {
                if (enabled == _active) return;
                IsPlaying = _active;
                enabled = _active;
            }

            public bool Update()
            {
                if (!CanPlay)
                {
                    SetEnable(false);
                    return false;
                }
                if (!IsPlaying) return false;
                mAnimator.Update(Time.deltaTime);
                if (IsDone && !Loop)
                    GoToEnd();
                return true;
            }

        }
        public class UIInterface : BehaviourInterfaceBase
        {
            public enum UISate
            {
                Normal = 0,
                ShowAni,
                HideAni,
                enabled,
                disable,
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

                UIAnimator[] tanimators = GetComponents<UIAnimator>();
                if(tanimators.Length > 0)
                {
                    mAniMap = new Dictionary<UIAniType, UIAnimator>();
                    for(int i = 0;i< tanimators.Length; i++)
                    {
                        switch (tanimators[i].Type)
                        {
                            case UIAniType.Hide:
                                tanimators[i].Init(HideAniCallBack);
                                break;
                            case UIAniType.Show:
                                tanimators[i].Init(ShowAniCallBack);
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
                if (!mAniMap.ContainsKey(_type)) return false;
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
                PlayUIAni(UIAniType.Normal);
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
