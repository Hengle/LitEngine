using UnityEngine;
namespace LitEngine
{
    public class RootScript : MonoBehaviour
    {
        public string APPNAME = "AppName,应于AppConfig中的配置保持一致.";
        public string ScriptFileName = "脚本文件名不带后缀";
        public string StartClass = "启动类名";
        public string StartFun = "启动方法名";
        private object mMainObject = null;
        private GameCore mCore = null;
        
        protected void Awake()
        {
            gameObject.name = "Root-" + APPNAME;
            mCore = AppCore.CreatGameCore(APPNAME);
            mCore.DontDestroyOnLoad(gameObject);

            LoadScriptFormFile(ScriptFileName);
        }

        public void StartMain(string _mainClass)
        {
            mMainObject = mCore.SManager.CodeTool.GetCSLEObjectParmas(_mainClass);
            mCore.SManager.CodeTool.CallMethodByName(StartFun, mMainObject);
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
                StartMain(StartClass);
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
                StartMain(StartClass);
            }
        }
    }
}
