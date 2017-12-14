using UnityEngine;
using System.Collections.Generic;
namespace LitEngine
{
    using Loader;
    public class GameCore : CoreBase
    {
        public const string DataPath = "/Data/";//App总数据目录
        public const string ResDataPath = "/ResData/";//App资源目录
        public const string ConfigDataPath = "/ConfigData/";//App配置文件目录
        public const string ScriptDataPath = "/LogicDll/";//App配置文件目录
        #region static path获取
        static public string GetPersistentAppPath(string _appname)
        {
            return CombinePath(AppCore.persistentDataPath, DataPath, _appname);
        }
        static public string GetStreamingAssetsAppPath(string _appname)
        {
            return CombinePath(AppCore.streamingAssetsPath, DataPath, _appname);
        }
        static public string CombinePath(params object[] _params)
        {
            for (int i = 0; i < _params.Length; i++)
            {
                string tobjstr = _params[i].ToString();
                if(i!=0)
                    tobjstr = RemoveStartWithString(tobjstr, "/");
                tobjstr = RemoveEndWithString(tobjstr, "/");
                _params[i] = tobjstr;
            }

            System.Text.StringBuilder tformatbuilder = new System.Text.StringBuilder();
            for (int i = 0; i < _params.Length; i++)
            {
                tformatbuilder.Append("{");
                tformatbuilder.Append(i);
                tformatbuilder.Append("}/");
            }

            return string.Format(tformatbuilder.ToString(), _params);
        }

        static public string RemoveEndWithString(string _source,string _des)
        {
            while (_source.EndsWith(_des))
                _source = _source.Remove(_source.Length - _des.Length);
            return _source;
        }

        static public string RemoveStartWithString(string _source, string _des)
        {
            while (_source.StartsWith(_des))
                _source = _source.Remove(0,_des.Length);
            return _source;
        }

        #endregion
        #region 类变量
        private AppCore mParentCore;
        protected bool mIsInited = false;
        public string AppName
        {
            get;
            private set;
        }
        public string AppPersistentDataPath { get; private set; }
        public string AppStreamingAssetsDataPath { get; private set; }
        public string AppResourcesDataPath { get; private set; }

        public string AppPersistentResDataPath { get; private set; }
        public string AppStreamingAssetsResDataPath { get; private set; }

        public string AppPersistentConfigDataPath { get; private set; }
        public string AppStreamingAssetsConfigDataPath { get; private set; }

        public string AppPersistentScriptDataPath { get; private set; }
        public string AppStreamingAssetsScriptDataPath { get; private set; }

        private List<UnityEngine.GameObject> mDontDestroyList = new List<UnityEngine.GameObject>();
        private List<ScriptInterface.BehaviourInterfaceBase> mScriptInterfaces = new List<ScriptInterface.BehaviourInterfaceBase>();
        #endregion

        #region 管理器
        public ScriptManager SManager
        {
            get;
            private set;
        }
        public LoaderManager LManager
        {
            get;
            private set;
        }
        public GameUpdateManager GManager
        {
            get;
            private set;
        }

        public CoroutineManager CManager
        {
            get;
            private set;
        }
        #endregion
        #region 初始化
        protected GameCore()
        {

        }
        protected GameCore(AppCore _core,string _appname)
        {
            AppName = _appname;
            mParentCore = _core;
        }
        private void InitGameCore(UseScriptType _scripttype)
        {
            if(!CheckInited(false))
            {
                DLog.LogError( "不允许重复初始化GameCore,请检查代码");
                return;
            }
            SetPath();

            GameObject tobj = new GameObject("GameUpdateManager-" + AppName);
            GameObject.DontDestroyOnLoad(tobj);
            GManager = tobj.AddComponent<GameUpdateManager>();

            tobj = new GameObject("CoroutineManager-" + AppName);
            GameObject.DontDestroyOnLoad(tobj);
            CManager = tobj.AddComponent<CoroutineManager>();

            SManager = new ScriptManager(AppName,_scripttype);
            LManager = new LoaderManager(AppName, AppPersistentResDataPath, AppStreamingAssetsResDataPath, AppResourcesDataPath, GManager);

            mIsInited = true;
        }

        public bool CheckInited(bool _need)
        {
            if (mIsInited != _need)
            {
                DLog.LogError( string.Format("GameCore的初始化状态不正确:Inited = {0} needstate = {1}", mIsInited, _need));
                return false;
            }
            return true ;
        }


