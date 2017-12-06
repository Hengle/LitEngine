using System;
namespace LitEngine
{
    using UpdateSpace;
    using DownLoad;
    using UnZip;
    public class PublicUpdateManager : MonoManagerBase
    {
        private static PublicUpdateManager sInstance = null;
        private UpdateObjectVector UpdateList = new UpdateObjectVector(UpdateType.Update);
        static private void CreatInstance()
        {
            if (sInstance == null)
            {
                UnityEngine.GameObject tobj = new UnityEngine.GameObject("PublicUpdateManager");
                UnityEngine.GameObject.DontDestroyOnLoad(tobj);
                sInstance = tobj.AddComponent<PublicUpdateManager>();
                tobj.SetActive(false);
            }
        }

        static public void AddUpdate(UpdateBase _uobj)
        {
            if (sInstance == null) CreatInstance();
            sInstance.UpdateList.Add(_uobj);
        }

        static public void AddUpdate(string _key,System.Action _act)
        {
            if (sInstance == null) CreatInstance();
            UpdateBase tobj = new UpdateObject(_key, _act);
            sInstance.UpdateList.Add(tobj);
        }

        static public void ClearByKey(string _appkey)
        {
            if(sInstance != null)
                sInstance.UpdateList.ClearByKey(_appkey);
        }

        static public void DownLoadFileAsync(string _AppName, string _sourceurl, string _destination, bool _IsClear, Action<string, string> _finished, Action<long, long, float> _progress)
        {
            if (sInstance == null) CreatInstance();
            if (DownLoadTask.DownLoadFileAsync(sInstance.UpdateList, _AppName, _sourceurl, _destination, _IsClear, _finished, _progress))
                sInstance.SetActive(true);
        }

        static public void UnZipFileAsync(string _appname, string _source, string _destination, Action<string> _finished, Action<float> _progress)
        {
            if (sInstance == null) CreatInstance();
            if (UnZipTask.UnZipFileAsync(sInstance.UpdateList, _appname, _source, _destination, _finished, _progress))
                sInstance.SetActive(true);
        }

        override protected void OnDestroy()
        {
            sInstance = null;
            UpdateList.Clear();
            base.OnDestroy();
        }

        public PublicUpdateManager()
        {

        }

        protected void SetActive(bool _active)
        {
            if (gameObject.activeSelf != _active)
                gameObject.SetActive(_active);
        }

        void Update()
        {
            UpdateList.Update();
            if (UpdateList.Count == 0)
                gameObject.SetActive(false);
        }
    }
}
