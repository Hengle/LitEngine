using System;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceOnEnable : ScriptInterfaceCETBase
        {
            #region mymethod
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceOnEnable()
            {

            }
            override public void ClearScriptObject()
            {
                base.ClearScriptObject();
            }
            override protected void InitParamList()
            {
                base.InitParamList();
            }
            #endregion
            #region Unity 
            protected void OnDisable()
            {
                CallScriptFunctionByName("OnDisable");
                UnRegAll();
            }
            protected void OnEnable()
            {
                CallScriptFunctionByName("OnEnable");
                RegAll();
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            
            #endregion
        }
    }
    
}