        #endregion
        #region 释放

        override protected void DisposeNoGcCode()
        {
            //公用
            PublicUpdateManager.ClearByKey(AppName);
            NetTool.HttpNet.ClearByKey(AppName);


            for (int i = mScriptInterfaces.Count - 1; i >= 0; i--)
            {
                ScriptInterface.BehaviourInterfaceBase tscript = mScriptInterfaces[i];
                if (tscript == null) continue;
                if (!tscript.mAppName.Equals(AppName)) continue;
                tscript.ClearScriptObject();
            }
            mScriptInterfaces.Clear();

            for (int i = mDontDestroyList.Count - 1;i >= 0;i--)
            {
                Destroy(mDontDestroyList[i]);
            }
            mDontDestroyList.Clear();

            GManager.DestroyManager();
            CManager.DestroyManager();
            LManager.Dispose();
            SManager.Dispose();

            GManager = null;
            LManager = null;
            SManager = null;
            CManager = null;
        }
        #endregion
        #region 方法
        private void SetPath()
        {
            AppResourcesDataPath = CombinePath(DataPath, AppName);
            AppPersistentDataPath = GetPersistentAppPath(AppName);
            AppStreamingAssetsDataPath = GetStreamingAssetsAppPath(AppName);

            AppPersistentResDataPath = CombinePath(AppPersistentDataPath, ResDataPath);
            AppStreamingAssetsResDataPath = CombinePath(AppStreamingAssetsDataPath, ResDataPath);

            AppPersistentConfigDataPath = CombinePath(AppPersistentDataPath, ConfigDataPath);
            AppStreamingAssetsConfigDataPath = CombinePath(AppStreamingAssetsDataPath, ConfigDataPath);

            AppPersistentScriptDataPath = CombinePath(AppPersistentDataPath, ScriptDataPath);
            AppStreamingAssetsScriptDataPath = CombinePath(AppStreamingAssetsDataPath, ScriptDataPath);
        }

        public void AddScriptInterface(ScriptInterface.BehaviourInterfaceBase _scriptinterface)
        {
            if (mScriptInterfaces.Contains(_scriptinterface)) return;
            mScriptInterfaces.Add(_scriptinterface);
        }

        public void RemveScriptInterface(ScriptInterface.BehaviourInterfaceBase _scriptinterface)
        {
            if (!mScriptInterfaces.Contains(_scriptinterface)) return;
            mScriptInterfaces.Remove(_scriptinterface);
        }

        public void DontDestroyOnLoad(UnityEngine.GameObject _obj)
        {
            UnityEngine.Object.DontDestroyOnLoad(_obj);
            if(!mDontDestroyList.Contains(_obj))
                mDontDestroyList.Add(_obj);
        }
        public void DestroyObject(UnityEngine.GameObject _obj,float _t)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.DestroyObject(_obj, _t);
        }
        public void DestroyImmediate(UnityEngine.GameObject _obj)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.DestroyImmediate(_obj);
        }

        public void Destroy(UnityEngine.GameObject _obj)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.Destroy(_obj);
        }
        #endregion
        #region Tool方法
        public void DownLoadFileAsync(string _sourceurl, string _destination, bool _IsClear, System.Action<string, string> _finished, System.Action<long, long, float> _progress)
        {
            PublicUpdateManager.DownLoadFileAsync(AppName, _sourceurl, _destination, _IsClear, _finished, _progress);
        }

        public void UnZipFileAsync(string _source, string _destination,System.Action<string> _finished, System.Action<float> _progress)
        {
            PublicUpdateManager.UnZipFileAsync(AppName, _source, _destination, _finished, _progress);
        }

        public void HttpSend(string _url, string _key, System.Action<string,string, byte[]> _delegate)
        {
            NetTool.HttpNet.Send(AppName, _url, _key, _delegate);
        }

        public void HttpSendHaveHeader(string _url, string _key,Dictionary<string,string> _headers, System.Action<string,string, byte[]> _delegate)
        {
            NetTool.HttpData tdata = new NetTool.HttpData(AppName, _key, _url, _delegate);

            if (tdata.Request.Headers == null) tdata.Request.Headers = new System.Net.WebHeaderCollection();
            foreach (KeyValuePair<string, string> tkey in _headers)
            {
                tdata.Request.Headers.Add(tkey.Key, tkey.Value);
            }

            NetTool.HttpNet.SendData(tdata);
        }
        #endregion
    }
}
