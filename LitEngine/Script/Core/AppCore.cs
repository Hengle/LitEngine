﻿using System;
namespace LitEngine
{
    public class CoreBase : IDisposable
    {
        protected bool mDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            if(_disposing)
                DisposeNoGcCode();
            mDisposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {

        }

        ~CoreBase()
        {
            Dispose(false);
        }
    }
    public class AppCore : CoreBase
    {
        #region 静态函数及变量
        #region 变量
        public static UseScriptType sUseScriptType = UseScriptType.UseScriptType_LS;
        private static AppCore sInstance = null;

        private SafeMap<string, GameCore> mGameMap = new SafeMap<string, GameCore>();
        #endregion
        #region 方法
        static private string sPersistentDataPath = null;
        static public string persistentDataPath
        {
            get
            {
                if (sPersistentDataPath == null)
                {
                    if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor && UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsPlayer)
                        sPersistentDataPath = string.Format("{0}/", UnityEngine.Application.persistentDataPath).Replace("//", "/");
                    else
                        sPersistentDataPath = string.Format("{0}", UnityEngine.Application.dataPath + "/../").Replace("//", "/");
                }
                return sPersistentDataPath;
            }
        }
        static public string streamingAssetsPath
        {
            get
            {
                return UnityEngine.Application.streamingAssetsPath;
            }
        }
        public string AppStreamingAssetsDataPath { get; private set; }
        public static AppCore App
        {
            get
            {
                if (sInstance == null)
                    sInstance = new AppCore();
                return sInstance;
            }
        }

        private static GameCore GetGameCore(AppCore _app, string _appname)
        {
            System.Type ttype = typeof(GameCore);
            System.Reflection.ConstructorInfo[] constructorInfoArray = ttype.GetConstructors(System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Public);
            System.Reflection.ConstructorInfo noParameterConstructorInfo = null;

            foreach (System.Reflection.ConstructorInfo constructorInfo in constructorInfoArray)
            {
                System.Reflection.ParameterInfo[] parameterInfoArray = constructorInfo.GetParameters();
                if (2 == parameterInfoArray.Length)
                {
                    noParameterConstructorInfo = constructorInfo;
                    break;
                }
            }
            if (null == noParameterConstructorInfo)
            {
                throw new System.NotSupportedException("No constructor without 2 parameter");
            }

            object[] tparams = { _app, _appname };
            return (GameCore)noParameterConstructorInfo.Invoke(tparams);

        }

        public static GameCore AppChange(string _nowapp,string _nextapp,string _startscene)
        {
            DestroyGameCore(_nowapp);
            GameCore ret =  CreatGameCore(_nextapp);
            ret.LManager.LoadAsset(_startscene);
            string tscenename = _startscene.Replace(".unity","");
            UnityEngine.SceneManagement.SceneManager.LoadScene(tscenename);
            return ret;
        }

        public static GameCore CreatGameCore(string _appname)
        {
            if (App.ContainsKey(_appname)) return App[_appname];
            return App.AddGameCore(_appname);
        }

        public static void DestroyGameCore(string _appname)
        {
            App.DestroyGame(_appname);
        }
        #endregion
        #endregion

        #region 类方法

        #region private
        protected AppCore()
        {

        }
        private GameCore AddGameCore(string _appname)
        {
            if (mGameMap.ContainsKey(_appname))
                throw new System.IndexOutOfRangeException("重复创建 GameCore. _appname = " + _appname);
            
            GameCore ret = GetGameCore(this, _appname);

            System.Type ttype = ret.GetType();
            System.Reflection.MethodInfo tmethod = ttype.GetMethod("InitGameCore", System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Public);

            object[] tparms = { sUseScriptType };
            tmethod.Invoke(ret, tparms);

            mGameMap.Add(_appname, ret);
            return ret;
        }
        private void DestroyGame(string _key)
        {
            if (!mGameMap.ContainsKey(_key))
                throw new System.NullReferenceException("试图摧毁一个不存在的 GameCore. _appname = " + _key);

            DLog.LogWarning( "准备释放一个GameCore,同时摧毁App相关所有UnityInterface. AppName = " + _key);
            mGameMap[_key].Dispose();
            mGameMap.Remove(_key);
            DLog.LogWarning("释放GameCore结束. AppName = " + _key);
        }

        override protected void DisposeNoGcCode()
        {
            DLog.LogWarning("准备释放AppCore.");
            System.Collections.Generic.List<string> tlist = new System.Collections.Generic.List<string>(mGameMap.Keys);
            foreach(string tkey in tlist)
            {
                DestroyGame(tkey);
            }
            base.DisposeNoGcCode();
        }
        #endregion
        #region Public
        public bool ContainsKey(string _key)
        {
            return mGameMap.ContainsKey(_key);
        }

        public GameCore this[string _key]
        {
            get
            {
                if (ContainsKey(_key))
                {
                    if (!mGameMap[_key].CheckInited(true))
                        DLog.LogError( "调用了未经初始化的 GameCore.需调用InitGameCore");
                    return mGameMap[_key];
                }
                else
                    throw new System.NullReferenceException("未能取得对应的GameCore key=" + _key);

            }
        }
        #endregion
        #endregion

    }
}
