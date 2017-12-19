using UnityEngine;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class UIInterface : BehaviourInterfaceBase
        {
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

            public void ShowAniCallBack()
            {

            }

            public void HideAniCallBack()
            {

            }
            #endregion
        }
    }

     
}
