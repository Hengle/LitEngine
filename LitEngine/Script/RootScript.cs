using UnityEngine;
namespace LitEngine
{
    public class RootScript : MonoBehaviour
    {
        public string APPNAME = "";
        public string ScriptFileName = "";
        public string StartClass = "";
        public string StartFun = "";
        private object mMainObject = null;
        private GameCore mCore = null;
        private bool mInited = false;
        protected void Awake()
        {
            InitScript();
        }

        public void InitScript()
        {
            if (mInited) return;
            if (string.IsNullOrEmpty(APPNAME)) return;
            mInited = true;
            gameObject.name = "Root-" + APPNAME;
            mCore = AppCore.CreatGameCore(APPNAME);
            mCore.DontDestroyOnLoad(gameObject);
            if (!string.IsNullOrEmpty(ScriptFileName))
                LoadScriptFormFile(ScriptFileName);
        }

        public void StartMain(string _mainClass,string _fun)
        {
            if (mMainObject != null)
            {
                DLog.LogError("MainObject 重复创建.");
                return;
            }
            if (!string.IsNullOrEmpty(_mainClass))
                mMainObject = mCore.SManager.CodeTool.GetCSLEObjectParmas(_mainClass);
            if (!string.IsNullOrEmpty(_fun))
                mCore.SManager.CodeTool.CallMethodByName(_fun, mMainObject);
        }

        public void LoadScriptFormFile(string _filename)
        {
            if (mCore.SManager.ProjectLoaded)
            {
                DLog.LogError("脚本不可重复加载.");
                return;
            }
                
            string tdllPath = mCore.AppPersistentScriptDataPath + _filename;
            if (System.IO.File.Exists(tdllPath + ".dll"))
            {
                mCore.SManager.LoadProject(tdllPath);
                StartMain(StartClass, StartFun);
            }
        }

        public void LoadScriptFormMemory(byte[] _dllbytes,byte[] _pdbbytes)
        {
            if (mCore.SManager.ProjectLoaded)
            {
                DLog.LogError("脚本不可重复加载.");
                return;
            }
            if (_dllbytes != null && _pdbbytes != null)
            {
                mCore.SManager.LoadProjectByBytes(_dllbytes, _pdbbytes);
                StartMain(StartClass, StartFun);
            }
        }
    }
}
