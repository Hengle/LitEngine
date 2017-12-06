using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace LitEngine
{
    using UpdateSpace;
    namespace Loader
    {
        public class LoaderManager : ManagerInterface
        {
            
            #region 属性
            private string mAppName = "";//App名字 App数据目录\
            private BundleVector mBundleList = null;
            private LoadTaskVector mBundleTaskList = null;
            private WaitingList mWaitLoadBundleList = null;
            private UpdateObject mUpdateAction = null;
            private bool mIsRegToUpdate = false;
            private AssetBundle mManifestBundle;
            public AssetBundleManifest Manifest { get; private set; }
            #region PATH_LOADER
            private  string mStreamingDataPath = null;
            private  string mResourcesPath = null;
            private  string mPersistentDataPath = null;
            #endregion
            #endregion
            #region 路径获取

            public string GetResourcesDataPath(string _filename)
            {
                return Path.Combine(mResourcesPath, _filename);
            }

            public string GetFullPath(string _filename)
            {
                _filename = BaseBundle.CombineSuffixName(_filename);
                string tfullpathname = Path.Combine(mPersistentDataPath, _filename);
                if (!File.Exists(tfullpathname))
                    tfullpathname = Path.Combine(mStreamingDataPath, _filename);
                return tfullpathname;
            }
            #endregion

            #region 初始化,销毁,设置
            public LoaderManager(string _appname,string _persistenpath,string _streamingpath,string _resources, GameUpdateManager _Updatemanager)
            {
                mPersistentDataPath = _persistenpath;
                mStreamingDataPath = _streamingpath;
                mResourcesPath = string.Format("{0}{1}", _resources, GameCore.ResDataPath).Replace("//", "/");
                SetAppName(_appname);

                mUpdateAction = new UpdateObject(string.Format("{0}:LoaderManager->Update", _appname), Update);
                mUpdateAction.Owner = _Updatemanager.UpdateList;
                mUpdateAction.MaxTime = 0;
                
                mWaitLoadBundleList = new WaitingList();
                mBundleList = new BundleVector();
                mBundleTaskList = new LoadTaskVector();
            }

            ~LoaderManager()
            {
                Dispose(true);//即使是gc释放的也需要释放掉加载的资源
            }
            #region 释放
            protected bool mDisposed = false;
            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }
            protected void Dispose(bool _disposing)
            {
                if (mDisposed)
                    return;

                if (_disposing)
                    DisposeNoGcCode();

                mDisposed = true;
            }
            protected void DisposeNoGcCode()
            {
                if (mManifestBundle != null)
                    mManifestBundle.Unload(true);
                mBundleTaskList.Clear();
                if (mWaitLoadBundleList.Count != 0)
                    DLog.LogError(mAppName +":删除LoaderManager时,发现仍然有未完成的加载动作.请确保加载完成后正确调用.");
                mWaitLoadBundleList.Clear();
                RemoveAllAsset();
                mUpdateAction.Dispose();
                mUpdateAction = null;
            }
            #endregion

            private void SetAppName(string _appname)
            {
                DLog.Log("载入资源列表-"+ _appname);
                mAppName = _appname;
                mManifestBundle = AssetBundle.LoadFromFile(GetFullPath(mAppName));
                if (mManifestBundle != null)
                    Manifest = mManifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                else
                    DLog.LOGColor(DLogType.Warning, "未能加载App资源列表 AppName = " + mAppName, LogColor.RED);
                
            }
            #endregion

            #region update
            private void RegUpdate()
            {
                if (mIsRegToUpdate) return;
                mUpdateAction.RegToOwner();
                mIsRegToUpdate = true;
            }
            private void UnRegUpdte()
            {
                if (!mIsRegToUpdate) return;
                mUpdateAction.UnRegToOwner();
                mIsRegToUpdate = false;
            }
            void Update()
            {
                if (mWaitLoadBundleList.Count > 0)
                {
                    for(int i = mWaitLoadBundleList.Count -1; i >= 0 ; i--)
                    {
                        BaseBundle tbundle = mWaitLoadBundleList[i];
                        if (tbundle.IsDone())
                            mWaitLoadBundleList.Remove(tbundle);
                    }
                }

                if (mBundleTaskList.Count > 0)
                {
                    for (int i = mBundleTaskList.Count - 1; i >= 0; i--)
                    {
                        mBundleTaskList[i].IsDone();
                    }
                }


                if (mWaitLoadBundleList.Count == 0 && mBundleTaskList.Count == 0)
                    ActiveLoader(false);
            }
            #endregion

            #region fun
            void ActiveLoader(bool _active)
            {
                if (mIsRegToUpdate == _active) return;
                if (_active)
                    RegUpdate();
                else
                    UnRegUpdte();
            }
            #endregion

            #region 资源管理

            public string[] GetAllDependencies(string _assetBundleName)
            {
                if (Manifest == null) return null;
                return Manifest.GetAllDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
            }
            public string[] GetDirectDependencies(string _assetBundleName)
            {
                if (Manifest == null) return null;
                return Manifest.GetDirectDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
            }
            public string[] GetAllAssetBundles()
            {
                if (Manifest == null) return null;
                return Manifest.GetAllAssetBundles();
            }

            private void AddmWaitLoadList(BaseBundle _bundle)
            {
                mWaitLoadBundleList.Add(_bundle);
            }

            private void AddCache(BaseBundle _bundle)
            {
                mBundleList.Add(_bundle);
            }

            public void ReleaseAsset(string _key)
            {
                mBundleList.ReleaseBundle(_key);
            }

            private void RemoveAllAsset()
            {
                mBundleList.Clear();
            }

            public void RemoveAsset(string _AssetsName)
            {
                mBundleList.Remove(_AssetsName);
            }

            #endregion
            private LoadTask CreatTaskAndStart(string _key, BaseBundle _bundle, System.Action<string, object> _callback,bool _retain)
            {
                LoadTask ret = new LoadTask(_key, _bundle, _callback, _retain);
                mBundleTaskList.Add(ret);
                return ret;
            }

            #region 资源载入

            #region 同步载入
            #region Res资源
            /// <summary>
            /// 载入Resources资源
            /// </summary>
            /// <param name="_AssetsName">_curPathname 是相对于path/Date/下的路径 例如目录结构Assets/Resources/Date/ +_curPathname</param>
            /// <returns></returns>
            public Object LoadResources(string _AssetsName)
            {
                if (_AssetsName == null || _AssetsName.Equals("")) return null;
                if (mBundleList.Contains(_AssetsName))
                {
                    return (Object)mBundleList[_AssetsName].Retain();
                }

                ResourcesBundle tbundle = new ResourcesBundle(_AssetsName);
                tbundle.Load(this);
                mBundleList.Add(tbundle);
                return (Object)tbundle.Retain();
            }
            #endregion
            //使用前需要设置datapath 默认为 Data _assetname 
            public UnityEngine.Object LoadAsset(string _AssetsName)
            {
                return (UnityEngine.Object)LoadAssetRetain(_AssetsName).Retain();
            }

            private BaseBundle LoadAssetRetain(string _AssetsName)
            {
                if (_AssetsName == null || _AssetsName.Equals("")) return null;

                if (!mBundleList.Contains(_AssetsName))
                {
                    AssetsBundleHaveDependencie tbundle = new AssetsBundleHaveDependencie(_AssetsName, LoadAssetRetain);
                    tbundle.Load(this);
                    AddCache(tbundle);
                }
                return mBundleList[_AssetsName];
            }
            #endregion
            #region 异步载入

            protected void LoadBundleAsync(BaseBundle _bundle,string _key, System.Action<string, object> _callback,bool _retain)
            {
                _bundle.Load(this);
                AddmWaitLoadList(_bundle);
                AddCache(_bundle);
                CreatTaskAndStart(_key, _bundle, _callback, _retain);
                ActiveLoader(true);
            }

            public void LoadResourcesAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
            {
                if (_AssetsName.Length == 0)
                {
                    DLog.LogError("LoadResourcesBundleByRelativePathNameAsync -- _AssetsName 的长度不能为空");
                }
                if (_callback == null)
                {
                    DLog.LogError("LoadResourcesBundleByRelativePathNameAsync -- CallBack Fun can not be null");
                    return;
                }

                if (mBundleList.Contains(_AssetsName))
                {
                    if (mBundleList[_AssetsName].Loaded)
                    {
                        if (mBundleList[_AssetsName].Asset == null)
                            DLog.LogError("ResourcesBundleAsync-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);
    
                        mBundleList[_AssetsName].Retain();
                        _callback(_key, mBundleList[_AssetsName].Asset);
                    }
                    else
                    {
                        CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback,true);
                        ActiveLoader(true);
                    }

                }
                else
                {
                    LoadBundleAsync(new ResourcesBundleAsync(_AssetsName), _key, _callback,true);
                }
            }
            
            public void LoadAssetAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
            {
                 LoadAssetAsyncRetain(_key, _AssetsName, _callback,true);
            }

            private BaseBundle LoadAssetAsyncRetain(string _key, string _AssetsName, System.Action<string, object> _callback, bool _retain )
            {
                if (_AssetsName.Length == 0)
                {
                    DLog.LogError( "LoadAssetsBundleByFullNameAsync -- _AssetsName 的长度不能为空");
                }
                if (_callback == null)
                {
                    DLog.LogError( "LoadAssetsBundleByFullNameAsync -- CallBack Fun can not be null");
                    return null;
                }
                _AssetsName = BaseBundle.DeleteSuffixName(_AssetsName);

                if (mBundleList.Contains(_AssetsName))
                {
                    if (mBundleList[_AssetsName].Loaded)
                    {
                        if (mBundleList[_AssetsName].Asset == null)
                            DLog.LogError( "AssetsBundleAsyncFromFile-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);
                        if (_retain)
                            mBundleList[_AssetsName].Retain();
                        _callback(_key, mBundleList[_AssetsName].Asset);

                    }
                    else
                    {
                        CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback, _retain);
                        ActiveLoader(true);
                    }

                }
                else
                {

                    LoadBundleAsync(new AssetsBundleHaveDependencieAsync(_AssetsName, LoadAssetAsyncRetain), _key, _callback, _retain);
                }
                return mBundleList[_AssetsName];
            }
            #endregion
            #region WWW载入
            public BaseBundle WWWLoad(string _key, string _FullName, System.Action<string, object> _callback)
            {
                if (_callback == null)
                {
                    DLog.LogError("assetsbundle -- CallBack Fun can not be null");
                    return null;
                }

                if (mBundleList.Contains(_FullName))
                {
                    if (mBundleList[_FullName].Loaded)
                    {
                        if (mBundleList[_FullName].Asset == null)
                            DLog.LogError("WWWLoad-erro in vector。文件载入失败,请检查文件名:" + _FullName);
                        mBundleList[_FullName].Retain();
                        _callback(_key, mBundleList[_FullName].Asset);
                    }
                    else
                    {
                        CreatTaskAndStart(_key, mBundleList[_FullName], _callback,true);
                        ActiveLoader(true);
                    }

                }
                else
                {
                    LoadBundleAsync(new WWWBundle(_FullName), _key, _callback,true);
                }
                return mBundleList[_FullName];
            }
            #endregion
            #endregion
        }

    }
}

